using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Common.IO;

namespace Common;

[DataContract]
public abstract class StateBase
{
	//========================================
	// コンストラクタ
	public StateBase()
	{
	}

	//========================================
	// プロパティ
	[IgnoreDataMember]
	public bool Enabled { get; private set; } = false;
	[DataMember]
	public string WindowState { get; set; } = "Normal";
	[DataMember]
	public int Left { get; set; }
	[DataMember]
	public int Top { get; set; }
	[DataMember]
	public double Width { get; set; }
	[DataMember]
	public double Height { get; set; }

	//========================================
	// セーブ・ロード
	public static void SaveToFile(string path, StateBase state, Type type)
	{
		DataContractSerializer serializer = new DataContractSerializer(type);

		// シリアル化してXMLファイルに保存する
		using (TemporaryFileStream fs = new TemporaryFileStream(path))
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
				serializer.WriteObject(xw, state);
			}
			fs.Store();
		}
	}

	public static StateBase? LoadFromFile(string path, Type type)
	{
		DataContractSerializer serializer = new DataContractSerializer(type);

		// XMLファイルから読み込み、逆シリアル化する
		StateBase? state = null;
		try
		{
			if (File.Exists(path))
			{
				using (XmlReader xr = XmlReader.Create(path))
				{
					state = (StateBase?)serializer.ReadObject(xr);
					state?.Enabled = true;
				}
			}
		}
		catch (IOException)
		{
		}

		return state;
	}
}
