using System;
using ManagedBass;
using ManagedBass.Flac;

namespace k2audio.Models.Audio;

internal class AudioPlayer
{
	private BassStream? _stream = null;
	private AudioFile? _currentFile = null;

	//========================================
	// コンストラクタ
	public AudioPlayer()
	{
		this.Init();
	}

	public void Init()
	{
	}

	//========================================
	// 操作
	public bool IsPlaying
	{
		get {
			if (_stream is not null) {
				return (_stream.IsPlaying);
			}
			return false;
		}
	}
	public AudioFile? CurrentFile
	{
		get { return _currentFile; }
	}
	public void Play(AudioFile file, float volume)
	{
		this.Stop();

		try {
			_currentFile = file;
			_stream = new BassStream(file.Path);
			_stream.Volume = (volume / 100);
			_stream.Play();
			System.Console.WriteLine($"Play: {file.Path} Volume: {_stream.Volume}");	// TODO: 削除
		} catch (Exception ex) {
			System.Console.WriteLine(ex.Message);		// TODO: 削除
			System.Console.WriteLine(ex.StackTrace);	// TODO: 削除
			this.Stop();
		}
	}

	public void Pause()
	{
		if (_stream is not null) {
			if (_stream.IsPaused) {
				_stream.Resume();
			} else {
				_stream.Pause();
			}
		}
		//if(_output is not null){
		//	if(_output.PlaybackState == PlaybackState.Playing){
		//		_output.Pause();
		//	}else if(_output.PlaybackState == PlaybackState.Paused){
		//		_output.Play();
		//	}
		//}
	}

	public void Stop()
	{
		if (_stream is not null) {
			_stream.Dispose();
			_stream = null;
		}
		_currentFile = null;

		//if (_reader is not null){
		//	_reader.Close();
		//	_reader.Dispose();
		//	_reader = null;
		//}
		//if(_output is not null){
		//	_output.Stop();
		//	_output.Dispose();
		//	_output = null;
		//}
		//_currentFile = null;
	}

	public void SetVolume(float volume)
	{
		if (_stream is not null) {
			_stream.Volume = volume / 100;
		}
		//if(_reader is not null){
		//	_reader.Volume = volume / 100;
		//}
		//if(_output is not null){
		//	_output.Volume = volume / 100;
		//}
	}

	public void Seek(double position)
	{
		if (_stream is not null)
		{
			_stream.Seek(position / 100.0);
		}
		//if(_reader is not null){
		//	TimeSpan	tm = _reader.TotalTime * (position / 100.0);
		//	_reader.CurrentTime = tm;
		//}
	}

	public static bool IsSupported(string filename)
	{
		string ext = System.IO.Path.GetExtension(filename).ToLower();
		return (ext.Equals(".mp3") ||
				ext.Equals(".flac") ||
				ext.Equals(".m4a"));
	}

	//========================================
	// プロパティ
	public TimeSpan CurrentTime
	{
		get
		{
			if (_stream is not null)
			{
				return _stream.CurrentTime;
			}
			//if(_reader is not null){
			//	return _reader.CurrentTime;
			//}
			return TimeSpan.Zero;
		}
	}

	public TimeSpan TotalTime
	{
		get
		{
			if (_stream is not null)
			{
				return _stream.TotalTime;
			}
			//if(_reader is not null){
			//	return _reader.TotalTime;
			//}
			return TimeSpan.Zero;
		}
	}

	//========================================
	// シングルトン
	public static readonly AudioPlayer Instance = new AudioPlayer();
}
