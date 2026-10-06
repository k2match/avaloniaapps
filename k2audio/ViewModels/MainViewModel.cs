using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Avalonia.Threading;
using Common;
using k2audio.Models;
using k2audio.Models.Audio;

namespace k2audio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	private Catalog? _catalog = null;
	private DispatcherTimer? _playWatchTimer = null;

	public MainViewModel() : base()
	{
	}

	public void Initialize()
	{
		System.Console.WriteLine("Initialize");		// TODO: 削除
		System.Console.WriteLine("Configure: " + ThisApp.Instance.ConfigureDirectory);		// TODO: 削除

		BassStream.Initialize();

		if (ThisApp.Instance.DirectFiles.Count > 0)
		{
			this.SetFiles(ThisApp.Instance.DirectFiles);
			this.PlayCommand.Execute(null);
		}
		else
		{
			this.ClearFiles();
			this.ShowPlayState();
		}
	}

	public void Terminate()
	{
		// App.axaml.cs で終了時に呼び出されるようにセット

		System.Console.WriteLine("Terminate");		// TODO: 削除

		AudioPlayer.Instance.Stop();
		BassStream.Free();

		ThisApp.Instance.Save();
	}

	internal void SaveState(ThisState state)
	{
		System.Console.WriteLine("SaveState");		// TODO: 削除
		if(state is null){
			System.Console.WriteLine("  state is null!");
		}else if(state.Catalogs is null){
			System.Console.WriteLine("  state.Catalogs is null!");
		}else{
			System.Console.WriteLine("  not null!");
		}

		// Volume
		state?.Volume = this.Volume;

		// TODO: CatalogList
		//state.Catalogs.Clear();
		//foreach(Catalog catalog in this.CatalogList){
		//	state.Catalogs.Add(catalog.Path);
		//}
	}

	internal void RestoreState(ThisState state)
	{
		System.Console.WriteLine("RestoreState");		// TODO: 削除

		// Volume
		this.Volume = state.Volume;

		// TODO:
	}

	//========================================
	// Catalog
	private void CreateCatalog()
	{
		string path = "/home/machijima/.config/k2audio/songs.cat";

		if (System.IO.File.Exists(path))
		{
			System.IO.File.Delete(path);
		}
		this.OpenCatalog(Catalog.Create(path, "/home/machijima/music/Songs"));
	}

	private void OpenCatalog()
	{
		string path = "/home/machijima/.config/k2audio/songs.cat";

		if (System.IO.File.Exists(path))
		{
			this.OpenCatalog(Catalog.Open(path));
		}
	}

	private void OpenCatalog(Catalog catalog)
	{
		// 開く
		_catalog = catalog;
		this.SetFiles(_catalog.AllFiles);

		foreach (string s in _catalog.AllFiles.GetArtists()) System.Console.WriteLine($"artist [{s}]");		// TODO: 削除

		// リストの先頭に追加
		this.CatalogList.Insert(0, catalog);

		// 既に存在すれば削除
		for (int i = 1; i < this.CatalogList.Count; i++)
		{
			if (this.CatalogList[i].Path == catalog.Path)
			{
				this.CatalogList.RemoveAt(i);
				break;
			}
		}
	}

	//========================================
	// リスト
	private void ClearFiles()
	{
		this.Files.Clear();
		this.SelectedIndex = -1;
		this.SetCommandEnabled();
	}
	private void SetFiles(IEnumerable<string>? files)
	{
		this.Files.Clear();
		if (files is not null)
		{
			foreach (string f in files)
			{
				this.Files.Add(new AudioFile(new FileInfo(f)));
			}
		}
		this.SelectedIndex = this.Files.Count > 0 ? 0 : -1;
		this.SetCommandEnabled();
	}
	private void SetFiles(IEnumerable<AudioFile>? files)
	{
		this.Files.Clear();
		if (files is not null)
		{
			foreach (AudioFile f in files)
			{
				this.Files.Add(f);
			}
		}
		this.SelectedIndex = this.Files.Count > 0 ? 0 : -1;
		this.SetCommandEnabled();
	}

	private	AudioFile?	GetSelectedFile()
	{
		if (this.Files.Count <= 0) return null;

		if (this.SelectedIndex < 0 || this.SelectedIndex >= this.Files.Count)
		{
			this.SelectedIndex = 0;
		}

		return this.Files[this.SelectedIndex];
	}

	//========================================
	// プレイヤー
	private void PlayAudio(AudioFile file)
	{
		this.StopAudio();

		AudioPlayer.Instance.Play(file, this.Volume);

		int?	cnt = this.Files?.Count;
		int?	idx = this.Files?.IndexOf(file);
		if(cnt is null || idx is null){
			cnt = 0;
			idx = -1;
		}
		System.Console.WriteLine($"Play Audio: {file.FileName} ({idx + 1}/{cnt})");		// TODO: 削除

		if(file is not null && file.Tag is not null){
			this.TrackSummary = $"{file.Tag.Artist} - {file.Tag.Album} - {file.Tag.Title}";
		}else{
			this.TrackSummary = string.Empty;
		}
		this.ShowPlayState();

		_playWatchTimer = new DispatcherTimer(DispatcherPriority.Normal){
			Interval = TimeSpan.FromMilliseconds(500),		// インターバル
		};
		_playWatchTimer.Tick += (sender, e) => {
			// 再生中に停止した(1曲の再生が終わった)とき
			//if (this._outputDevice.PlaybackState == PlaybackState.Stopped){
			//	// 次の曲がある場合は再生、無いならタイマーを止める
			//	PlayNextOrStopTimer(this._currentMusic);
			//}
			if(AudioPlayer.Instance.IsPlaying){
				this.ShowPlayState();
			}else{
				this.PlayNext();
			}
		};
		_playWatchTimer.Start();

		this.SetCommandEnabled();
	}

	private void PauseAudio()
	{
		AudioPlayer.Instance.Pause();

		this.SetCommandEnabled();
	}

	private void StopAudio()
	{
		if (_playWatchTimer is not null)
		{
			_playWatchTimer.Stop();
			_playWatchTimer = null;
		}

		AudioPlayer.Instance.Stop();
		this.ShowPlayState();
		this.SetCommandEnabled();
	}

	private AudioFile? GetNextFile()
	{
		int next = -1;

		AudioFile? currnet = AudioPlayer.Instance.CurrentFile;
		if (this.Files is not null && currnet is not null)
		{
			int idx = this.Files.IndexOf(currnet);
			if (idx >= 0 && idx < this.Files.Count - 1)
			{
				next = idx + 1;
			}
		}

		if (this.Files is not null && next >= 0)
		{
			return this.Files[next];
		}
		else
		{
			return null;
		}
	}

	private void PlayNext()
	{
		AudioFile? next = GetNextFile();
		if (next is not null)
		{
			this.PlayAudio(next);
		}
		else
		{
			this.StopAudio();
		}
	}

	private void SetPosition(double position)
	{
		AudioPlayer.Instance.Seek(position);
	}

	private void ShowPlayState()
	{
		bool playing = false;

		if (AudioPlayer.Instance.IsPlaying)
		{
			TimeSpan current = AudioPlayer.Instance.CurrentTime;
			TimeSpan total = AudioPlayer.Instance.TotalTime;

			if (total > TimeSpan.Zero)
			{
				this.CurrentTime = string.Format("{0:D2}:{1:D2} / {2:D2}:{3:D2}",
					current.Minutes, current.Seconds,
					total.Minutes, total.Seconds);

				this.SetPlayingPosition((current.TotalSeconds / total.TotalSeconds) * 100.0);
				//this.Position = (current.TotalSeconds / total.TotalSeconds) * 100.0;

				string title = AudioPlayer.Instance.CurrentFile?.Tag?.Title ?? string.Empty;
				if (!string.IsNullOrEmpty(title)) title += " - ";
				this.Title = title + ThisApp.Instance.Name;

				playing = true;
			}
		}

		if (!playing)
		{
			this.CurrentTime = string.Empty;
			this.SetPlayingPosition(0.0);
			//this.Position = 0.0;
			this.Title = ThisApp.Instance.Name;
		}
	}

	private void SetPlayingPosition(double position)
	{
		this.Position = position;
	}

	private void SetCommandEnabled()
	{
		this.IsCanPlay = (this.Files.Count > 0);
		this.IsCanStop = (AudioPlayer.Instance.IsPlaying);
		this.IsCanPause = (AudioPlayer.Instance.IsPlaying);
	}

	public void Seek(double position)
	{
		AudioPlayer.Instance.Seek(position);
	}

	public void SetVolume(float volume)
	{
		if (volume.Compare(this.Volume, 2) != 0)
		{
			this.Volume = volume;
			AudioPlayer.Instance.SetVolume(volume);
		}
	}

	//========================================
	// プロパティ
	[ObservableProperty]
	internal partial string Title { get; private set; } = String.Empty;
	internal ObservableCollection<Catalog> CatalogList { get; } = new();
	internal ObservableCollection<AudioFile> Files { get; } = new();
	[ObservableProperty]
	internal partial int SelectedIndex { get; set; } = -1;
	[ObservableProperty]
	internal partial string TrackSummary { get; private set; } = string.Empty;
	[ObservableProperty]
	internal partial double Position { get; private set; } = 0.0;
	[ObservableProperty]
	internal partial float Volume { get; private set; } = 0;
	[ObservableProperty]
	internal partial string CurrentTime { get; private set; } = string.Empty;

	//========================================
	// コマンド
	[RelayCommand]
	private void Quit()
	{
		System.Console.WriteLine("Quit");	// TODO: 削除

		// 閉じるメッセージを送信
		WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
	}

	[RelayCommand]
	private void Configure()
	{
		System.Console.WriteLine("Configure");		// TODO: 削除

		// 設定ウィンドウオープンメッセージ
		WeakReferenceMessenger.Default.Send(new OpenConfigureWindowMessage());
	}

	[RelayCommand(CanExecute = nameof(CanPlay))]
	private void Play()
	{
		AudioFile? file = this.GetSelectedFile();
		if (file is not null){
			this.PlayAudio(file);
			System.Console.WriteLine($"Play {file.Path}");		// TODO: 削除
		}else{
			System.Console.WriteLine("No Selected File");		// TODO: 削除
		}
	}
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(PlayCommand))]
	private bool _isCanPlay;
	private bool CanPlay()
	{
		return IsCanPlay;
	}

	[RelayCommand(CanExecute = nameof(CanStop))]
	private void Stop()
	{
		System.Console.WriteLine("Stop");		// TODO: 削除
		this.StopAudio();
	}
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StopCommand))]
	private bool _isCanStop;
	private bool CanStop()
	{
		return IsCanStop;
	}

	[RelayCommand(CanExecute = nameof(CanPause))]
	private void Pause()
	{
		System.Console.WriteLine("Pause");		// TODO: 削除
		this.PauseAudio();
	}
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(PauseCommand))]
	private bool _isCanPause;
	private bool CanPause()
	{
		return IsCanPause;
	}

	[RelayCommand]
	private void CatalogNew()
	{
		System.Console.WriteLine("CatalogNew");		// TODO: 削除

		this.CreateCatalog();
	}

	[RelayCommand]
	private void CatalogOpen()
	{
		System.Console.WriteLine("CatalogOpen");		// TODO: 削除

		this.OpenCatalog();
	}

	[ObservableProperty]
	public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
