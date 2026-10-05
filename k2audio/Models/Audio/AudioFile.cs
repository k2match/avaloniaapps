using System;
using System.IO;
using k2audio.Models;

namespace k2audio.Models.Audio;

internal class AudioFile
{
	//========================================
	// コンストラクタ
	public AudioFile(FileInfo fi) : this(null, null, fi)
	{
	}

	public AudioFile(Catalog? catalog, int? directoryID, FileInfo fi)
	{
		this.Catalog = catalog;
		this.ID = null;
		this.Directory = directoryID;
		this.Path = fi.FullName;
		this.Size = fi.Length;
		this.LastWrite = fi.LastWriteTimeUtc.ToFileTimeUtc();

		this.Tag.ReadTag(this.Path);
	}

	public AudioFile(Catalog catalog, int id, int directoryID, string path, long size, long lastWrite)
	{
		this.Catalog = catalog;
		this.ID = id;
		this.Directory = directoryID;
		this.Path = path;
		this.Size = size;
		this.LastWrite = lastWrite;
	}

	//========================================
	// 操作

	//========================================
	// プロパティ
	private Catalog? Catalog { get; set; }
	public int? ID { get; private set; }
	public int? Directory { get; private set; }
	public string Path { get; }
	public string FileName
	{
		get { return System.IO.Path.GetFileName(this.Path); }
	}
	public long Size { get; } = 0;
	public long LastWrite { get; } = 0;
	public AudioTag Tag { get; } = new();
}
