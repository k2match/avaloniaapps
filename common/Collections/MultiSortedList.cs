using System;
using System.Collections.Generic;

namespace Common.Collections;

/// <summary>
/// 検索結果
/// </summary>
public class SearchResult : IEquatable<SearchResult>
{
	//========================================
	// コンストラクタ
	public SearchResult(int index, int foundCount)
	{
		this.Index = index;
		this.FoundCount = foundCount;
	}

	//========================================
	// プロパティ
	/// <summary>
	/// キーに一致する要素のインデックス
	/// </summary>
	/// <remarks>
	/// <para>キーに一致する要素が見つかった場合、先頭の要素のインデックス。</para>
	/// <para>見つからなかった場合は負の値。これはkeyの次に大きい要素のインデックスのビットごとの補数となる。例えば(1,3,5)から0をサーチした場合は-1、4をサーチした場合は-3、10をサーチした場合は-4。</para>
	/// </remarks>
	public int Index
	{
		get; private set;
	}

	/// <summary>
	/// キーに一致する要素の個数
	/// </summary>
	public int FoundCount
	{
		get; private set;
	}

	/// <summary>
	/// キーに一致する要素が見つかったかを表す値
	/// </summary>
	/// <remarks>キーに一致する要素が見つかった場合 true、それ以外の場合 false。</remarks>
	public bool Found
	{
		get { return (this.Index >= 0); }
	}

	/// <summary>
	/// キーに一致する要素が見つかった場合、その最後の要素のインデックス
	/// </summary>
	/// <exception cref="InvalidOperationException">検索対象が見つからなかった場合。</exception>
	public int LastIndex
	{
		get
		{
			if (!this.Found) throw new InvalidOperationException();

			return (this.Index + this.FoundCount - 1);
		}
	}

	/// <summary>
	/// キーに一致する要素が見つからなかった場合、キーの次に大きい要素のインデックス
	/// </summary>
	public int IndexToInsert
	{
		get
		{
			if (this.Index >= 0) return this.Index;
			return ~this.Index;
		}
	}

	//========================================
	// IEquatable
	public bool Equals(SearchResult? other)
	{
		if (other is null || this.GetType() != other.GetType()) return false;

		if (this.Index != other.Index) return false;
		if (this.FoundCount != other.FoundCount) return false;

		return true;
	}
	public static bool operator !=(SearchResult res1, SearchResult res2)
	{
		return !(res1 == res2);
	}

	public static bool operator ==(SearchResult res1, SearchResult res2)
	{
		if (object.ReferenceEquals(res1, res2)) return true;

		if (object.ReferenceEquals(res1, null))
		{
			return (object.ReferenceEquals(res2, null));
		}
		else
		{
			return res1.Equals(res2);
		}
	}

	public override bool Equals(object? obj)
	{
		if (obj is null || this.GetType() != obj.GetType()) return false;
		return this.Equals(obj as SearchResult);
	}

	public override int GetHashCode()
	{
		return this.Index.GetHashCode() ^ this.FoundCount.GetHashCode();
	}
}

