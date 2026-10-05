using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using k2audio.Models;

namespace k2audio.ViewModels;

public partial class ConfigureViewModel : ViewModelBase
{
	public ConfigureViewModel() : base()
	{
	}

	public void Initialize()
	{
		System.Console.WriteLine("Initialize Configure");
	}

	public void Terminate()
	{
	}

	//========================================
	// プロパティ

	//========================================
	// コマンド
	[RelayCommand]
	private void Close()
	{
		System.Console.WriteLine("Close");

		// 閉じるメッセージを送信
		WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
	}
}
