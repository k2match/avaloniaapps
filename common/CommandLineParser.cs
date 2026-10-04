using System;
using System.Collections.Generic;
using System.Text;

namespace Common;

public class InvalidParameterException : ApplicationException
{
	public InvalidParameterException() : base()
	{ }
	public InvalidParameterException(string message) : base(message)
	{ }
}

public class InvalidOptionException : ApplicationException
{
	public InvalidOptionException() : base()
	{ }
	public InvalidOptionException(string message) : base(message)
	{ }
}

public class DuplicateOptionException : InvalidOptionException
{
	public DuplicateOptionException(string message) : base(message)
	{ }
}

public class UnknownOptionException : InvalidOptionException
{
	public UnknownOptionException(string message) : base(message)
	{ }
}

public class CommandLineParser
{
	private string _command;
	private List<string> _parameters;
	private SortedDictionary<string, string?> _options;
	private SortedList<string, int> _inquiryOptions;
	private bool _ignoreCaseOption;

	//========================================
	// コンストラクタ
	public CommandLineParser() : this(Environment.GetCommandLineArgs(), false)
	{
	}
	public CommandLineParser(bool ignoreCaseOption) : this(Environment.GetCommandLineArgs(), ignoreCaseOption)
	{
	}
	public CommandLineParser(string[] commandline) : this(commandline, false)
	{
	}
	public CommandLineParser(string[] commandline, bool ignoreCaseOption)
	{
		_command = string.Empty;
		_parameters = [];
		_options = [];
		_inquiryOptions = [];
		_ignoreCaseOption = ignoreCaseOption;
		this.Parse(commandline);
	}

	//========================================
	// パース
	protected void Parse(string[] commandline)
	{
		Clear();
		if (commandline is null) throw new ArgumentException();
		if (commandline.Length <= 0) throw new ArgumentException();

		_command = commandline[0];
		for (int i = 1; i < commandline.Length; i++)
		{
			string cmd = commandline[i];
			//if(cmd[0] == '-' || cmd[0] == '/'){
			if (cmd[0] == '-')
			{
				// オプション
				string[] keyval = cmd.Split(new Char[] { '=' }, 2);
				string key = keyval[0];
				if (_ignoreCaseOption) key = key.ToLower();
				if (keyval.Length >= 2)
				{
					_options.Add(TrimOptionKey(key), keyval[1]);
				}
				else if (keyval.Length >= 1)
				{
					_options.Add(TrimOptionKey(key), null);
				}

			}
			else
			{
				// パラメータ
				_parameters.Add(cmd);
			}
		}
	}

	protected void Clear()
	{
		_command = string.Empty;
		_parameters.Clear();
		_options.Clear();
	}

	/// <summary>
	/// オプションのキー文字列から余分な文字をトリミング
	/// </summary>
	/// <param name="key">トリミングするキー。</param>
	/// <returns>トリミングした結果文字列。</returns>
	/// <remarks>
	/// <para>前方の"/","-"、前後の空白をトリミングします。</para>
	/// </remarks>
	protected static string TrimOptionKey(string key)
	{
		// 前方の "-" "/" をトリム
		key = key.TrimStart(new char[] { '-', '/' });
		// 空白をトリム
		key = key.Trim(new char[] { ' ', '\t' });
		// クォーテーションをトリム
		key = StringExtensions.TrimQuotation(key, new char[] { '\"', '\'' });
		return key;
	}

	//========================================
	// プロパティ
	/// <summary>
	/// コマンド名
	/// </summary>
	public string Command
	{
		get { return _command; }
	}

	//========================================
	// パラメータ
	/// <summary>
	/// パラメータ数
	/// </summary>
	public int ParameterCount
	{
		get { return _parameters.Count; }
	}

	/// <summary>
	/// 全てのパラメータ
	/// </summary>
	public List<string> Parameters
	{
		get { return _parameters; }
	}