/// <summary>
/// キーによりソートされた、キーと値のペアのコレクション。キーは重複が可能。
/// </summary>
/// <typeparam name="TKey">コレクション内のキーの型。</typeparam>
/// <typeparam name="TValue">コレクション内の値の型。</typeparam>
/// <remarks>キーが重複する要素については、格納した順番にソートされる。</remarks>
public class MultiSortedList<TKey, TValue> : IDictionary<TKey, TValue?>
												where TKey : IComparable
{
	private List<KeyValuePair<TKey, TValue?>> _list;
	private IComparer<TKey>? _comparer;
	private bool _sorted;

	//========================================
	// コンストラクタ
	public MultiSortedList() : this((IComparer<TKey>?)null)
	{
	}

	public MultiSortedList(IComparer<TKey>? comparer)
	{
		_list = new List<KeyValuePair<TKey, TValue?>>();
		_comparer = comparer;
		_sorted = false;
	}

	public MultiSortedList(IDictionary<TKey, TValue?> dictionary) : this(dictionary, null)
	{
	}

	public MultiSortedList(IDictionary<TKey, TValue?> dictionary, IComparer<TKey>? comparer)
	{
		_list = new List<KeyValuePair<TKey, TValue?>>(dictionary);
		_comparer = comparer;
		_sorted = false;
	}

	public MultiSortedList(int size) : this(size, null)
	{
	}

	public MultiSortedList(int size, IComparer<TKey>? comparer)
	{
		_list = new List<KeyValuePair<TKey, TValue?>>(size);
		_comparer = comparer;
		_sorted = false;
	}

	//========================================
	// IDictionary
	public void Add(TKey key, TValue? value)
	{
		this.Add(new KeyValuePair<TKey, TValue?>(key, value));
	}

	public void Add(KeyValuePair<TKey, TValue?> item)
	{
		_list.Add(item);
		this.Reset();
	}

	public void Clear()
	{
		_list.Clear();
		this.Reset();
	}

	public bool Contains(KeyValuePair<TKey, TValue?> item)
	{
		return (this.Search(item) >= 0);
	}

	public bool ContainsKey(TKey key)
	{
		return (this.BinarySearch(key) >= 0);
	}

	public void CopyTo(KeyValuePair<TKey, TValue?>[] array, int arrayIndex)
	{
		this.Sort();
		_list.CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<TKey, TValue?>> GetEnumerator()
	{
		this.Sort();
		return _list.GetEnumerator();
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public bool Remove(KeyValuePair<TKey, TValue?> item)
	{
		var search = this.SearchCount(item.Key);
		var target = new Stack<int>();
		int delCnt = 0;

		if (search.Found)
		{
			for (int i = search.Index; i <= search.LastIndex; i++)
			{
				if (object.Equals(_list[i].Value, item.Value)) target.Push(i);
			}
		}

		while (target.Count > 0)
		{
			_list.RemoveAt(target.Pop());
			delCnt++;
		}

		return (delCnt > 0);
	}

	public bool Remove(TKey key)
	{
		var search = this.SearchCount(key);

		if (search.Found)
		{
			_list.RemoveRange(search.Index, search.FoundCount);
			return true;
		}

		return false;
	}

	public bool TryGetValue(TKey key, out TValue? value)
	{
		var search = this.SearchCount(key);

		if (search.Found)
		{
			value = _list[search.Index].Value;
			return true;
		}
		else
		{
			value = default(TValue);
			return false;
		}
	}

	public int Count
	{
		get { return _list.Count; }
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public TValue? this[TKey key]
	{
		get
		{
			var search = this.SearchCount(key);
			if (search.Found)
			{
				return _list[search.Index].Value;
			}
			else
			{
				throw new KeyNotFoundException();
			}
		}
		set
		{
			var search = this.SearchCount(key);
			if (search.Found)
			{
				_list.RemoveAt(search.Index);
			}
			this.Add(key, value);
		}
	}

	public ICollection<TKey> Keys
	{
		get
		{
			this.Sort();
			List<TKey> keys = new(_list.Count);
			foreach (KeyValuePair<TKey, TValue?> i in _list)
			{
				keys.Add(i.Key);
			}
			return keys;
		}
	}

	public ICollection<TValue?> Values
	{
		get
		{
			this.Sort();
			List<TValue?> values = new(_list.Count);
			foreach (KeyValuePair<TKey, TValue?> i in _list)
			{
				values.Add(i.Value);
			}
			return values;
		}
	}

	//========================================
	// 検索
	/// <summary>
	/// コレクションに対してバイナリサーチを行う
	/// </summary>
	/// <param name="key">検索するキー。</param>
	/// <returns>
	/// <para>キーに一致する要素が見つかった場合、そのインデックス。一致する要素が複数ある場合、どの要素のインデックスかは未定義。</para>
	/// <para>見つからなかった場合は負の値。これはkeyの次に大きい要素のインデックスのビットごとの補数となる。例えば(1,3,5)から0をサーチした場合は-1、4をサーチした場合は-3、10をサーチした場合は-4。</para>
	/// <para>ここで~(ReturnValue)でkeyの次に大きい要素のインデックスを求められる。この例では~-1=0、~-3=2、~-4=3。</para>
	/// </returns>
	public int BinarySearch(TKey key)
	{
		this.Sort();
		return _list.BinarySearch(new KeyValuePair<TKey, TValue?>(key, default(TValue)), new KeyCompare(_comparer));
	}

	/// <summary>
	/// コレクションからキーに一致する要素を検索する
	/// </summary>
	/// <param name="key">検索するキー。</param>
	/// <returns>
	/// <para>キーに一致する要素が見つかった場合、その先頭要素のインデックス。</para>
	/// <para>見つからなかった場合は負の値。負の値については<see cref="BinarySearch(TKey)"/>を参照。</para>
	/// </returns>
	public int Search(TKey key)
	{
		int index = this.BinarySearch(key);
		if (index < 0) return index;

		int i1 = index - 1;
		while (i1 >= 0)
		{
			if (!_list[i1].Key.Equals(key)) break;
			i1--;
		}
		return i1 + 1;
	}

	/// <summary>
	/// コレクションからキーに一致する要素を検索する
	/// </summary>
	/// <param name="key">検索するキー。</param>
	/// <returns>検索結果。</returns>
	public SearchResult SearchCount(TKey key)
	{
		int index = this.BinarySearch(key);
		if (index < 0) return new SearchResult(index, 0); ;

		int i1 = index - 1;
		int i2 = index + 1;
		while (i1 >= 0)
		{
			if (!_list[i1].Key.Equals(key)) break;
			i1--;
		}
		while (i2 < _list.Count)
		{
			if (!_list[i2].Key.Equals(key)) break;
			i2++;
		}
		i1++; i2--;
		int cnt = (i2 - i1) + 1;
		return new SearchResult(i1, cnt);
	}

	/// <summary>
	/// コレクションから指定した範囲に一致するキーを持つ要素を検索する
	/// </summary>
	/// <param name="lower">検索するキーの下限。</param>
	/// <param name="upper">検索するキーの上限。</param>
	/// <returns>検索結果。</returns>
	/// <remarks>
	/// <para>SearchRange(x, y) は x 以上、y 以下のキーを検索する。これは [x,y] の意味。</para>
	/// </remarks>
	public SearchResult SearchRange(TKey lower, TKey upper)
	{
		return this.SearchRange(lower, true, upper, true);
	}

	/// <summary>
	/// コレクションから指定した範囲に一致するキーを持つ要素を検索する
	/// </summary>
	/// <param name="lower">検索するキーの下限。</param>
	/// <param name="upper">検索するキーの上限。</param>
	/// <param name="containThreshold">上限、下限に等しいキーを含める場合 true。</param>
	/// <returns>検索結果。</returns>
	/// <remarks>
	/// <para>SearchRange(x, y, false) は x より大きく y 未満のキーを検索する。これは (x,y) の意味。</para>
	/// <para>SearchRange(x, y, true) は x 以上、y 以下のキーを検索する。これは [x,y] の意味。</para>
	/// </remarks>
	public SearchResult SearchRange(TKey lower, TKey upper, bool containThreshold)
	{
		return this.SearchRange(lower, containThreshold, upper, containThreshold);
	}

	/// <summary>
	/// コレクションから指定した範囲に一致するキーを持つ要素を検索する
	/// </summary>
	/// <param name="lower">検索するキーの下限。</param>
	/// <param name="containLower">下限に等しいキーを含める場合 true。</param>
	/// <param name="upper">検索するキーの上限。</param>
	/// <param name="containUpper">上限に等しいキーを含める場合 true。</param>
	/// <returns>検索結果。</returns>
	/// <remarks>
	/// <para>SearchRange(x, true, y, false) は x 以上 y 未満のキーを検索する。これは [x,y) の意味。</para>
	/// <para>SearchRange(x, false, y, true) は x より大きく y 以下ののキーを検索する。これは (x,y] の意味。</para>
	/// </remarks>
	public SearchResult SearchRange(TKey lower, bool containLower, TKey upper, bool containUpper)
	{
		if (lower.CompareTo(upper) > 0) throw new ArgumentException();

		SearchResult res1 = this.SearchCount(lower);
		int i1;
		if (res1.Index < 0)
		{
			// 見つからない場合、次の要素のインデックス
			i1 = res1.IndexToInsert;
		}
		else
		{
			// 見つかった場合
			if (containLower)
			{
				i1 = res1.Index;
			}
			else
			{
				// 下限を含まないのであれば次のインデックスへ
				i1 = res1.Index + res1.FoundCount;
			}
		}
		SearchResult res2 = this.SearchCount(upper);
		int i2;
		if (res2.Index < 0)
		{
			// 見つからない場合、前の要素のインデックス
			i2 = res2.IndexToInsert - 1;
		}
		else
		{
			// 見つかった場合
			if (containUpper)
			{
				// 上限を含むのであれば最後のインデックスへ
				i2 = res2.LastIndex;
			}
			else
			{
				// 上限を含まないのであれば一つ前のインデックスへ
				i2 = res2.Index - 1;
			}
		}

		int index = i1;
		int cnt = i2 - i1 + 1;
		if (cnt <= 0)
		{
			index = res1.Index;
			cnt = 0;
		}

		return new SearchResult(index, cnt);
	}

	public int Search(KeyValuePair<TKey, TValue?> item)
	{
		SearchResult search = this.SearchCount(item.Key);

		if (search.Index >= 0)
		{
			for (int i = search.Index; i <= search.LastIndex; i++)
			{
				if (object.Equals(_list[i].Value, item.Value)) return i;
			}
			return -(search.Index + 1);
		}
		else
		{
			return search.Index;
		}
	}

	public bool TryGetValues(TKey key, out TValue?[]? values)
	{
		var search = this.SearchCount(key);

		if (search.Found)
		{
			values = new TValue[search.FoundCount];
			for (int i = 0; i < search.FoundCount; i++)
			{
				values[i] = _list[search.Index + i].Value;
			}
			return true;
		}
		else
		{
			values = null;
			return false;
		}
	}

	//========================================
	// 取得
	/// <summary>
	/// 指定した位置に格納されたキーを取得する
	/// </summary>
	/// <param name="index">キーを取得する、先頭を0とするインデックス。</param>
	/// <returns>指定した位置に格納されたキー。</returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public TKey GetKeyAt(int index)
	{
		this.Sort();
		return _list[index].Key;
	}

	/// <summary>
	/// 指定した位置に格納された値を取得する
	/// </summary>
	/// <param name="index">値を取得する、先頭を0とするインデックス。</param>
	/// <returns>指定した位置に格納された値。</returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public TValue? GetValueAt(int index)
	{
		this.Sort();
		return _list[index].Value;
	}

	/// <summary>
	/// 指定した位置から複数個の値を取得する
	/// </summary>
	/// <param name="index">値を取得する、先頭を0とするインデックス。</param>
	/// <param name="count">値を取得する個数。</param>
	/// <returns>指定した位置から指定された個数の値を格納したリスト。</returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public List<TValue?> GetValuesAt(int index, int count)
	{
		this.Sort();
		List<TValue?> values = new(count);
		for (int i = 0; i < count; i++)
		{
			values.Add(_list[index + i].Value);
		}
		return values;
	}

	/// <summary>
	/// コレクションから重複しないキーのリストを取得する
	/// </summary>
	/// <returns>重複しないキーのリスト。重複するキーは一つだけが含まれる。</returns>
	public List<TKey> GetKeysOne()
	{
		this.Sort();

		List<TKey> keys = new List<TKey>();
		foreach (TKey key in this.Keys)
		{
			if (keys.Count <= 0)
			{
				keys.Add(key);
			}
			else if (!key.Equals(keys[keys.Count - 1]))
			{
				keys.Add(key);
			}
		}

		return keys;
	}

	/// <summary>
	/// コレクションからキーの重複する項目を取得する
	/// </summary>
	/// <returns>キーが重複する項目の、キーと値のペア。</returns>
	public List<KeyValuePair<TKey, TValue?>> GetDuplicate()
	{
		var duplicate = new List<KeyValuePair<TKey, TValue?>>();
		if (_list.Count <= 1) return duplicate;

		this.Sort();

		TKey lastKey = _list[0].Key;
		int lastIndex = 0;
		for (int i = 1; i < _list.Count; i++)
		{
			if (lastKey.Equals(_list[i].Key))
			{
				if (lastIndex == (i - 1))
				{
					duplicate.Add(_list[lastIndex]);
				}
				duplicate.Add(_list[i]);
			}
			else
			{
				lastKey = _list[i].Key;
				lastIndex = i;
			}
		}

		return duplicate;
	}

	//========================================
	// 操作
	private void Reset()
	{
		_sorted = false;
	}

	private void Sort()
	{
		if (_sorted) return;

		//IComparer<TKey>		comparer	= (_comparer != null) ? _comparer : Comparer<TKey>.Default;
		//_list.MergeSort((x, y) => comparer.Compare(x.Key, y.Key));
		_list.MergeSort(new KeyCompare(_comparer));
		_sorted = true;
	}

	private class KeyCompare : IComparer<KeyValuePair<TKey, TValue>>
	{
		IComparer<TKey>? _comparer;

		public KeyCompare(IComparer<TKey>? comparer)
		{
			_comparer = comparer;
		}

		public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			if (_comparer != null)
			{
				return _comparer.Compare(x.Key, y.Key);
			}
			else
			{
				return x.Key.CompareTo(y.Key);
			}
		}
	}
}
