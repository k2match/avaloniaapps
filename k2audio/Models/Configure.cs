using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Media;
using Common.IO;

namespace k2audio.Models;

[DataContract]
internal class Configure : IEquatable<Configure>, ICloneable
{
	//========================================
	// コンストラクタ
	public Configure()
	{
	}

	//========================================
	// プロパティ
	[IgnoreDataMember]
	public FontFamily ListFont { get; set; } = FontFamily.Default;
	[DataMember]
	public string ListFontName { get { return this.ListFont.Name; } set { this.ListFont = new FontFamily(value); } }
	[DataMember]
	public int ListFontSize { get; set; } = 14;

	//========================================
	// ICloneable
	public object Clone()
	{
		return this.MemberwiseClone();
	}

	public void CopyTo(Configure dest)
	{
		dest.ListFont = this.ListFont;
		dest.ListFontSize = this.ListFontSize;
	}

	//========================================
	// IEquatable
	public bool Equals(Configure? another)
	{
		if (another is null || this.GetType() != another.GetType()) return false;

		if (!this.ListFont.Equals(another.ListFont)) return false;
		if (this.ListFontSize != another.ListFontSize) return false;

		return true;
	}

	public static bool operator !=(Configure? a, Configure? b)
	{
		return !(a == b);
	}

	public static bool operator ==(Configure? a, Configure? b)
	{
		if (object.ReferenceEquals(a, b)) return true;

		if (a is null)
		{
			return (b is null);
		}
		else
		{
			return a.Equals(b);
		}
	}

	public override bool Equals(object? o)
	{
		if (o is null || this.GetType() != o.GetType()) return false;
		return this.Equals(o as Configure);
	}

	public override int GetHashCode()
	{
		int hash = 0;
		hash ^= ListFont.GetHashCode();
		hash ^= ListFontSize.GetHashCode();

		return hash;
	}

	//========================================
	// セーブ・ロード
	public static string SavePath
	{
		get
		{
			String name = Path.ChangeExtension(ThisApp.Instance.Name, ".cfg");
			return Path.Combine(ThisApp.Instance.ConfigureDirectory, name);
		}
	}

	public static void SaveToFile(Configure config)
	{
		DataContractSerializer serializer = new DataContractSerializer(typeof(Configure));

		// シリアル化してXMLファイルに保存する
		using (TemporaryFileStream fs = new TemporaryFileStream(Configure.SavePath))
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.NewLineOnAttributes = false;
			using (XmlWriter xw = XmlWriter.Create(fs, settings))
			{
				serializer.WriteObject(xw, config);
			}
			fs.Store();
		}
	}

	public static Configure LoadFromFile()
	{
		DataContractSerializer serializer = new DataContractSerializer(typeof(Configure));

		// XMLファイルから読み込み、逆シリアル化する
		Configure? config = null;
		try
		{
			if (File.Exists(Configure.SavePath))
			{
				using (XmlReader xr = XmlReader.Create(Configure.SavePath))
				{
					config = (Configure?)serializer.ReadObject(xr); ;
				}
			}
		}
		catch (IOException)
		{
		}

		return config ?? new Configure();
	}
}
