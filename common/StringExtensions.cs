using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common;

internal static class StringExtensions
{
	/// <summary>
	/// 文字列の前後のクォーテーションマークをトリミングする
	/// </summary>
	/// <param name="s">トリミング対象の文字列。</param>
	/// <param name="quart">トリミングするクォーテーションマーク。</param>
	/// <returns>クォーテーションマークをトリミングした文字列。</returns>
	/// <remarks>
	/// <para>文字列の前後が同一のマークの場合のみ、そのマークをトリミングして返す。</para>
	/// <para>前後が別々のマークの場合には、元の文字列をそのまま返す。</para>
	/// <para>また、トリミングするのは一番外側のクォーテーションマークのみ。</para>
	/// </remarks>
	public static string TrimQuotation(this string s, char[] quart)
	{
		if (string.IsNullOrEmpty(s)) return s;

		int last = s.Length - 1;
		if (last == 0) return s;

		foreach (char c in quart) {
			if (s[0] == c && s[last] == c) {
				return s.Substring(1, s.Length - 2);
			}
		}

		return s;
	}

	/// <summary>
	/// 単語の先頭を大文字にする
	/// </summary>
	/// <param name="s">先頭を大文字にする文字列。</param>
	/// <returns>単語の先頭を大文字に変換した結果。</returns>
	/// <remarks>元の文字列は変更しない。</remarks>
	public static string ToInitialCase(this string s)
	{
		TextInfo tx = Thread.CurrentThread.CurrentCulture.TextInfo;
		return tx.ToTitleCase(s);
	}

	/// <summary>
	/// 文字列の集合を小文字化する
	/// </summary>
	/// <param name="ss">小文字化する文字列の集合。</param>
	/// <returns>小文字化した文字列の集合。</returns>
	/// <remarks>元の文字列は変更しない。</remarks>
	public static string?[]? ToLower(ICollection<string>? ss)
	{
		if (ss is null) return null;
		if (ss.Count <= 0) return Array.Empty<string>();

		string?[] result = new string[ss.Count];
		int i = 0;
		foreach (string s in ss) {
			if (s is not null) {
				result[i++] = s.ToLower();
			} else {
				result[i++] = null;
			}
		}
		return result;
	}

	/// <summary>
	/// 文字列の集合を大文字化する
	/// </summary>
	/// <param name="ss">大文字化する文字列の集合。</param>
	/// <returns>大文字化した文字列の集合。</returns>
	/// <remarks>元の文字列は変更しない。</remarks>
	public static string?[]? ToUpper(ICollection<string> ss)
	{
		if (ss is null) return null;
		if (ss.Count <= 0) return Array.Empty<string>();

		string?[] result = new string[ss.Count];
		int i = 0;
		foreach (string s in ss) {
			if (s is not null) {
				result[i++] = s.ToUpper();
			} else {
				result[i++] = s;
			}
		}
		return result;
	}

	/// <summary>
	/// 文字列の集合に対して単語の先頭を大文字化する
	/// </summary>
	/// <param name="ss">単語の先頭を大文字化する文字列の集合。</param>
	/// <returns>単語の先頭を大文字化した文字列の集合。</returns>
	/// <remarks>元の文字列は変更しない。</remarks>
	public static string?[]? ToInitialCase(ICollection<string> ss)
	{
		if (ss is null) return null;
		if (ss.Count <= 0) return Array.Empty<string>();

		string?[] result = new string[ss.Count];
		int i = 0;
		foreach (string s in ss) {
			if (s is not null) {
				result[i++] = ToInitialCase(s);
			} else {
				result[i++] = s;
			}
		}
		return result;
	}

	/// <summary>
	/// バイト列を string にダンプする
	/// </summary>
	/// <param name="data">ダンプするバイト列。</param>
	/// <returns>バイト列を16進文字列にダンプした文字列。</returns>
	public static string DumpByte(byte[] data)
	{
		List<string> dat = new List<string>();
		foreach (byte b in data) {
			dat.Add(b.ToString("x2"));
		}
		return string.Join(" ", dat.ToArray());
	}

