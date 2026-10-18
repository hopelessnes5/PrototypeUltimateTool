using System;

namespace System
{
	// Token: 0x02000003 RID: 3
	internal static class HalfHelper
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00002780 File Offset: 0x00000980
		private static uint ConvertMantissa(int i)
		{
			uint num = (uint)((uint)i << 13);
			uint num2 = 0U;
			while ((num & 8388608U) == 0U)
			{
				num2 -= 8388608U;
				num <<= 1;
			}
			num &= 4286578687U;
			num2 += 947912704U;
			return num | num2;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000027C0 File Offset: 0x000009C0
		private static uint[] GenerateMantissaTable()
		{
			uint[] array = new uint[2048];
			array[0] = 0U;
			for (int i = 1; i < 1024; i++)
			{
				array[i] = HalfHelper.ConvertMantissa(i);
			}
			for (int j = 1024; j < 2048; j++)
			{
				array[j] = (uint)(939524096 + (j - 1024 << 13));
			}
			return array;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002820 File Offset: 0x00000A20
		private static uint[] GenerateExponentTable()
		{
			uint[] array = new uint[64];
			array[0] = 0U;
			for (int i = 1; i < 31; i++)
			{
				array[i] = (uint)((uint)i << 23);
			}
			array[31] = 1199570944U;
			array[32] = 2147483648U;
			for (int j = 33; j < 63; j++)
			{
				array[j] = unchecked((uint)(int.MinValue + ((j - 32) << 23)));
			}
			array[63] = 3347054592U;
			return array;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x0000288C File Offset: 0x00000A8C
		private static ushort[] GenerateOffsetTable()
		{
			ushort[] array = new ushort[64];
			array[0] = 0;
			for (int i = 1; i < 32; i++)
			{
				array[i] = 1024;
			}
			array[32] = 0;
			for (int j = 33; j < 64; j++)
			{
				array[j] = 1024;
			}
			return array;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x000028D8 File Offset: 0x00000AD8
		private static ushort[] GenerateBaseTable()
		{
			ushort[] array = new ushort[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 0;
					array[i | 256] = 32768;
				}
				else if (b > 14)
				{
					array[i | 0] = (ushort)(1024 >> (int)(18 + b));
					array[i | 256] = (ushort)(1024 >> (int)(18 + b) | 32768);
				}
				else if (b >= -15)
				{
					array[i | 0] = (ushort)(15 - b << 10);
					array[i | 256] = (ushort)((int)(15 - b) << 10 | 32768);
				}
				else if (b > -128)
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
				else
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
			}
			return array;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000029C4 File Offset: 0x00000BC4
		private static sbyte[] GenerateShiftTable()
		{
			sbyte[] array = new sbyte[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else if (b > 14)
				{
					array[i | 0] = (sbyte)(b - 1);
					array[i | 256] = (sbyte)(b - 1);
				}
				else if (b >= -15)
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
				else if (b > -128)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
			}
			return array;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002A74 File Offset: 0x00000C74
		public unsafe static float HalfToSingle(Half half)
		{
			uint num = HalfHelper.MantissaTable[(int)(HalfHelper.OffsetTable[half.Value >> 10] + (half.Value & 1023))] + HalfHelper.ExponentTable[half.Value >> 10];
			return *(float*)(&num);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002AB8 File Offset: 0x00000CB8
		public unsafe static Half SingleToHalf(float single)
		{
			uint num = *(uint*)(&single);
			return Half.ToHalf((ushort)((uint)HalfHelper.BaseTable[(int)(num >> 23 & 511U)] + ((num & 8388607U) >> (int)HalfHelper.ShiftTable[(int)(num >> 23)])));
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00002AF6 File Offset: 0x00000CF6
		public static Half Negate(Half half)
		{
			return Half.ToHalf((ushort)(half.Value ^ 32768));
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002B0A File Offset: 0x00000D0A
		public static Half Abs(Half half)
		{
			return Half.ToHalf((ushort)(half.Value & 32767));
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002B1E File Offset: 0x00000D1E
		public static bool IsNaN(Half half)
		{
			return (half.Value & 32767) > 31744;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002B33 File Offset: 0x00000D33
		public static bool IsInfinity(Half half)
		{
			return (half.Value & 32767) == 31744;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00002B48 File Offset: 0x00000D48
		public static bool IsPositiveInfinity(Half half)
		{
			return half.Value == 31744;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002B57 File Offset: 0x00000D57
		public static bool IsNegativeInfinity(Half half)
		{
			return half.Value == 64512;
		}

		// Token: 0x04000008 RID: 8
		private static readonly uint[] MantissaTable = HalfHelper.GenerateMantissaTable();

		// Token: 0x04000009 RID: 9
		private static readonly uint[] ExponentTable = HalfHelper.GenerateExponentTable();

		// Token: 0x0400000A RID: 10
		private static readonly ushort[] OffsetTable = HalfHelper.GenerateOffsetTable();

		// Token: 0x0400000B RID: 11
		private static readonly ushort[] BaseTable = HalfHelper.GenerateBaseTable();

		// Token: 0x0400000C RID: 12
		private static readonly sbyte[] ShiftTable = HalfHelper.GenerateShiftTable();
	}
}
