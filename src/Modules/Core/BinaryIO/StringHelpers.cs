using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gibbed.IO
{
	// Token: 0x02000009 RID: 9
	public static class StringHelpers
	{
		// Token: 0x0600009C RID: 156 RVA: 0x000032D0 File Offset: 0x000014D0
		public static uint HashFileName(this string input, uint seed)
		{
			if (input.StartsWith("\\"))
			{
				input = input.Substring(1);
			}
			foreach (byte b in Encoding.ASCII.GetBytes(input))
			{
				seed = (seed << 5) - seed;
				if (b < 97)
				{
					seed += (uint)(32 + b);
				}
				else
				{
					seed += (uint)b;
				}
			}
			return seed;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000332D File Offset: 0x0000152D
		public static uint HashFileName(this string input)
		{
			return input.HashFileName(0U);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00003336 File Offset: 0x00001536
		public static ulong HashX65599(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return 0UL;
			}
			return input.Aggregate(0UL, (ulong current, char t) => current * 65599UL ^ (ulong)t);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000336A File Offset: 0x0000156A
		public static string SeparateCamelCase(this string input)
		{
			return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
		}
	}
}
