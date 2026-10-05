using System;
using System.Collections;
using System.Collections.Generic;
using k2audio.Models;

namespace k2audio.Models.Audio;

internal class AudioFileCollection : ICollection<AudioFile>
{
	private SortedList<int, AudioFile> _files = [];
	private SortedList<DirectoryFilenameKey, AudioFile> _filenameDictionary = [];

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
	// ICollection
	public void Add(AudioFile file)
	{
		if (file.ID is not null && file.Directory is not null)
		{
			_files.Add((int)file.ID, file);
			_filenameDictionary.Add(new DirectoryFilenameKey((int)file.Directory, file.FileName), file);
		}
	}
	public void Clear()
	{
		_files.Clear();
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
			return _files.Remove((int)file.ID);
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
