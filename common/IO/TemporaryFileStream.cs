using System;
using System.IO;

namespace Common.IO;

/// <summary>
/// ファイルを直接上書きしないために一時ファイルを作成して出力する
/// </summary>
/// <remarks>
/// <para>ファイルを上書きする際、出力途中にエラーが発生して、上書き前のファイルが壊れるのを避けるため、出力用の一時ファイルを作成してそのファイルに出力する。</para>
/// <para>一時ファイルへの出力が終了した場合、<see cref="Store"/>メソッドを呼び出す必要がある。その前にクローズした場合、一時ファイルは削除される。</para>
/// </remarks>
/// <example>
/// <code>
/// using(TemporaryFileStream fs = new TemporaryFileStream(path)){
/// 	fs.Write(array, offset, count);
/// 	fs.Store();
/// }
/// </code>
/// </example>
public class TemporaryFileStream : FileStream
{
	private	string	_orgName;
	private	string?	_tmpName;
	private	bool	_keepAttribute = true;

	//========================================
	// コンストラクタ
	/// <summary>
	/// 出力先のパスを指定して、TemporaryFileStream の新しいインスタンスを作成する。
	/// </summary>
	/// <param name="path">ファイルの出力パス。このパスに一時ファイル用のサフィックスを付与したファイル名で、ファイルを作成する。</param>
	public TemporaryFileStream(string path) : base(CreateTemporaryPath(path), FileMode.Create, FileAccess.Write)
	{
		_orgName = path;
		_tmpName = base.Name;
	}

	/// <summary>
	/// 出力先のパスを指定して、TemporaryFileStream の新しいインスタンスを作成する。
	/// </summary>
	/// <param name="path">ファイルの出力パス。このパスに一時ファイル用のサフィックスを付与したファイル名で、ファイルを作成する。</param>
	/// <param name="bufferSize">バッファーサイズ。</param>
	/// <param name="share">共有方法。</param>
	/// <param name="useAsync">非同期I/Oを使用する場合 true。</param>
	public	TemporaryFileStream(string path, FileShare share, int bufferSize, bool useAsync) : base(CreateTemporaryPath(path), FileMode.Create, FileAccess.Write, share, bufferSize, useAsync)
	{
		_orgName = path;
		_tmpName = base.Name;
	}

	private	static	string	CreateTemporaryPath(string path)
	{
		return path + ".$tmp";
	}

	//========================================
	// 操作
	/// <summary>
	/// 一時ファイルをクローズして、元のファイル名にリネームする。
	/// </summary>
	/// <remarks>
	/// <para>ファイル出力が正常に終了した場合、必ずこのメソッドを呼び出す必要がある。</para>
	/// <para>このメソッドを呼び出さずにクローズした場合、出力した一時ファイルは削除される。</para>
	/// </remarks>
	public void	Store()
	{
		base.Close();
		if(!string.IsNullOrEmpty(_tmpName) && !string.IsNullOrEmpty(_orgName)){
			if(File.Exists(_orgName)){
				// 属性をコピー
				if(_keepAttribute) {
					try{
						FileInfo	org = new FileInfo(_orgName);
						FileInfo	tmp = new FileInfo(_tmpName);
						tmp.Attributes = org.Attributes;
						tmp.CreationTime = org.CreationTime;
					}catch(IOException){
					}
				}

				// 上書き
				string	backup = TemporaryFile.CreateTemporaryPath(Path.GetDirectoryName(_orgName));
				File.Replace(_tmpName, _orgName, backup);
				File.Delete(backup);
			}else{
				File.Move(_tmpName, _orgName);
			}
			_tmpName = null;
		}
	}

	/// <summary>
	/// 一時ファイルをクローズする。
	/// </summary>
	/// <remarks>
	/// <para><see cref="Store"/>メソッドを呼び出さずにクローズした場合、一時ファイルは削除される。</para>
	/// </remarks>
	public	override	void	Close()
	{
		base.Close();
		if(!string.IsNullOrEmpty(_tmpName)){
			File.Delete(_tmpName);
		}
		_tmpName = null;
	}
}

/// <summary>
/// 書き込み終了後に自動的に元のファイルに置き換える TemporaryFileStream
/// </summary>
/// <remarks>
/// <para>ファイルへ出力の終了時に Store メソッドを呼び呼び出さなくても、自動的に元のファイルに置き換えられる。</para>
/// <para>ただし、出力中にエラーが発生した場合でも元のファイルが置き換えられてしまう点に注意。</para>
/// </remarks>
/// <example>
/// <code>
/// using(TemporaryFileStreamAuto fs = new TemporaryFileStreamAuto(path)){
/// 	fs.Write(array, offset, count);
/// }
/// </code>
/// </example>
public class TemporaryFileStreamAuto : TemporaryFileStream
{
	private	bool	_error;

	public	TemporaryFileStreamAuto(string path) : base(path)
	{
		_error = false;
	}

	public	override	void	Write(byte[] array, int offset, int count)
	{
		try{
			base.Write(array, offset, count);
		}catch(Exception){
			_error = true;
			throw;
		}
	}

	public	override	void	WriteByte(byte value)
	{
		try {
			base.WriteByte(value);
		}catch(Exception){
			_error = true;
			throw;
		}
	}

	public	override	void	Close()
	{
		if(!_error){
			base.Store();
		}else{
			base.Close();
		}
	}
}