	/// <summary>
	/// 文字列から、正規表現で指定した文字を削除する
	/// </summary>
	/// <param name="s">指定の文字を削除する文字列。</param>
	/// <param name="remove">削除する文字を表す正規表現。</param>
	/// <returns>指定した文字を削除した文字列。</returns>
	/// <remarks>
	/// <para>数字を削除する。</para>
	/// <code>StringExtensions.RemoveRegex(s, "[0-9]");</code>
	/// <para>アルファベット以外を削除する。</para>
	/// <code>StringExtensions.RemoveRegex(s, "[^a-zA-Z]");</code>
	/// </remarks>
	public static string RemoveByRegex(this string s, string remove)
	{
		Regex re = new(remove);
		return re.Replace(s, "");
	}

	/// <summary>
	/// 文字列の先頭から、指定された文字がいくつあるかカウントする
	/// </summary>
	/// <param name="s">指定された文字を探す文字列。</param>
	/// <param name="chr">先頭から探す文字。</param>
	/// <returns>文字列の先頭から、指定された文字が連続する回数。</returns>
	public static int StartsWithRepeatOf(this string s, char chr)
	{
		if (string.IsNullOrEmpty(s)) return 0;

		int cnt = 0;
		foreach (char c in s) {
			if (c != chr) break;
			cnt++;
		}
		return cnt;
	}

	public static bool Equals(string[] s1, string[] s2)
	{
		return Equals(s1, s2, StringComparison.CurrentCulture);
	}

	public static bool Equals(string[] s1, string[] s2, StringComparison comparison)
	{
		if (s1 is not null && s2 is not null) {
			if (s1.Length != s2.Length) return false;
			for (int i = 0; i < s1.Length; i++) {
				if (!string.Equals(s1[i], s2[i], comparison)) return false;
			}
			return true;
		} else if (s1 is null && s2 is null) {
			return true;
		} else {
			return false;
		}
	}

	public static int CountOf(this string s, char c)
	{
		return s.CountOf(c, 0, s.Length);
	}

	public static int CountOf(this string s, char c, int startIndex)
	{
		return s.CountOf(c, startIndex, s.Length - startIndex);
	}

	public static int CountOf(this string s, char c, int startIndex, int count)
	{
		int cnt = 0;
		while (true) {
			if (count <= 0) break;

			int ret = s.IndexOf(c, startIndex, count);
			if (ret < 0) break;
			cnt++;

			int start = ret + 1;
			count -= (start - startIndex);
			startIndex = start;
		}

		return cnt;
	}

	public static int CountOf(this string s, char[] chars)
	{
		return s.CountOf(chars, 0, s.Length);
	}

	public static int CountOf(this string s, char[] chars, int startIndex)
	{
		return s.CountOf(chars, startIndex, s.Length - startIndex);
	}

	public static int CountOf(this string s, char[] chars, int startIndex, int count)
	{
		int cnt = 0;
		foreach (char c in chars) {
			cnt += s.CountOf(c, startIndex, count);
		}
		return cnt;
	}

	public static int CountOf(this string s, string c, int startIndex, int count)
	{
		int cnt = 0;
		while (true) {
			if (count <= 0) break;

			int ret = s.IndexOf(c, startIndex, count);
			if (ret < 0) break;
			cnt++;

			int start = ret + 1;
			count -= (start - startIndex);
			startIndex = start;
		}

		return cnt;
	}

	public static int CountOf(this string s, string c, int startIndex, int count, StringComparison comparisonType)
	{
		int cnt = 0;
		while (true) {
			if (count <= 0) break;

			int ret = s.IndexOf(c, startIndex, count, comparisonType);
			if (ret < 0) break;
			cnt++;

			int start = ret + 1;
			count -= (start - startIndex);
			startIndex = start;
		}

		return cnt;
	}
}
