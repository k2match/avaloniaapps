using System;
using System.Collections.Generic;
using System.IO;
using Common.IO;
using Microsoft.Data.Sqlite;
using k2audio.Models.Audio;

namespace k2audio.Models;

internal class Catalog : IDisposable
{
	//========================================
	// コンストラクタ
	public Catalog(string path)
	{
		this.Path = path;
		this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
		this.BaseDirectory = string.Empty;
	}

	~Catalog()
	{
		this.Dispose();
	}

	public void Dispose()
	{
		if (_conn is not null)
		{
			_conn.Close();
			_conn.Dispose();
			_conn = null;
		}

		GC.SuppressFinalize(this);
	}

	public static Catalog Open(string path)
	{
		Catalog catalog = new(path);
		catalog.OpenDB();
		return catalog;
	}

	public static Catalog Create(string path, string baseDirectory)
	{
		Catalog catalog = new(path);
		catalog.CreateDB(baseDirectory);
		catalog.Scan();
		return catalog;
	}

	//========================================
	// プロパティ
	public string Path { get; }
	public string Name { get; }
	public string BaseDirectory { get; private set; }
	private SqliteConnection? _conn = null;
	private SqliteConnection Connection
	{
		get
		{
			if (_conn is not null) return _conn;
			_conn = new SqliteConnection($"Data Source={this.Path}");
			System.Console.WriteLine($"Connect DB {_conn.DataSource}");
			_conn.Open();
			return _conn;
		}
	}

	//========================================
	// ファイル取得
	public AudioFileCollection GetFiles()
	{
		return GetFiles(null);
	}

	//========================================
	// ファイルスキャン
	private void Scan()
	{
		// DBからディレクトリを取得してファイルシステム上に存在しなければ削除
		DeleteRemovedDirectory();

		string[] dirs = Directory.GetDirectories(this.BaseDirectory, "*", SearchOption.AllDirectories);
		foreach (string dirPath in dirs)
		{
			string? dir = dirPath.ToRelative(this.BaseDirectory);
			if (dir is null) continue;

			// ディレクトリのIDをセレクト（またはインサート）
			int? dirID = GetDirectoryID(dir, true);
			if (dirID is null) continue;

			// ディレクトリ内をスキャン
			ScnaDirectory(dirPath, (int)dirID);
		}
	}

	private void ScnaDirectory(string dirPath, int dirID)
	{
		AudioFileCollection dbFiles = GetFiles(dirID);

		string[] files = Directory.GetFiles(dirPath);

		foreach (string filePath in files)
		{
			// サポート外の拡張子はスキップ
			if (!AudioPlayer.IsSupported(filePath)) continue;

			FileInfo fi = new(filePath);
			AudioFile? exists = dbFiles.GetFile(dirID, fi.Name);
			if (exists is not null)
			{
				// DBにすでに登録されていて、サイズ・タイムスタンプが同一であればスキップ
				if ((exists.Size == fi.Length) &&
					(exists.LastWrite == fi.LastWriteTimeUtc.ToFileTimeUtc())) continue;
			}

			// ファイル、TAG情報をDBに保存
			AudioFile af = new(this, dirID, fi);
			this.Save(af);
		}
	}

