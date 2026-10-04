// See https://aka.ms/new-console-template for more information
using Common;

string[] expressions = new[]
{
	"1 + 2 * 3",                  // 7
	"(1 + 2) * 3",                // 9
	"2 ^ 3 ^ 2",                  // 2^(3^2) = 2^9 = 512 (右結合)
	"4 * (2 + 3) ^ 2 - 10 / 2",   // 4 * 25 - 5 = 95
	"-2 ^ 2",                     // (-2)^2 = 4 (※結合優先順位による)
	"-(2 ^ 2)",                   // -4
	"3.5 * 2 + 1.5"               // 8.5
};


foreach (var expr in expressions)
{
	double result = MathExpressionEvaluator.Evaluate(expr);
	Console.WriteLine($"{expr} = {result}");
}
