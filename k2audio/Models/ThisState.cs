using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common;

namespace k2audio.Models;

[DataContract]
internal class ThisState : StateBase
{
	//========================================
	// コンストラクタ
	public ThisState()
	{
	}

	//========================================
	// プロパティ
	public float Volume = 20.0f;
	public List<string> Catalogs = new();

	//========================================
	// セーブ・ロード
	public static string SavePath
	{
		get
		{
			return System.IO.Path.Combine(ThisApp.Instance.ConfigureDirectory, ThisApp.Instance.Name + ".state");
		}
	}

	public static void SaveToFile(ThisState state)
	{
		StateBase.SaveToFile(ThisState.SavePath, state, typeof(ThisState));
	}

	public static ThisState LoadFromFile()
	{
		StateBase? winState = StateBase.LoadFromFile(ThisState.SavePath, typeof(ThisState));
		ThisState? appState = null;
		if (winState is not null)
		{
			appState = winState as ThisState;
		}
		if (appState is null)
		{
			appState = new ThisState();
		}
		return appState;
	}
}
