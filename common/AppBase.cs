using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common;

/// <summary>
/// 終了コード
/// </summary>
public enum ExitCode
{
	Normal = 0,
	Cancel = 1,
	Error = -1
}

public	abstract	class	AppBase
{
	//private	const	string	ENVIROMENT_HOME	= "CONFIG_HOME";
	//private	string		_name = string.Empty;
	//private	string		_version = string.Empty;
	private string _configDirectory = string.Empty;

	//========================================
	// コンストラクタ
	public	AppBase() : this(ExitCode.Error)
	{
	}

	public	AppBase(ExitCode exitCode)
	{
		this.ExitCode = exitCode;
	}

	//========================================
	// プロパティ
	public abstract string Name { get; }
	//{
	//	get{
	//		if(string.IsNullOrEmpty(_name))			this.GetVersion();

	//		return _name;
	//	}
	//}

	//public	virtual	string	Version
	//{
	//	get{
	//		if(string.IsNullOrEmpty(_version))		this.GetVersion();

	//		return _version;
	//	}
	//}

	//protected	virtual	Assembly	GetAssembly()
	//{
	//	return Assembly.GetExecutingAssembly();
	//}

	//private	void		GetVersion()
	//{
	//	Assembly		asm = GetAssembly();
	//	AssemblyName	nm = asm.GetName();
	//	Version?		ver = nm.Version;
	//	if(nm?.Name != null)	_name = nm.Name;
	//	if(ver != null)			_version = ver.ToString();
	//}

	public	ExitCode	ExitCode		{	get;	set;	}

	public	string		ConfigureDirectory
	{
		get{
			if (!string.IsNullOrEmpty(_configDirectory)) return _configDirectory;

			//string?		dir = System.Environment.GetEnvironmentVariable(ENVIROMENT_HOME);
			//if(string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)){
			//	dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
			//}
			string	dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
			dir = Path.Combine(dir, ".config", this.Name);
			if(!Directory.Exists(dir))		Directory.CreateDirectory(dir);
			_configDirectory = dir;
			return _configDirectory;
		}
		set{	_configDirectory = value;	}
	}
}