	//========================================
	// DB
	private void CreateDB(string baseDirectory)
	{
		SqliteConnection conn = this.Connection;
		this.BaseDirectory = baseDirectory;

		// Properties テーブル
		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS Properties (
				Name		TEXT		PRIMARY KEY		NOT NULL,
				Value		TEXT						NOT NULL
			);";
		cmd.ExecuteNonQuery();
		cmd = conn.CreateCommand();
		cmd.CommandText = "INSERT INTO Properties (Name, Value) VALUES ($name, $value);";
		cmd.Parameters.AddWithValue("$name", "BaseDirectory");
		cmd.Parameters.AddWithValue("$value", baseDirectory);
		cmd.ExecuteNonQuery();

		// Directories テーブル
		cmd = conn.CreateCommand();
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS Directories (
				ID			INTEGER		PRIMARY KEY		AUTOINCREMENT,
				Path		TEXT		UNIQUE			NOT NULL
			);";
		cmd.ExecuteNonQuery();

		// Files テーブル
		cmd = conn.CreateCommand();
		cmd.CommandText = @"
			CREATE TABLE IF NOT EXISTS Files (
				ID			INTEGER		PRIMARY KEY		AUTOINCREMENT,
				Directory	INTEGER						NOT NULL,
				Name		TEXT						NOT NULL,
				Size		INTEGER						NOT NULL,
				LastWrite	INTEGER						NOT NULL,
				Genre		TEXT,
				Artist		TEXT,
				Album		TEXT,
				Title		TEXT,
				Time		INTEGER,
				UNIQUE (Directory, Name)
			);";
		cmd.ExecuteNonQuery();

		// テーブル作成
		//var createTableCmd = connection.CreateCommand();
		//createTableCmd.CommandText = @"
		//CREATE TABLE IF NOT EXISTS Users (
		//Id INTEGER PRIMARY KEY AUTOINCREMENT,
		//Name TEXT NOT NULL
		//);
		//";
		//createTableCmd.ExecuteNonQuery();

		// データ挿入（SQLインジェクション対策としてパラメータを使用）
		//var insertCmd = connection.CreateCommand();
		//insertCmd.CommandText = "INSERT INTO Users (Name) VALUES ($name);";
		//insertCmd.Parameters.AddWithValue("$name", "Taro");
		//insertCmd.ExecuteNonQuery();

		// データ読み出し
		//var selectCmd = connection.CreateCommand();
		//selectCmd.CommandText = "SELECT Id, Name FROM Users;";
		//using var reader = selectCmd.ExecuteReader();
		//while (reader.Read())
		//{
		//var id = reader.GetInt32(0);
		//var name = reader.GetString(1);
		//Console.WriteLine($"Id: {id}, Name: {name}");
		//}
	}

