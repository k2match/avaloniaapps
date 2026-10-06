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
	private SortedList<string, DirectoryFilenameDictionary>? _artistDictionary = null;
	private SortedList<string, DirectoryFilenameDictionary>? _albumDictionary = null;
	private SortedList<string, DirectoryFilenameDictionary>? _genreDictionary = null;

	//========================================
	// コンストラクタ
	public AudioFileCollection()
	{
	}

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
	internal IList<string> GetArtists()
	{
		this.MakeTagDictionary();

		List<string> artists = new();
		if(_artistDictionary is not null){
			artists.AddRange(_artistDictionary.Keys);
		}
		return artists;
	}

	internal IList<AudioFile>? SelectArtist(string argist)
	{
		this.MakeTagDictionary();

		if (_artistDictionary is not null && _artistDictionary.TryGetValue(argist, out DirectoryFilenameDictionary? dic))
		{
			return dic.Values;
		}
		return null;
	}

	private void ClearTagDictionary()
	{
		_artistDictionary = null;
		_albumDictionary = null;
		_genreDictionary = null;
	}

	private void MakeTagDictionary()
	{
		if (_artistDictionary is null)
		{
			_artistDictionary = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Artist) || af.Directory is null) continue;
				if (_artistDictionary.TryGetValue(af.Tag.Artist, out DirectoryFilenameDictionary? dic))
				{
					dic.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
				}
				else
				{
					DirectoryFilenameDictionary dicn = new();
					dicn.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
					_artistDictionary.Add(af.Tag.Artist, dicn);
				}
			}
		}

		if (_albumDictionary is null)
		{
			_albumDictionary = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Album) || af.Directory is null) continue;
				if (_albumDictionary.TryGetValue(af.Tag.Album, out DirectoryFilenameDictionary? dic))
				{
					dic.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
				}
				else
				{
					DirectoryFilenameDictionary dicn = new();
					dicn.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
					_albumDictionary.Add(af.Tag.Album, dicn);
				}
			}
		}

		if(_genreDictionary is null)
		{
			_genreDictionary = new();

			foreach (AudioFile af in _files.Values)
			{
				if (string.IsNullOrEmpty(af.Tag.Genre) || af.Directory is null) continue;
				if (_genreDictionary.TryGetValue(af.Tag.Genre, out DirectoryFilenameDictionary? dic))
				{
					dic.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
				}
				else
				{
					DirectoryFilenameDictionary dicn = new();
					dicn.Add(new DirectoryFilenameKey((int)af.Directory, af.FileName), af);
					_genreDictionary.Add(af.Tag.Genre, dicn);
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

			this.ClearTagDictionary();
		}
	}
	public void Clear()
	{
		_files.Clear();
		this.ClearTagDictionary();
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
			if (ret) this.ClearTagDictionary();
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
