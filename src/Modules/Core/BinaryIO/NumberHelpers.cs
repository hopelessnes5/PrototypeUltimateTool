using System;

namespace Gibbed.IO
{
	// Token: 0x02000003 RID: 3
	public static class NumberHelpers
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		public static int Align(this int value, int align)
		{
			if (value != 0)
			{
				return value + value.Padding(align);
			}
			return 0;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public static uint Align(this uint value, uint align)
		{
			if (value != 0U)
			{
				return value + value.Padding(align);
			}
			return 0U;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		public static long Align(this long value, long align)
		{
			if (value != 0L)
			{
				return value + value.Padding(align);
			}
			return 0L;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002079 File Offset: 0x00000279
		public static ulong Align(this ulong value, ulong align)
		{
			if (value != 0UL)
			{
				return value + value.Padding(align);
			}
			return 0UL;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000208A File Offset: 0x0000028A
		public static short BigEndian(this short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000209B File Offset: 0x0000029B
		public static ushort BigEndian(this ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000020AC File Offset: 0x000002AC
		public static int BigEndian(this int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020BD File Offset: 0x000002BD
		public static uint BigEndian(this uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020CE File Offset: 0x000002CE
		public static long BigEndian(this long value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000020DF File Offset: 0x000002DF
		public static ulong BigEndian(this ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000020F0 File Offset: 0x000002F0
		public static float BigEndian(this float value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0).Swap()), 0);
			}
			return value;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002117 File Offset: 0x00000317
		public static double BigEndian(this double value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return BitConverter.ToDouble(BitConverter.GetBytes(BitConverter.ToUInt64(BitConverter.GetBytes(value), 0).Swap()), 0);
			}
			return value;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000213E File Offset: 0x0000033E
		public static short LittleEndian(this short value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000214F File Offset: 0x0000034F
		public static ushort LittleEndian(this ushort value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002160 File Offset: 0x00000360
		public static int LittleEndian(this int value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002171 File Offset: 0x00000371
		public static uint LittleEndian(this uint value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002182 File Offset: 0x00000382
		public static long LittleEndian(this long value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002193 File Offset: 0x00000393
		public static ulong LittleEndian(this ulong value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000021A4 File Offset: 0x000003A4
		public static float LittleEndian(this float value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0).Swap()), 0);
			}
			return value;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000021CB File Offset: 0x000003CB
		public static double LittleEndian(this double value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return BitConverter.ToDouble(BitConverter.GetBytes(BitConverter.ToUInt64(BitConverter.GetBytes(value), 0).Swap()), 0);
			}
			return value;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000021F2 File Offset: 0x000003F2
		public static int Padding(this int value, int align)
		{
			return (align - value % align) % align;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000021FB File Offset: 0x000003FB
		public static uint Padding(this uint value, uint align)
		{
			return (align - value % align) % align;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002204 File Offset: 0x00000404
		public static long Padding(this long value, long align)
		{
			return (align - value % align) % align;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000220D File Offset: 0x0000040D
		public static ulong Padding(this ulong value, ulong align)
		{
			return (align - value % align) % align;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002216 File Offset: 0x00000416
		public static sbyte RotateLeft(this sbyte value, int count)
		{
			return (sbyte)((byte)value).RotateLeft(count);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002221 File Offset: 0x00000421
		public static byte RotateLeft(this byte value, int count)
		{
			return (byte)((int)value << count | value >> 8 - count);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002233 File Offset: 0x00000433
		public static short RotateLeft(this short value, int count)
		{
			return (short)((ushort)value).RotateLeft(count);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000223E File Offset: 0x0000043E
		public static ushort RotateLeft(this ushort value, int count)
		{
			return (ushort)((int)value << count | value >> 16 - count);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002251 File Offset: 0x00000451
		public static int RotateLeft(this int value, int count)
		{
			return (int)((uint)value).RotateLeft(count);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000225A File Offset: 0x0000045A
		public static uint RotateLeft(this uint value, int count)
		{
			return value << count | value >> 32 - count;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000226C File Offset: 0x0000046C
		public static long RotateLeft(this long value, int count)
		{
			return (long)((ulong)value).RotateLeft(count);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002275 File Offset: 0x00000475
		public static ulong RotateLeft(this ulong value, int count)
		{
			return value << count | value >> 64 - count;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002287 File Offset: 0x00000487
		public static sbyte RotateRight(this sbyte value, int count)
		{
			return (sbyte)((byte)value).RotateRight(count);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002292 File Offset: 0x00000492
		public static byte RotateRight(this byte value, int count)
		{
			return (byte)(value >> count | (int)value << 8 - count);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000022A4 File Offset: 0x000004A4
		public static short RotateRight(this short value, int count)
		{
			return (short)((ushort)value).RotateRight(count);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000022AF File Offset: 0x000004AF
		public static ushort RotateRight(this ushort value, int count)
		{
			return (ushort)(value >> count | (int)value << 16 - count);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000022C2 File Offset: 0x000004C2
		public static int RotateRight(this int value, int count)
		{
			return (int)((uint)value).RotateRight(count);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000022CB File Offset: 0x000004CB
		public static uint RotateRight(this uint value, int count)
		{
			return value >> count | value << 32 - count;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000022DD File Offset: 0x000004DD
		public static long RotateRight(this long value, int count)
		{
			return (long)((ulong)value).RotateRight(count);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000022E6 File Offset: 0x000004E6
		public static ulong RotateRight(this ulong value, int count)
		{
			return value >> count | value << 64 - count;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000022F8 File Offset: 0x000004F8
		public static short Swap(this short value)
		{
			return (short)((ushort)value).Swap();
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002302 File Offset: 0x00000502
		public static ushort Swap(this ushort value)
		{
			return (ushort)((255L & (long)(value >> 8)) | (65280L & (long)((long)value << 8)));
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000231C File Offset: 0x0000051C
		public static int Swap(this int value)
		{
			return (int)((uint)value).Swap();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002324 File Offset: 0x00000524
		public static uint Swap(this uint value)
		{
			return (255U & value >> 24) | (65280U & value >> 8) | (16711680U & value << 8) | (4278190080U & value << 24);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000234F File Offset: 0x0000054F
		public static long Swap(this long value)
		{
			return (long)((ulong)value).Swap();
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002358 File Offset: 0x00000558
		public static ulong Swap(this ulong value)
		{
			return (255UL & value >> 56) | (65280UL & value >> 40) | (16711680UL & value >> 24) | (4278190080UL & value >> 8) | (1095216660480UL & value << 8) | (280375465082880UL & value << 24) | (71776119061217280UL & value << 40) | (18374686479671623680UL & value << 56);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000023D0 File Offset: 0x000005D0
		public static float Swap(this float value)
		{
			OverlapSingle overlapSingle = new OverlapSingle(value);
			overlapSingle.AsU = overlapSingle.AsU.Swap();
			return overlapSingle.AsF;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002400 File Offset: 0x00000600
		public static double Swap(this double value)
		{
			OverlapDouble overlapDouble = new OverlapDouble(value);
			overlapDouble.AsU = overlapDouble.AsU.Swap();
			return overlapDouble.AsD;
		}
	}
}
