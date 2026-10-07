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
	[DataMember]
	public float Volume { get; set; } = 20.0f;
	[DataMember]
	public List<string> Catalogs { get; set; } = new();
	[DataMember]
	public double PlayingWidth { get; set; } = -1;
	[DataMember]
	public double FileNameWidth { get; set; } = -1;
	[DataMember]
	public double ArtistWidth { get; set; } = -1;
	[DataMember]
	public double AlbumWidth { get; set; } = -1;
	[DataMember]
	public double TitleWidth { get; set; } = -1;
	[DataMember]
	public double TimeWidth { get; set; } = -1;
	[DataMember]
	public double FavoriteWidth { get; set; } = -1;

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
