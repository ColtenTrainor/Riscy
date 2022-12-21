
namespace Riscy;

public static class Extensions
{
	public static string ToOutString(this byte num, string format)
	{
		return format switch
		{
			"b" => Convert.ToString(num, 2).PadLeft(8, '0'),
			"x" => num.ToString("x2"),
			"d" => num.ToString("d3"),
			_ => "INVALID FORMAT PARAMETER"
		};
	}
	public static string ToOutString4Bit(this byte num, string format)
	{
		return format switch
		{
			"b" => Convert.ToString(num, 2).PadLeft(4, '0'),
			"x" => num.ToString("x1"),
			"d" => num.ToString("d2"),
			_ => "INVALID FORMAT PARAMETER"
		};
	}
	public static string ToOutString2Bit(this byte num, string format)
	{
		return format switch
		{
			"b" => Convert.ToString(num, 2).PadLeft(2, '0'),
			"x" => num.ToString("x1"),
			"d" => num.ToString("d1"),
			"n" => Convert.ToChar(num + 65).ToString(),
			_ => "INVALID FORMAT PARAMETER"
		};
	}
}