	//========================================
	// オプション
	/// <summary>
	/// オプション数
	/// </summary>
	public int OptionCount
	{
		get { return _options.Count; }
	}

	/// <summary>
	/// 特定のオプションが登録されているかどうか
	/// </summary>
	/// <param name="option">登録されているか判断するオプション。</param>
	/// <returns>オプションが登録されている場合は true。それ以外の場合は false。</returns>
	public bool HasOption(string option)
	{
		if (_ignoreCaseOption) option = option.ToLower();
		if (!_inquiryOptions.ContainsKey(option))
		{
			_inquiryOptions.Add(option, 0);
		}
		return _options.ContainsKey(option);
	}

	/// <summary>
	/// オプションを指定して値を取得
	/// </summary>
	/// <param name="option">値を取得するオプション。</param>
	/// <returns>オプションが登録されていて、値が設定されている場合はその値。それ以外の場合は null。</returns>
	/// <remarks>
	/// <para>オプションが登録されていない場合、オプションに値が指定されていない場合は共に null を返す。</para>
	/// </remarks>
	public string? GetOptionValue(string option)
	{
		if (_ignoreCaseOption) option = option.ToLower();
		if (!_inquiryOptions.ContainsKey(option))
		{
			_inquiryOptions.Add(option, 0);
		}
		string? value;
		if (this.GetOptionValue(option, out value))
		{
			return value;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// オプションを指定して値を取得
	/// </summary>
	/// <param name="option">値を取得するオプション。</param>
	/// <param name="val">オプションが見つかった場合は、そのオプションに関連付けられている値。それ以外の場合は未定義。</param>
	/// <returns>オプションが登録されている場合は true。それ以外の場合は false。</returns>
	public bool GetOptionValue(string option, out string? val)
	{
		if (_ignoreCaseOption) option = option.ToLower();
		if (!_inquiryOptions.ContainsKey(option))
		{
			_inquiryOptions.Add(option, 0);
		}
		return _options.TryGetValue(option, out val);
	}

	/// <summary>
	/// 複数のオプションの中から何れかが指定されているかを調べる
	/// </summary>
	/// <param name="options">何れかが指定されたかを調べるオプション。</param>
	/// <returns>指定されたオプション。いずれのオプションも指定されなかった場合は null 参照。</returns>
	/// <exception cref="ArgumentException">パラメータ options が null か空。</exception>
	/// <exception cref="DuplicateOptionException">options のうち二つ以上のオプションがコマンドラインから指定されている。</exception>
	public string? SelectOption(params string[] options)
	{
		if (options == null || options.Length <= 0)
		{
			throw new ArgumentException();
		}

		List<string> opts = new List<string>();
		for (int i = 0; i < options.Length; i++)
		{
			if (this.HasOption(options[i]))
			{
				opts.Add(options[i]);
			}
		}

		if (opts.Count == 0)
		{
			return null;
		}
		else if (opts.Count == 1)
		{
			return opts[0];
		}
		else
		{
			throw new DuplicateOptionException(string.Join(",", opts.ToArray()));
		}
	}

	/// <summary>
	/// 不明なオプションのチェック
	/// </summary>
	/// <returns>不明なオプションの配列。不明なオプションが存在しない場合 null 参照。</returns>
	private string[]? GetUnknownOptions()
	{
		List<string> unknown = new List<string>();

		foreach (KeyValuePair<string, string?> k in _options)
		{
			if (!_inquiryOptions.ContainsKey(k.Key))
			{
				unknown.Add(k.Key);
			}
		}

		if (unknown.Count <= 0) return null;
		return unknown.ToArray();
	}

	/// <summary>
	/// 不明なオプションのチェック
	/// </summary>
	/// <exception cref="UnknownOptionException">不明なオプションが指定されている。</exception>
	public void EvaluateUnknownOptions()
	{
		string[]? unknown = this.GetUnknownOptions();

		if (unknown == null)
		{
			return;
		}
		else
		{
			throw new UnknownOptionException(string.Join(",", unknown));
		}
	}
}
