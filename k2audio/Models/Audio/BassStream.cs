using System;
using ManagedBass;
using ManagedBass.Flac;
using ManagedBass.Aac;

namespace k2audio.Models.Audio;

internal class BassStream : IDisposable
{
	private int _stream = 0;

	public BassStream(string path)
	{
		_stream = CreateStream(path);
		if (_stream == 0) {
			string msg = $"Bass.CreateStream Error: {Bass.LastError}";
			System.Console.WriteLine(msg);		// TODO: 削除
			throw new Exception(msg);
		}
	}

	~BassStream()
	{
		this.Dispose();
	}

	public void Dispose()
	{
		if (_stream != 0)
		{
			Bass.StreamFree(_stream);
			_stream = 0;
		}

		GC.SuppressFinalize(this);
	}

	private static int CreateStream(string path)
	{
		string ext = System.IO.Path.GetExtension(path).ToLower();
		return ext switch {
			".flac" => BassFlac.CreateStream(path, 0, 0, BassFlags.Default),
			".m4a" => BassAac.CreateStream(path, 0, 0, BassFlags.Default),
			_ => Bass.CreateStream(path, 0, 0, BassFlags.Default),
		};
	}

	public void Play()
	{
		if (_stream != 0)
		{
			if (!Bass.ChannelPlay(_stream, false))
			{
				string msg = $"Bass.ChannelPlay() Error: {Bass.LastError}";
				System.Console.WriteLine(msg);		// TODO: 削除
				throw new Exception(msg);
			}
		}
	}

	public void Stop()
	{
		if (_stream != 0)
		{
			if (!Bass.ChannelStop(_stream))
			{
				string msg = $"Bass.ChannelStop() Error: {Bass.LastError}";
				System.Console.WriteLine(msg);		// TODO: 削除
				throw new Exception(msg);
			}
		}
		Bass.StreamFree(_stream);
		_stream = 0;
	}

	public void Pause()
	{
		if (_stream != 0)
		{
			if (!Bass.ChannelPause(_stream))
			{
				string msg = $"Bass.ChannelPause() Error: {Bass.LastError}";
				System.Console.WriteLine(msg);		// TODO: 削除
				throw new Exception(msg);
			}
		}
	}

	public void Resume()
	{
		if (_stream != 0)
		{
			if (!Bass.ChannelPlay(_stream, false))
			{
				string msg = $"Bass.ChannelPlay() Error: {Bass.LastError}";
				System.Console.WriteLine(msg);		// TODO: 削除
				throw new Exception(msg);
			}
		}
	}

	public float Volume
	{
		get
		{
			if (_stream != 0)
			{
				if (Bass.ChannelGetAttribute(_stream, ChannelAttribute.Volume, out float volume))
				{
					return volume;
				}
				else
				{
					string msg = $"Bass.ChannelGetAttribute() Error: {Bass.LastError}";
					System.Console.WriteLine(msg);		// TODO: 削除
					throw new Exception(msg);
				}
			}
			return 0;
		}

		set
		{
			if (_stream != 0)
			{
				if (!Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, value))
				{
					string msg = $"Bass.ChannelSetAttribute() Error: {Bass.LastError}";
					System.Console.WriteLine(msg);		// TODO: 削除
					throw new Exception(msg);
				}
			}
		}
	}

	public TimeSpan CurrentTime
	{
		get
		{
			if (_stream != 0)
			{
				long bytes = Bass.ChannelGetPosition(_stream, PositionFlags.Bytes);
				double seconds = Bass.ChannelBytes2Seconds(_stream, bytes);
				return TimeSpan.FromSeconds(seconds);
				//return (float)seconds;
			}
			return TimeSpan.Zero;
		}
	}

	public TimeSpan TotalTime
	{
		get
		{
			if (_stream != 0)
			{
				long bytes = Bass.ChannelGetLength(_stream, PositionFlags.Bytes);
				double seconds = Bass.ChannelBytes2Seconds(_stream, bytes);
				return TimeSpan.FromSeconds(seconds);
				//return seconds;
			}
			return TimeSpan.Zero;
		}
	}

	public void Seek(double position)
	{
		if (_stream != 0)
		{
			long bytes = Bass.ChannelGetLength(_stream, PositionFlags.Bytes);
			long targetBytes = (long)(bytes * position);
			if (!Bass.ChannelSetPosition(_stream, targetBytes, PositionFlags.Bytes))
			{
				string msg = $"Bass.ChannelSetPosition() Error: {Bass.LastError}";
				System.Console.WriteLine(msg);		// TODO: 削除
				throw new Exception(msg);
			}
			//		long	bytes = (long)(position * Bass.ChannelGetAttribute(_stream, ChannelAttribute.BytesPerSecond));
			//		if(!Bass.ChannelSetPosition(_stream, bytes, PositionFlags.Relative)){
			//			string	msg = $"Bass.ChannelSetPosition() Error: {Bass.LastError}";
			//			System.Console.WriteLine(msg);
			//			throw new Exception(msg);
			//		}
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (_stream != 0)
			{
				var state = Bass.ChannelIsActive(_stream);
				return (state == PlaybackState.Playing);
			}
			return false;
		}
	}

	public bool IsPaused
	{
		get
		{
			if (_stream != 0)
			{
				var state = Bass.ChannelIsActive(_stream);
				return (state == PlaybackState.Paused);
			}
			return false;
		}
	}

	public static void Initialize()
	{
		if (!Bass.Init(-1, 44100, DeviceInitFlags.Default, IntPtr.Zero))
		{
			string msg = $"Bass.Init() Error: {Bass.LastError}";
			System.Console.WriteLine(msg);		// TODO: 削除
			throw new Exception(msg);
		}

		//BassInfo	info;
		//if(!Bass.GetInfo(out info)){
		//	string	msg = $"Bass.Init() Error: {Bass.LastError}";
		//	System.Console.WriteLine(msg);
		//	throw new Exception(msg);
		//}
	}

	public static void Free()
	{
		Bass.Free();
	}
}
