using System;
using System.Collections.Generic;
using Common;

namespace k2audio.Models;

internal class ThisApp : AppBase
{
	//========================================
	// コンストラクタ
	public ThisApp() : base(ExitCode.Normal)
	{
		CommandLineParser cmd = new();
		foreach (string s in cmd.Parameters)
		{
			this.DirectFiles.Add(s);
		}
	}

	//========================================
	// プロパティ
	public override string Name
	{ get { return "k2audio"; } }

	private Configure? _configure = null;
	public Configure Configure
	{
		get
		{
			return _configure ??= Configure.LoadFromFile();
		}
	}
	//public Configure Configure { get; } = new();

	private ThisState? _state = null;
	public ThisState State
	{
		get
		{
			return _state ??= ThisState.LoadFromFile();
		}
	}
	//public ThisState State { get; } = new();

	public List<string> DirectFiles { get; } = [];

	//========================================
	// セーブ・ロード
	public void Save()
	{
		Configure.SaveToFile(this.Configure);
	}

	//========================================
	// シングルトン
	public static readonly ThisApp Instance = new();
}
