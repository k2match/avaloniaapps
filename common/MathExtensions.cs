using System;

namespace Common;

public static class MathExtensions
{
	/// <summary>
	/// double型の指定した小数点桁数で切り捨てます。
	/// </summary>
	/// <param name="value">対象の数値</param>
	/// <param name="digits">残す小数点以下の桁数</param>
	public static double Floor(double value, int digits)
	{
		double factor = Math.Pow(10, digits);
		return Math.Floor(value * factor) / factor;
	}

	/// <summary>
	/// decimal型の指定した小数点桁数で切り捨てます。
	/// </summary>
	/// <param name="value">対象の数値</param>
	/// <param name="digits">残す小数点以下の桁数</param>
	public static decimal Floor(decimal value, int digits)
	{
		decimal factor = (decimal)Math.Pow(10, digits);
		return Math.Floor(value * factor) / factor;
	}

	/// <summary>
	/// double型の指定した小数点桁数で切り上げます。
	/// </summary>
	/// <param name="value">対象の数値</param>
	/// <param name="digits">残す小数点以下の桁数</param>
	public static double Ceiling(double value, int digits)
	{
		double factor = Math.Pow(10, digits);
		return Math.Ceiling(value * factor) / factor;
	}

	/// <summary>
	/// decimal型の指定した小数点桁数で切り上げます。
	/// </summary>
	/// <param name="value">対象の数値</param>
	/// <param name="digits">残す小数点以下の桁数</param>
	public static decimal Ceiling(decimal value, int digits)
	{
		decimal factor = (decimal)Math.Pow(10, digits);
		return Math.Ceiling(value * factor) / factor;
	}

	/// <summary>
	/// double型の変数を指定した小数点桁数で比較します。
	/// </summary>
	/// <param name="d1">比較する数値</param>
	/// <param name="d2">比較する数値</param>
	/// <param name="digits">比較する小数点以下の桁数</param>
	public static int Compare(this double d1, double d2, int digits)
	{
		d1 = Math.Round(d1, digits);
		d2 = Math.Round(d2, digits);
		return d1.CompareTo(d2);
	}

	/// <summary>
	/// float型の変数を指定した小数点桁数で比較します。
	/// </summary>
	/// <param name="f1">比較する数値</param>
	/// <param name="f2">比較する数値</param>
	/// <param name="digits">比較する小数点以下の桁数</param>
	public static int Compare(this float f1, float f2, int digits)
	{
		double d1 = Math.Round(f1, digits);
		double d2 = Math.Round(f2, digits);
		return d1.CompareTo(d2);
	}

	/// <summary>
	/// decimal型の変数を指定した小数点桁数で比較します。
	/// </summary>
	/// <param name="d1">比較する数値</param>
	/// <param name="d2">比較する数値</param>
	/// <param name="digits">比較する小数点以下の桁数</param>
	public static int Compare(this decimal d1, decimal d2, int digits)
	{
		d1 = Math.Round(d1, digits);
		d2 = Math.Round(d2, digits);
		return d1.CompareTo(d2);
	}
}
