using System;

namespace k2audio.Models.Audio;

internal class AudioTag
{
	//========================================
	// コンストラクタ
	public AudioTag()
	{
	}
	public AudioTag(string? genre, string? artist, string? album, string? title, int seconds)
	{
		this.Genre = genre;
		this.Artist = artist;
		this.Album = album;
		this.Title = title;
		this.Span = TimeSpan.FromSeconds(seconds);
	}

	//========================================
	// プロパティ
	public string? Genre { get; set; } = null;
	public string? Artist { get; set; } = null;
	public string? Album { get; set; } = null;
	public string? Title { get; set; } = null;
	public TimeSpan Span { get; set; } = TimeSpan.MinValue;
	public string Time
	{
		get
		{
			return this.Span.ToString("mm\\:ss");
		}
	}
	public int Seconds
	{
		get
		{
			return (int)this.Span.TotalSeconds;
		}
		set
		{
			this.Span = TimeSpan.FromSeconds(value);
		}
	}

	//========================================
	// 操作
	public void ReadTag(string path)
	{
		try
		{
			var tag = TagLib.File.Create(path);
			this.Genre = tag.Tag.FirstGenre;
			this.Artist = tag.Tag.FirstPerformer;
			this.Album = tag.Tag.Album;
			this.Title = tag.Tag.Title;
			this.Span = tag.Properties.Duration;
		}
		catch (Exception ex)
		{
			System.Console.WriteLine(ex.Message);
		}

		//System.Console.WriteLine(string.Format("{0} => [{1}] [{2}] [{3}] [{4}] [{5}({6})]", path, this.Genre, this.Artist, this.Album, this.Title, this.Time, this.Seconds));
	}
}
