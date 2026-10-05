using System;
using System.Collections.Generic;

namespace Common.Collections;

public class FunctorComparer<T> : IComparer<T>
{
	private Comparison<T?> _comparison;

	public FunctorComparer(Comparison<T?> comparison)
	{
		_comparison = comparison;
	}

	public int Compare(T? x, T? y)
	{
		return _comparison(x, y);
	}
}

public static class ListExtensions
{
	//========================================
	// 定数
	// マージソートの代わりに挿入ソートを適用する要素数
	private const int insertionSortThreshold = 4;

	//========================================
	// 要素の取得
	public static T First<T>(this List<T> list)
	{
		return list[0];
	}
	public static T Last<T>(this List<T> list)
	{
		return list[list.Count - 1];
	}

	//========================================
	// 挿入ソート
	public static void InsertionSort<T>(this List<T> list)
	{
		InsertionSort(list, 0, list.Count, null);
	}
	public static void InsertionSort<T>(this List<T> list, Comparison<T?>? comparison)
	{
		if (list.Count <= 1) return;
		IComparer<T>? comparer = (comparison is not null) ? new FunctorComparer<T?>(comparison) : null;
		InsertionSort(list, comparer);
	}
	public static void InsertionSort<T>(this List<T> list, IComparer<T>? comparer)
	{
		InsertionSort(list, 0, list.Count, comparer);
	}
	public static void InsertionSort<T>(this List<T> list, int index, int count, IComparer<T>? comparer)
	{
		if (count <= 1) return;
		if (comparer is null)
		{
			comparer = Comparer<T>.Default;
		}

		InsertionSortUnsafe(list, index, count, comparer);
	}
	private static void InsertionSortUnsafe<T>(this List<T> list, int index, int count, IComparer<T> comparer)
	{
		var end = index + count;
		for (var i = index + 1; i < end; ++i)
		{
			var value = list[i];

			if (comparer.Compare(list[i - 1], value) > 0)
			{
				var j = i;
				do
				{
					list[j] = list[j - 1];
					--j;
				} while (j > index && comparer.Compare(list[j - 1], value) > 0);

				list[j] = value;
			}
		}
	}

	//========================================
	// マージソート
	public static void MergeSort<T>(this List<T> list, int minUnit = insertionSortThreshold)
	{
		MergeSort(list, 0, list.Count, null, minUnit);
	}
	public static void MergeSort<T>(this List<T> list, Comparison<T?> comparison, int minUnit = insertionSortThreshold)
	{
		if (list.Count <= 1) return;
		IComparer<T?>? comparer = (comparison is not null) ? new FunctorComparer<T?>(comparison) : null;
		MergeSort(list, comparer, minUnit);
	}
	public static void MergeSort<T>(this List<T> list, IComparer<T>? comparer, int minUnit = insertionSortThreshold)
	{
		MergeSort(list, 0, list.Count, comparer, minUnit);
	}
	public static void MergeSort<T>(this List<T> list, int index, int count, IComparer<T>? comparer, int minUnit = insertionSortThreshold)
	{
		if (count <= 1) return;
		if (comparer is null)
		{
			comparer = Comparer<T>.Default;
		}

		if (minUnit < 1)
		{
			minUnit = 1;
		}

		// 全体要素数が少なければ挿入ソート
		if (count <= minUnit)
		{
			InsertionSortUnsafe(list, index, count, comparer);
			return;
		}

		var end = index + count;
		var work = new T[count];

		// 少ない要素数の部分リストは挿入ソートを適用
		if (minUnit > 1)
		{
			for (var i = index; i < end; i += minUnit)
			{
				var subEnd = i + minUnit;
				if (subEnd < end)
				{
					InsertionSortUnsafe(list, i, minUnit, comparer);
				}
				else
				{
					// 末尾の部分リストはサイズが中途半端になることがある
					var subCount = end - i;
					if (subCount > 1)
					{
						InsertionSortUnsafe(list, i, subCount, comparer);
					}
					break;
				}
			}
		}

		// nは部分リストの長さ
		// 1, 2, 4, ... というように2倍されていく
		for (var n = minUnit; n < count; n <<= 1)
		{
			// 長さnの部分リストに分割されている前提で、2組ずつソートしていく
			var n_x2 = n << 1;
			for (var begin1 = index; begin1 <= end - n; begin1 += n_x2)
			{
				var begin2 = begin1 + n;

				// 前方部分リストをバッファに詰める
				list.CopyTo(begin1, work, 0, n);

				var i = begin1;

				// 前方部分リスト（バッファ）の範囲
				var i1 = 0;
				var end1 = n;

				// 後方部分リストの範囲
				var i2 = begin2;
				var end2 = i2 + n;
				if (end2 > end)
				{
					end2 = end;
				}

				// マージ
				while (i1 < end1 && i2 < end2)
				{
					var value1 = work[i1];
					var value2 = list[i2];

					if (comparer.Compare(value1, value2) > 0)
					{
						list[i] = value2;
						++i2;
					}
					else
					{
						list[i] = value1;
						++i1;
					}

					++i;
				}

				// バッファに残った要素の追加
				while (i1 < end1)
				{
					list[i] = work[i1];
					++i1;
					++i;
				}
			}
		}
	}
}

public static class IListExtensions
{
	public static void Swap<T>(this IList<T> list, int index1, int index2)
	{
		T work = list[index1];
		list[index1] = list[index2];
		list[index2] = work;
	}

	public static bool CanMoveForward<T>(this IList<T> list, int index)
	{
		return (index > 0 && index < list.Count);
	}

	public static void MoveForward<T>(this IList<T> list, int index)
	{
		list.Swap(index, index - 1);
	}

	public static bool CanMoveBackward<T>(this IList<T> list, int index)
	{
		return (index >= 0 && index < (list.Count - 1));
	}

	public static void MoveBackward<T>(this IList<T> list, int index)
	{
		list.Swap(index, index + 1);
	}

	public static bool IsIndexValid<T>(this IList<T> list, int index)
	{
		return (index >= 0 && index < list.Count);
	}
}
