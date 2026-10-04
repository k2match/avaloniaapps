using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Common.IO;

public static class PathExtensions
{
	/// <summary>
	/// ファイルまたはディレクトリが存在するか確認する
	/// </summary>
	/// <param name="path">ファイルまたはディレクトリが存在するか確認するパス。</param>
	/// <returns>ファイルまたはディレクトリが存在する場合 true。</returns>
	public	static	bool	Exists(string path)
	{
		if(File.Exists(path) || Directory.Exists(path))		return true;
		return false;
	}

	/// <summary>
	/// フルパスより、基準となるディレクトリからの相対パス部分を取り出す
	/// </summary>
	/// <param name="baseDirectory">相対パスを取り出す基準となるディレクトリのパス。</param>
	/// <param name="fullPath">フルパス。</param>
	/// <returns>フルパスから取り出した、基準となるディレクトリからの相対パス。</returns>
	public static string? ToRelativePath(string? fullPath, string? baseDirectory)
	{
		if (string.IsNullOrEmpty(fullPath)) return null;
		if (string.IsNullOrEmpty(baseDirectory)) return null;

		if (baseDirectory.Equals(fullPath))
		{
			return string.Empty;
		}

		string dir;

		if (baseDirectory[baseDirectory.Length - 1] != System.IO.Path.DirectorySeparatorChar)
		{
			dir = baseDirectory + System.IO.Path.DirectorySeparatorChar;
		}
		else
		{
			dir = baseDirectory;
		}

		if (fullPath.StartsWith(dir))
		{
			return fullPath.Substring(dir.Length);
		}

		return null;
	}

	public static string? ToRelative(this string fullPath, string? baseDirectory)
	{
		return ToRelativePath(fullPath, baseDirectory);
	}

	/// <summary>
	/// パスを構成要素に分割する
	/// </summary>
	/// <param name="path">構成要素に分割するパス。</param>
	/// <returns>パスの構成要素の文字列を格納する配列。</returns>
	public	static	string[]	Split(string path)
	{
		if(string.IsNullOrEmpty(path))		throw new ArgumentException();

		int				n = path.StartsWithRepeatOf(Path.DirectorySeparatorChar);	// 先頭のセパレータの数

		string[]		ele = path.Split(Path.DirectorySeparatorChar);
		List<string>	elements = new List<string>(ele.Length);
		foreach(string s in ele){
			if(string.IsNullOrEmpty(s)){
				;
			}else if(n > 0){
				elements.Add(new string(Path.DirectorySeparatorChar, n) + s);		// 先頭のセパレータを戻す
				n = 0;
			}else{
				elements.Add(s);
			}
		}

		return elements.ToArray();
	}

	/// <summary>
	/// あるパスが、特定のディレクトリの配下であるかを調べる
	/// </summary>
	/// <param name="baseDirectory">このディレクトリの配下であるかを調べる。</param>
	/// <param name="subPath">特定のディレクトリの配下であるかを調べるパス。</param>
	/// <param name="isLower">パスが小文字（大文字）に揃えてある場合、trueを指定する。</param>
	/// <returns>パスが、特定のディレクトリの配下である場合true。</returns>
	public	static	bool	IsSubPath(string baseDirectory, string subPath, bool isLower = false)
	{
		// パスを正規化
		baseDirectory = Normalize(baseDirectory);
		subPath = Normalize(subPath);

		// 小文字に揃える
		if(!isLower)	subPath = subPath.ToLower();
		if(!isLower)	baseDirectory = baseDirectory.ToLower();

		// ディレクトリの末尾には"\"を付加しておく
		if(baseDirectory[baseDirectory.Length - 1] != '\\'){
			baseDirectory = baseDirectory + '\\';
		}

		// ディレクトリのパスで始まっていれば配下
		return subPath.StartsWith(baseDirectory);
	}

	/// <summary>
	/// 二つのパスが等しいかを調べる
	/// </summary>
	/// <param name="path1">等しいか調べるパス。</param>
	/// <param name="path2">等しいか調べるパス。</param>
	/// <returns>二つのパスが等しい場合true、それ以外の場合false。</returns>
	public	static	bool	IsEquals(string path1, string path2)
	{
		if(string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)){
			return false;
		}

		// パスを正規化
		path1 = Normalize(path1);
		path2 = Normalize(path2);
		return string.Equals(path1, path2, StringComparison.CurrentCultureIgnoreCase);
	}

	/// <summary>
	/// 与えられたパスを正規化する
	/// </summary>
	/// <param name="path">正規化するパス。</param>
	/// <returns>与えられたパスを正規化したパス。</returns>
	public	static	string	Normalize(string path)
	{
		return Path.GetFullPath(path);
	}

	/// <summary>
	/// ファイル名が不正でないかをチェックする
	/// </summary>
	/// <param name="name">不正でないかをチェックするファイル名。</param>
	/// <returns>ファイル名に使用禁止の文字が含まれている、または空の文字列であれば true。正しければ false。</returns>
	/// <remarks>ファイル名に"\"が含まれていると不正とするので、パスを与えた場合は不正となる。</remarks>
	/*public	static	bool	IsInvalidName(string name)
	{
		if(string.IsNullOrWhiteSpace(name))		return true;

		if(__invalid == null){
			__invalid = new Regex(	"[\\x00-\\x1f<>:\"/\\\\|?*]" +
									"|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)" +
									"|[\\. ]$",
									RegexOptions.IgnoreCase);
		}
		return __invalid.IsMatch(name);
	}
	private	static	Regex?	__invalid = null;*/

	/// <summary>
	/// 与えられたパスがフルパスかを判断する
	/// </summary>
	/// <param name="path">フルパスであるかを判断するパス。</param>
	/// <returns>与えられたパスがフルパスであれば true。</returns>
	public	static	bool	IsFullPath(string path)
	{
		if(string.IsNullOrWhiteSpace(path))		return false;

		if(path.StartsWith(@"\\"))				return true;
		if(path.Length >= 3){
			return (string.Equals(path.Substring(1, 2), @":\"));
		}

		return false;
	}
}
