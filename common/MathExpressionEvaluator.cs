using System;
using System.Globalization;

namespace Common;

public class MathExpressionEvaluator
{
	private readonly string _expr;
	private int _pos;

	private MathExpressionEvaluator(string expression)
	{
		_expr = expression ?? throw new ArgumentNullException(nameof(expression));
		_pos = 0;
	}

	/// <summary>
	/// 与えられた数式文字列を評価し、計算結果を返します。
	/// </summary>
	public static double Evaluate(string expression)
	{
		var evaluator = new MathExpressionEvaluator(expression);
		double result = evaluator.ParseExpression();

		evaluator.SkipWhitespace();
		if (evaluator._pos < evaluator._expr.Length)
		{
			throw new FormatException($"予期しない文字が見つかりました: '{evaluator._expr[evaluator._pos]}' (位置: {evaluator._pos})");
		}

		return result;
	}

	// 式: 加算・減算 (+, -)
	private double ParseExpression()
	{
		double value = ParseTerm();

		while (true)
		{
			SkipWhitespace();
			if (Match('+'))
			{
				value += ParseTerm();
			}
			else if (Match('-'))
			{
				value -= ParseTerm();
			}
			else
			{
				break;
			}
		}

		return value;
	}

	// 項: 乗算・除算 (*, /)
	private double ParseTerm()
	{
		double value = ParseFactor();

		while (true)
		{
			SkipWhitespace();
			if (Match('*'))
			{
				value *= ParseFactor();
			}
			else if (Match('/'))
			{
				double divisor = ParseFactor();
				if (divisor == 0.0)
				{
					throw new DivideByZeroException("0による除算が発生しました。");
				}
				value /= divisor;
			}
			else
			{
				break;
			}
		}

		return value;
	}

	// 因子: べき乗 (^) - べき乗は右結合 (例: 2^3^2 = 2^(3^2))
	private double ParseFactor()
	{
		double value = ParsePrimary();

		SkipWhitespace();
		if (Match('^'))
		{
			// 右結合のため再帰的に ParseFactor() を呼び出す
			double exponent = ParseFactor();
			return Math.Pow(value, exponent);
		}

		return value;
	}

	// 基本要素: 数値、単項演算子 (+/-)、カッコ
	private double ParsePrimary()
	{
		SkipWhitespace();

		if (_pos >= _expr.Length)
		{
			throw new FormatException("数式の途中で終端に達しました。");
		}

		// 単項演算子の処理 (+, -)
		if (Match('+'))
		{
			return ParsePrimary();
		}
		if (Match('-'))
		{
			return -ParsePrimary();
		}

		// カッコの処理
		if (Match('('))
		{
			double value = ParseExpression();
			SkipWhitespace();
			if (!Match(')'))
			{
				throw new FormatException("閉じカッコ ')' が不足しています。");
			}
			return value;
		}

		// 数値 (整数・小数) の読み取り
		int startPos = _pos;
		while (_pos < _expr.Length && (char.IsDigit(_expr[_pos]) || _expr[_pos] == '.'))
		{
			_pos++;
		}

		if (_pos == startPos)
		{
			throw new FormatException($"予期しないトークンです: '{_expr[_pos]}' (位置: {_pos})");
		}

		string numberString = _expr.Substring(startPos, _pos - startPos);
		if (!double.TryParse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
		{
			throw new FormatException($"無効な数値形式です: '{numberString}'");
		}

		return number;
	}

	private void SkipWhitespace()
	{
		while (_pos < _expr.Length && char.IsWhiteSpace(_expr[_pos]))
		{
			_pos++;
		}
	}

	private bool Match(char expected)
	{
		if (_pos < _expr.Length && _expr[_pos] == expected)
		{
			_pos++;
			return true;
		}
		return false;
	}
}
