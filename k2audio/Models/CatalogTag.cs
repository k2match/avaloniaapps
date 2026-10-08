using System;
using System.Collections;
using System.Collections.Generic;
using Common.Collections;
using k2audio.Models.Audio;

namespace k2audio.Models;

internal class CatalogTag
{
	//========================================
	// コンストラクタ
	public CatalogTag(string name)
	{
		this.Name = name;
	}
	public CatalogTag(string name, AudioFile file) : this(name)
	{
		this.AddFile(file);
	}

	//========================================
	// プロパティ
	public string Name { get; }
	public List<AudioFile> Files { get; } = new();
	public int Count { get { return this.Files.Count; } }

	//========================================
	// 操作
	public void AddFile(AudioFile file)
	{
		this.Files.Add(file);
	}
}

internal class CatalogTagCollection : ICollection<CatalogTag>//, IDictionary<string, CatalogTag>
{
	private SortedList<string, CatalogTag> _tags = [];

	//========================================
	// コンストラクタ
	public CatalogTagCollection()
	{
	}

	//========================================
	// 取得
	public IList<CatalogTag> GetTagsByCount(bool desc)
	{
		List<CatalogTag> list = new(_tags.Values);
		list.Sort((x, y) =>
		{
			int ret = x.Count.CompareTo(y.Count);
			if (desc) ret *= -1;
			if (ret == 0) ret = x.Name.CompareTo(y.Name);
			return ret;
		});

		return list;
	}

	//========================================
	// ICollection
	public void Add(CatalogTag tag)
	{
		_tags.Add(tag.Name, tag);
	}
	public void Clear()
	{
		_tags.Clear();
	}
	public bool Contains(CatalogTag tag)
	{
		return _tags.ContainsKey(tag.Name);
	}
	public void CopyTo(CatalogTag[] tags, int index)
	{
		_tags.Values.CopyTo(tags, index);
	}
	public int Count
	{
		get { return _tags.Count; }
	}
	public bool IsReadOnly
	{
		get { return false; }
	}
	public bool Remove(CatalogTag tag)
	{
		bool ret = _tags.Remove(tag.Name);
		return ret;
	}
	public IEnumerator<CatalogTag> GetEnumerator()
	{
		foreach (CatalogTag t in _tags.Values)
		{
			yield return t;
		}
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	//========================================
	// IDictionary
	public bool ContainsKey(string name)
	{
		return _tags.ContainsKey(name);
	}
	public bool Remove(string name)
	{
		return _tags.Remove(name);
	}
	public bool TryGetValue(string name, out CatalogTag tag)
	{
		return _tags.TryGetValue(name, out tag);
	}
	public CatalogTag this[string key]
	{
		get { return _tags[key]; }
		set { _tags[key] = value; }
	}
	/*public void Add(string name, CatalogTag tag)
	{
		_tags.Add(tag.Name, tag);
	}
	public ICollection<string> Keys
	{
		get { return _tags.Keys; }
	}
	public ICollection<CatalogTag> Values
	{
		get { return _tags.Values; }
	}
	public void Add(KeyValuePair<string, CatalogTag> item)
	{
		_tags.Add(item);
	}
	public bool Contains(KeyValuePair<string, CatalogTag> item)
	{
		return _tags.Contains(item);
	}
	public void CopyTo(KeyValuePair<string, CatalogTag>[] array, int arrayIndex)
	{
		_tags.CopyTo(array, arrayIndex);
	}
	public bool Remove(KeyValuePair<string, CatalogTag> item)
	{
		return _tags.Remove(item);
	}
	public IEnumerable<KeyValuePair<string, CatalogTag>> GetEnumerator()
	{
		foreach (var i in _tags)
		{
			yield return i;
		}
	}*/
}
