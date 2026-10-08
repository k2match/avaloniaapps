using System;
using System.Collections;
using System.Collections.Generic;
using k2audio.Models;

namespace k2audio.Models.Audio;

using DirectoryFilenameDictionary = SortedList<AudioFileCollection.DirectoryFilenameKey, AudioFile>;

internal class AudioFileCollection : ICollection<AudioFile>
{
	private SortedList<int, AudioFile> _files = [];
	private DirectoryFilenameDictionary _filenameDictionary = [];
	private CatalogTagCollection? _artists = null;
	private CatalogTagCollection? _albums = null;
	private CatalogTagCollection? _genres = null;

	//========================================
	// コンストラクタ
	public AudioFileCollection()
	{
	}

	//========================================
	// 操作
	public AudioFile? GetFile(int dirID, string filename)
	{
		if (_filenameDictionary.TryGetValue(new DirectoryFilenameKey(dirID, filename), out AudioFile? file))
		{
			return file;
		}

		return null;
	}

	//========================================
	// タグ
	internal CatalogTagCollection GetArtists()
	{
		this.MakeTagCollection();

		if (_artists is not null)
			return _artists;
		else
			throw new ApplicationException("Artists is null.");
	}

	internal CatalogTagCollection GetAlbums()
	{
		this.MakeTagCollection();

		if (_albums is not null)
			return _albums;
		else
			throw new ApplicationException("Albums is null.");
	}

	internal CatalogTagCollection GetGenres()
	{
		this.MakeTagCollection();

		if (_genres is not null)
			return _genres;
		else
			throw new ApplicationException("Genres is null.");
	}

	private void ClearTagCollection()
	{
		_artists = null;
		_albums = null;
		_genres = null;
	}

	private void MakeTagCollection()
	{
		if (_artists is null)
		{
			_artists = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Artist) || af.Directory is null) continue;

				if (_artists.TryGetValue(af.Tag.Artist, out CatalogTag tag))
				{
					tag.AddFile(af);
				}
				else
				{
					_artists.Add(new CatalogTag(af.Tag.Artist, af));
				}
			}
		}

		if(_albums is null)
		{
			_albums = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Album) || af.Directory is null) continue;

				if (_albums.TryGetValue(af.Tag.Album, out CatalogTag tag))
				{
					tag.AddFile(af);
				}
				else
				{
					_albums.Add(new CatalogTag(af.Tag.Album, af));
				}
			}
		}

		if(_genres is null)
		{
			_genres = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Genre) || af.Directory is null) continue;

				if (_genres.TryGetValue(af.Tag.Genre, out CatalogTag tag))
				{
					tag.AddFile(af);
				}
				else
				{
					_genres.Add(new CatalogTag(af.Tag.Genre, af));
				}
			}
		}
	}

	//========================================
	// ICollection
	public void Add(AudioFile file)
	{
		if (file.ID is not null && file.Directory is not null)
		{
			_files.Add((int)file.ID, file);
			_filenameDictionary.Add(new DirectoryFilenameKey((int)file.Directory, file.FileName), file);

			this.ClearTagCollection();
		}
	}
	public void Clear()
	{
		_files.Clear();
		this.ClearTagCollection();
	}
	public bool Contains(AudioFile file)
	{
		if (file.ID is null) return false;
		return _files.ContainsKey((int)file.ID);
	}
	public void CopyTo(AudioFile[] files, int index)
	{
		_files.Values.CopyTo(files, index);
	}
	public int Count
	{
		get { return _files.Count; }
	}
	public bool IsReadOnly
	{
		get { return false; }
	}
	public bool Remove(AudioFile file)
	{
		if (file.ID is not null && file.Directory is not null)
		{
			_filenameDictionary.Remove(new DirectoryFilenameKey((int)file.Directory, file.FileName));
			bool ret = _files.Remove((int)file.ID);
			if (ret) this.ClearTagCollection();
			return ret;
		}
		return false;
	}
	public IEnumerator<AudioFile> GetEnumerator()
	{
		foreach (AudioFile f in _files.Values)
		{
			yield return f;
		}
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal class DirectoryFilenameKey : IComparable<DirectoryFilenameKey>
	{
		//========================================
		// コンストラクタ
		public DirectoryFilenameKey(int directory, string filename)
		{
			this.DirectoryID = directory;
			this.FileName = filename;
		}

		//========================================
		// プロパティ
		public int DirectoryID { get; }
		public string FileName { get; }

		public int CompareTo(DirectoryFilenameKey? other)
		{
			// nullの場合は、このインスタンスを「より大きい」とみなす
			if (other is null) return 1;

			int ret = this.DirectoryID.CompareTo(other.DirectoryID);
			if (ret == 0)
			{
				ret = this.FileName.CompareTo(other.FileName);
			}
			return ret;
		}
	}
}