	private void OpenDB()
	{
		SqliteConnection conn = this.Connection;

		// Properties
		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = @"
			SELECT Name, Value FROM Properties;";

		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				switch (reader.GetString(0))
				{
					case "BaseDirectory":
						this.BaseDirectory = reader.GetString(1);
						break;
				}
			}
		}
	}

	private int? GetDirectoryID(string path, bool insertIfNotExists)
	{
		SqliteConnection conn = this.Connection;

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = @"
			SELECT ID FROM Directories
			WHERE Path = $dir;";
		cmd.Parameters.AddWithValue("$dir", path);

		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			if (reader.Read()) return reader.GetInt32(0);
		}

		if (!insertIfNotExists) return null;

		cmd = conn.CreateCommand();
		cmd.CommandText = @"
			INSERT INTO Directories(Path)
			VALUES($dir)
			RETURNING ID;";
		cmd.Parameters.AddWithValue("$dir", path);

		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			if (reader.Read()) return reader.GetInt32(0);
		}

		return null;
	}

	private void DeleteRemovedDirectory()
	{
		SqliteConnection conn = this.Connection;

		// DBからディレクトリを取得してファイルシステム上に存在しなければ削除
		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = @"
			SELECT ID, Path FROM Directories;";

		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			int oID = reader.GetOrdinal("ID");
			int oPath = reader.GetOrdinal("Path");

			while (reader.Read())
			{
				int id = reader.GetInt32(oID);
				string path = reader.GetString(oPath);
				if (!Directory.Exists(System.IO.Path.Combine(this.BaseDirectory, path)))
				{
					DeleteDirectory(id);
				}
			}
		}
	}

	private void DeleteDirectory(int id)
	{
		SqliteConnection conn = this.Connection;

		// ディレクトリ内のファイルを削除
		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = @"
			DELETE FROM Files WHERE Directory = $dir;";
		cmd.Parameters.AddWithValue("$dir", id);

		cmd.ExecuteNonQuery();

		// ディレクトリを削除
		cmd = conn.CreateCommand();
		cmd.CommandText = @"
			DELETE FROM Directories WHERE ID = $dir;";
		cmd.Parameters.AddWithValue("$dir", id);

		cmd.ExecuteNonQuery();
	}

	private AudioFileCollection GetFiles(int? dirID)
	{
		AudioFileCollection files = new();

		SqliteCommand cmd = this.Connection.CreateCommand();
		cmd.CommandText = @"
			SELECT	Files.ID,
					Files.Directory		AS DirectoryID,
					Files.Name,
					Files.Size,
					Files.LastWrite,
					Files.Genre,
					Files.Artist,
					Files.Album,
					Files.Title,
					Files.Time,
					Directories.Path	AS DirectoryPath
			FROM	Files
			INNER JOIN	Directories ON (Files.Directory = Directories.ID)
			";
		if (dirID is not null)
		{
			cmd.CommandText += @"
			WHERE	Files.Directory = $Directory";
		}
		cmd.CommandText += @"
			ORDER BY Files.Directory, Files.ID;";
		if (dirID is not null)
		{
			cmd.Parameters.AddWithValue("$Directory", dirID);
		}

		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			int oID = reader.GetOrdinal("ID");
			int oDir = reader.GetOrdinal("DirectoryID");
			int oName = reader.GetOrdinal("Name");
			int oSize = reader.GetOrdinal("Size");
			int oLastWrite = reader.GetOrdinal("LastWrite");
			int oGenre = reader.GetOrdinal("Genre");
			int oArtist = reader.GetOrdinal("Artist");
			int oAlbum = reader.GetOrdinal("Album");
			int oTitle = reader.GetOrdinal("Title");
			int oTime = reader.GetOrdinal("Time");
			int oDirPath = reader.GetOrdinal("DirectoryPath");

			while (reader.Read())
			{
				string path = System.IO.Path.Combine(this.BaseDirectory, reader.GetString(oDirPath), reader.GetString(oName));
				System.Console.WriteLine(@"{path}");

				//AudioFile af = new(this, reader.GetInt32(oID), reader.GetInt32(oDir), path, reader.GetInt64(oSize), reader.GetInt64(oLastWrite));
				//if (!reader.IsDBNull(oGenre)) af.Tag.Genre = reader.GetString(oGenre);
				//if (!reader.IsDBNull(oArtist)) af.Tag.Artist = reader.GetString(oArtist);
				//if (!reader.IsDBNull(oAlbum)) af.Tag.Album = reader.GetString(oAlbum);
				//if (!reader.IsDBNull(oTitle)) af.Tag.Title = reader.GetString(oTitle);
				//if (!reader.IsDBNull(oTime)) af.Tag.Seconds = reader.GetInt32(oTime);
			}
		}

		return files;
	}

	internal void Save(AudioFile file)
	{
		SqliteCommand cmd = this.Connection.CreateCommand();
		cmd.CommandText = @"
			INSERT INTO Files
					(	Directory,	Name,		Size,		LastWrite,
						Genre,		Artist,		Album,		Title,		Time)
			VALUES	(	$Directory,	$Name,		$Size,		$LastWrite,
						$Genre,		$Artist,	$Album,		$Title,		$Time)
			ON CONFLICT (Directory,	Name)
			DO UPDATE SET	Size=EXCLUDED.Size,					LastWrite==EXCLUDED.LastWrite,
							Genre==EXCLUDED.Genre,				Artist==EXCLUDED.Artist,
							Album==EXCLUDED.Album,				Title==EXCLUDED.Title,
							Time==EXCLUDED.Time;";
		cmd.Parameters.AddWithValue("$Directory", file.Directory);
		cmd.Parameters.AddWithValue("$Name", file.FileName);
		cmd.Parameters.AddWithValue("$Size", file.Size);
		cmd.Parameters.AddWithValue("$LastWrite", file.LastWrite);
		cmd.Parameters.AddWithValue("$Genre", (object?)file.Tag.Genre ?? DBNull.Value);
		cmd.Parameters.AddWithValue("$Artist", (object?)file.Tag.Artist ?? DBNull.Value);
		cmd.Parameters.AddWithValue("$Album", (object?)file.Tag.Album ?? DBNull.Value);
		cmd.Parameters.AddWithValue("$Title", (object?)file.Tag.Title ?? DBNull.Value);
		cmd.Parameters.AddWithValue("$Time", file.Tag.Seconds);

		//System.Console.WriteLine(string.Format($"{file.Path} => {file.Directory},{file.FileName}  {file.Size},{file.LastWrite} {file.Tag.Genre},{file.Tag.Artist},{file.Tag.Album},{file.Tag.Title},{file.Tag.Seconds}"));

		cmd.ExecuteNonQuery();
	}
}
