using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Gibbed.IO
{
	// Token: 0x02000008 RID: 8
	public static class StreamHelpers
	{
		// Token: 0x06000041 RID: 65 RVA: 0x0000265A File Offset: 0x0000085A
		internal static bool ShouldSwap(Endian endian)
		{
			if (endian == Endian.Little)
			{
				return !BitConverter.IsLittleEndian;
			}
			if (endian != Endian.Big)
			{
				throw new ArgumentException("unsupported endianness", "endian");
			}
			return BitConverter.IsLittleEndian;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002683 File Offset: 0x00000883
		public static int PeakByte(this Stream stream)
		{
			int result = stream.ReadByte();
			stream.Position -= 1L;
			return result;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x0000269C File Offset: 0x0000089C
		public static void MoveToAlign(this Stream stream, int aligment)
		{
			while (stream.Position % (long)aligment != 0L)
			{
				long position = stream.Position + 1L;
				stream.Position = position;
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000026C7 File Offset: 0x000008C7
		public static MemoryStream ReadToMemoryStream(this Stream stream, int size, bool writeable)
		{
			return new MemoryStream(stream.ReadBytes(size), writeable);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000026D6 File Offset: 0x000008D6
		public static MemoryStream ReadToMemoryStream(this Stream stream, int size)
		{
			return stream.ReadToMemoryStream(size, true);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000026E0 File Offset: 0x000008E0
		public static void WriteFromStream(this Stream stream, Stream input, long size, int buffer)
		{
			long num = size;
			byte[] buffer2 = new byte[buffer];
			while (num > 0L)
			{
				int num2 = (int)Math.Min(num, (long)buffer);
				input.ReadExactly(buffer2, 0, num2);
				stream.Write(buffer2, 0, num2);
				num -= (long)num2;
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00002727 File Offset: 0x00000927
		public static void WriteFromStream(this Stream stream, Stream input, long size)
		{
			stream.WriteFromStream(input, size, 262144);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002738 File Offset: 0x00000938
		public static byte[] ReadBytes(this Stream stream, int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			byte[] array = new byte[length];
			stream.ReadExactly(array, 0, length);
			return array;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x0000276E File Offset: 0x0000096E
		public static void WriteBytes(this Stream stream, byte[] data)
		{
			stream.Write(data, 0, data.Length);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x0000277C File Offset: 0x0000097C
		public static string ReadStringAlignedU32(this Stream stream, Endian endian)
		{
			uint num = stream.ReadValueU32(endian);
			if (num == 0U)
			{
				return "";
			}
			byte[] array = new byte[num];
			stream.ReadAligned(array, 0, array.Length, 4);
			return Encoding.ASCII.GetString(array);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000027BC File Offset: 0x000009BC
		public static void WriteStringU32(this Stream stream, string value, Endian endian)
		{
			if (string.IsNullOrEmpty(value))
			{
				stream.WriteValueU32(0U, endian);
				return;
			}
			int byteCount = Encoding.ASCII.GetByteCount(value);
			stream.WriteValueS32(byteCount + 1, endian);
			stream.WriteStringZ(value, Encoding.ASCII);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000027FC File Offset: 0x000009FC
		public static void WriteStringU32NoTerminator(this Stream stream, string value, Endian endian)
		{
			if (string.IsNullOrEmpty(value))
			{
				stream.WriteValueU32(0U, endian);
				return;
			}
			stream.WriteValueS32(value.Length, endian);
			stream.WriteStringInternalStatic(StreamHelpers.DefaultEncoding, value);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002828 File Offset: 0x00000A28
		public static void WriteStringAlignedU32(this Stream stream, string value, Endian endian)
		{
			if (string.IsNullOrEmpty(value))
			{
				stream.WriteValueU32(0U, endian);
				return;
			}
			int byteCount = Encoding.ASCII.GetByteCount(value);
			stream.WriteValueS32(byteCount, endian);
			stream.WriteAligned(Encoding.ASCII.GetBytes(value), 0, value.Length, 4);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002874 File Offset: 0x00000A74
		public static string ReadStringAlignedU8(this Stream stream)
		{
			byte b = stream.ReadValueU8();
			if (b == 0)
			{
				return "";
			}
			return stream.ReadString((int)b, false, Encoding.ASCII);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000028A0 File Offset: 0x00000AA0
		public static void WriteStringAlignedU8(this Stream stream, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				stream.WriteValueU8(0);
				return;
			}
			int byteCount = Encoding.ASCII.GetByteCount(value);
			if (byteCount > 255)
			{
				throw new ArgumentException("value is too long", "value");
			}
			stream.WriteValueU8((byte)byteCount);
			byte[] bytes = Encoding.ASCII.GetBytes(value);
			Array.Resize<byte>(ref bytes, byteCount);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002908 File Offset: 0x00000B08
		public static int ReadAligned(this Stream stream, byte[] buffer, int offset, int size, int align)
		{
			if (size == 0)
			{
				return 0;
			}
			if (align <= 0)
			{
				throw new ArgumentOutOfRangeException("align");
			}
			stream.ReadExactly(buffer, offset, size);
			int result = size;
			int num = size % align;
			if (num > 0)
			{
				stream.Seek((long)(align - num), SeekOrigin.Current);
			}
			return result;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x0000293C File Offset: 0x00000B3C
		public static void WriteAligned(this Stream stream, byte[] buffer, int offset, int size, int align)
		{
			if (size == 0)
			{
				return;
			}
			stream.Write(buffer, offset, size);
			int num = size % align;
			if (num > 0)
			{
				byte[] buffer2 = new byte[align - num];
				stream.Write(buffer2, 0, align - num);
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002975 File Offset: 0x00000B75
		public static bool ReadValueBoolean(this Stream stream)
		{
			return stream.ReadValueB8();
		}

		// Token: 0x06000053 RID: 83 RVA: 0x0000297D File Offset: 0x00000B7D
		public static void WriteValueBoolean(this Stream stream, bool value)
		{
			stream.WriteValueB8(value);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00002986 File Offset: 0x00000B86
		public static bool ReadValueB8(this Stream stream)
		{
			return stream.ReadValueU8() > 0;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00002991 File Offset: 0x00000B91
		public static void WriteValueB8(this Stream stream, bool value)
		{
			stream.WriteValueU8((byte)(value ? 1 : 0));
		}

		// Token: 0x06000056 RID: 86 RVA: 0x000029A1 File Offset: 0x00000BA1
		public static bool ReadValueB32(this Stream stream, Endian endian)
		{
			return stream.ReadValueU32(endian) > 0U;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x000029AD File Offset: 0x00000BAD
		public static bool ReadValueB32(this Stream stream)
		{
			return stream.ReadValueB32(Endian.Little);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000029B6 File Offset: 0x00000BB6
		public static void WriteValueB32(this Stream stream, bool value, Endian endian)
		{
			stream.WriteValueU32((uint)(value ? 1 : 0), endian);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000029C7 File Offset: 0x00000BC7
		public static void WriteValueB32(this Stream stream, bool value)
		{
			stream.WriteValueB32(value, Endian.Little);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000029D4 File Offset: 0x00000BD4
		public static T ReadValueEnum<T>(this Stream stream, Endian endian)
		{
			Type typeFromHandle = typeof(T);
			object value;
			switch (StreamHelpers.EnumTypeCache.Get(typeFromHandle))
			{
			case TypeCode.SByte:
				value = stream.ReadValueS8();
				break;
			case TypeCode.Byte:
				value = stream.ReadValueU8();
				break;
			case TypeCode.Int16:
				value = stream.ReadValueS16(endian);
				break;
			case TypeCode.UInt16:
				value = stream.ReadValueU16(endian);
				break;
			case TypeCode.Int32:
				value = stream.ReadValueS32(endian);
				break;
			case TypeCode.UInt32:
				value = stream.ReadValueU32(endian);
				break;
			case TypeCode.Int64:
				value = stream.ReadValueS64(endian);
				break;
			case TypeCode.UInt64:
				value = stream.ReadValueU64(endian);
				break;
			default:
				throw new NotSupportedException();
			}
			return (T)((object)Enum.ToObject(typeFromHandle, value));
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00002AA5 File Offset: 0x00000CA5
		public static T ReadValueEnum<T>(this Stream stream)
		{
			return stream.ReadValueEnum<T>(Endian.Little);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00002AB0 File Offset: 0x00000CB0
		public static void WriteValueEnum<T>(this Stream stream, object value, Endian endian)
		{
			switch (StreamHelpers.EnumTypeCache.Get(typeof(T)))
			{
			case TypeCode.SByte:
				stream.WriteValueS8((sbyte)value);
				return;
			case TypeCode.Byte:
				stream.WriteValueU8((byte)value);
				return;
			case TypeCode.Int16:
				stream.WriteValueS16((short)value, endian);
				return;
			case TypeCode.UInt16:
				stream.WriteValueU16((ushort)value, endian);
				return;
			case TypeCode.Int32:
				stream.WriteValueS32((int)value, endian);
				return;
			case TypeCode.UInt32:
				stream.WriteValueU32((uint)value, endian);
				return;
			case TypeCode.Int64:
				stream.WriteValueS64((long)value, endian);
				return;
			case TypeCode.UInt64:
				stream.WriteValueU64((ulong)value, endian);
				return;
			default:
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00002B6A File Offset: 0x00000D6A
		public static void WriteValueEnum<T>(this Stream stream, object value)
		{
			stream.WriteValueEnum<T>(value, Endian.Little);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00002B74 File Offset: 0x00000D74
		public static Guid ReadValueGuid(this Stream stream, Endian endian)
		{
			int a = stream.ReadValueS32(endian);
			short b = stream.ReadValueS16(endian);
			short c = stream.ReadValueS16(endian);
			byte[] d = SmallWorkBuffer.ReadBytes(stream, 8);
			return new Guid(a, b, c, d);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002BA8 File Offset: 0x00000DA8
		public static Guid ReadValueGuid(this Stream stream)
		{
			return stream.ReadValueGuid(Endian.Little);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002BB4 File Offset: 0x00000DB4
		public static void WriteValueGuid(this Stream stream, Guid value, Endian endian)
		{
			byte[] array = value.ToByteArray();
			stream.WriteValueS32(BitConverter.ToInt32(array, 0), endian);
			stream.WriteValueS16(BitConverter.ToInt16(array, 4), endian);
			stream.WriteValueS16(BitConverter.ToInt16(array, 6), endian);
			stream.Write(array, 8, 8);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002BFC File Offset: 0x00000DFC
		public static void WriteValueGuid(this Stream stream, Guid value)
		{
			stream.WriteValueGuid(value, Endian.Little);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002C08 File Offset: 0x00000E08
		public static float ReadValueF32(this Stream stream, Endian endian)
		{
			return new OverlapSingle(stream.ReadValueU32(endian)).AsF;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002C29 File Offset: 0x00000E29
		public static float ReadValueF32(this Stream stream)
		{
			return stream.ReadValueF32(Endian.Little);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002C34 File Offset: 0x00000E34
		public static void WriteValueF32(this Stream stream, float value, Endian endian)
		{
			uint asU = new OverlapSingle(value).AsU;
			stream.WriteValueU32(asU, endian);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002C58 File Offset: 0x00000E58
		public static void WriteValueF32(this Stream stream, float value)
		{
			stream.WriteValueF32(value, Endian.Little);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00002C64 File Offset: 0x00000E64
		public static double ReadValueF64(this Stream stream, Endian endian)
		{
			return new OverlapDouble(stream.ReadValueU64(endian)).AsD;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002C85 File Offset: 0x00000E85
		public static double ReadValueF64(this Stream stream)
		{
			return stream.ReadValueF64(Endian.Little);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002C90 File Offset: 0x00000E90
		public static void WriteValueF64(this Stream stream, double value, Endian endian)
		{
			ulong asU = new OverlapDouble(value).AsU;
			stream.WriteValueU64(asU, endian);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00002CB4 File Offset: 0x00000EB4
		public static void WriteValueF64(this Stream stream, double value)
		{
			stream.WriteValueF64(value, Endian.Little);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002CBE File Offset: 0x00000EBE
		public static sbyte ReadValueS8(this Stream stream)
		{
			return (sbyte)stream.ReadValueU8();
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002CC7 File Offset: 0x00000EC7
		public static void WriteValueS8(this Stream stream, sbyte value)
		{
			stream.WriteValueU8((byte)value);
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002CD1 File Offset: 0x00000ED1
		public static short ReadValueS16(this Stream stream, Endian endian)
		{
			return (short)stream.ReadValueU16(endian);
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00002CDB File Offset: 0x00000EDB
		public static short ReadValueS16(this Stream stream)
		{
			return (short)stream.ReadValueU16();
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002CE4 File Offset: 0x00000EE4
		public static void WriteValueS16(this Stream stream, short value, Endian endian)
		{
			stream.WriteValueU16((ushort)value, endian);
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00002CEF File Offset: 0x00000EEF
		public static void WriteValueS16(this Stream stream, short value)
		{
			stream.WriteValueU16((ushort)value);
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00002CF9 File Offset: 0x00000EF9
		public static int ReadValueS32(this Stream stream, Endian endian)
		{
			return (int)stream.ReadValueU32(endian);
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002D02 File Offset: 0x00000F02
		public static int ReadValueS32(this Stream stream)
		{
			return (int)stream.ReadValueU32();
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00002D0A File Offset: 0x00000F0A
		public static void WriteValueS32(this Stream stream, int value, Endian endian)
		{
			stream.WriteValueU32((uint)value, endian);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00002D14 File Offset: 0x00000F14
		public static void WriteValueS32(this Stream stream, int value)
		{
			stream.WriteValueU32((uint)value);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00002D1D File Offset: 0x00000F1D
		public static long ReadValueS64(this Stream stream, Endian endian)
		{
			return (long)stream.ReadValueU64(endian);
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00002D26 File Offset: 0x00000F26
		public static long ReadValueS64(this Stream stream)
		{
			return (long)stream.ReadValueU64(Endian.Little);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00002D2F File Offset: 0x00000F2F
		public static void WriteValueS64(this Stream stream, long value, Endian endian)
		{
			stream.WriteValueU64((ulong)value, endian);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00002D39 File Offset: 0x00000F39
		public static void WriteValueS64(this Stream stream, long value)
		{
			stream.WriteValueU64((ulong)value);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002D42 File Offset: 0x00000F42
		public static byte ReadValueU8(this Stream stream)
		{
			return SmallWorkBuffer.ReadBytes(stream, 1)[0];
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00002D50 File Offset: 0x00000F50
		public static void WriteValueU8(this Stream stream, byte value)
		{
			byte[] array = SmallWorkBuffer.Get(1);
			array[0] = value;
			stream.Write(array, 0, 1);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00002D74 File Offset: 0x00000F74
		public static ushort ReadValueU16(this Stream stream, Endian endian)
		{
			ushort num = BitConverter.ToUInt16(SmallWorkBuffer.ReadBytes(stream, 2), 0);
			if (StreamHelpers.ShouldSwap(endian))
			{
				num = num.Swap();
			}
			return num;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00002D9F File Offset: 0x00000F9F
		public static ushort ReadValueU16(this Stream stream)
		{
			return stream.ReadValueU16(Endian.Little);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002DA8 File Offset: 0x00000FA8
		public static void WriteValueU16(this Stream stream, ushort value, Endian endian)
		{
			if (StreamHelpers.ShouldSwap(endian))
			{
				value = value.Swap();
			}
			byte[] array = SmallWorkBuffer.Get(2);
			array[0] = (byte)(value & 255);
			array[1] = (byte)((uint)(value & 65280) >> 8);
			stream.Write(array, 0, 2);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00002DED File Offset: 0x00000FED
		public static void WriteValueU16(this Stream stream, ushort value)
		{
			stream.WriteValueU16(value, Endian.Little);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00002DF8 File Offset: 0x00000FF8
		public static uint ReadValueU32(this Stream stream, Endian endian)
		{
			uint num = BitConverter.ToUInt32(SmallWorkBuffer.ReadBytes(stream, 4), 0);
			if (StreamHelpers.ShouldSwap(endian))
			{
				num = num.Swap();
			}
			return num;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00002E23 File Offset: 0x00001023
		public static uint ReadValueU32(this Stream stream)
		{
			return stream.ReadValueU32(Endian.Little);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00002E2C File Offset: 0x0000102C
		public static void WriteValueU32(this Stream stream, uint value, Endian endian)
		{
			if (StreamHelpers.ShouldSwap(endian))
			{
				value = value.Swap();
			}
			byte[] array = SmallWorkBuffer.Get(4);
			array[0] = (byte)(value & 255U);
			array[1] = (byte)((value & 65280U) >> 8);
			array[2] = (byte)((value & 16711680U) >> 16);
			array[3] = (byte)((value & 4278190080U) >> 24);
			stream.Write(array, 0, 4);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00002E8D File Offset: 0x0000108D
		public static void WriteValueU32(this Stream stream, uint value)
		{
			stream.WriteValueU32(value, Endian.Little);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002E98 File Offset: 0x00001098
		public static ulong ReadValueU64(this Stream stream, Endian endian)
		{
			ulong num = BitConverter.ToUInt64(SmallWorkBuffer.ReadBytes(stream, 8), 0);
			if (StreamHelpers.ShouldSwap(endian))
			{
				num = num.Swap();
			}
			return num;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00002EC3 File Offset: 0x000010C3
		public static ulong ReadValueU64(this Stream stream)
		{
			return stream.ReadValueU64(Endian.Little);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00002ECC File Offset: 0x000010CC
		public static void WriteValueU64(this Stream stream, ulong value, Endian endian)
		{
			if (StreamHelpers.ShouldSwap(endian))
			{
				value = value.Swap();
			}
			byte[] array = SmallWorkBuffer.Get(8);
			uint num = (uint)value;
			uint num2 = (uint)(value >> 32);
			array[0] = (byte)(num & 255U);
			array[1] = (byte)((num & 65280U) >> 8);
			array[2] = (byte)((num & 16711680U) >> 16);
			array[3] = (byte)((num & 4278190080U) >> 24);
			array[4] = (byte)(num2 & 255U);
			array[5] = (byte)((num2 & 65280U) >> 8);
			array[6] = (byte)((num2 & 16711680U) >> 16);
			array[7] = (byte)((num2 & 4278190080U) >> 24);
			stream.Write(array, 0, 8);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00002F6A File Offset: 0x0000116A
		public static void WriteValueU64(this Stream stream, ulong value)
		{
			stream.WriteValueU64(value, Endian.Little);
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00002F74 File Offset: 0x00001174
		public static string ReadString(this Stream stream, int size)
		{
			return stream.ReadStringInternalStatic(StreamHelpers.DefaultEncoding, size, false);
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00002F83 File Offset: 0x00001183
		public static string ReadString(this Stream stream, int size, bool trailingNull)
		{
			return stream.ReadStringInternalStatic(StreamHelpers.DefaultEncoding, size, trailingNull);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00002F92 File Offset: 0x00001192
		public static string ReadStringZ(this Stream stream)
		{
			return stream.ReadStringInternalDynamic(StreamHelpers.DefaultEncoding, '\0');
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00002FA0 File Offset: 0x000011A0
		public static void WriteString(this Stream stream, string value)
		{
			stream.WriteStringInternalStatic(StreamHelpers.DefaultEncoding, value);
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00002FAE File Offset: 0x000011AE
		public static void WriteString(this Stream stream, string value, int size)
		{
			stream.WriteStringInternalStatic(StreamHelpers.DefaultEncoding, value, size);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00002FBD File Offset: 0x000011BD
		public static void WriteStringZ(this Stream stream, string value)
		{
			stream.WriteStringInternalDynamic(StreamHelpers.DefaultEncoding, value, '\0');
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00002FCC File Offset: 0x000011CC
		internal static string ReadStringInternalStatic(this Stream stream, Encoding encoding, int size, bool trailingNull)
		{
			byte[] array = stream.ReadBytes(size);
			string text = encoding.GetString(array, 0, array.Length);
			if (trailingNull)
			{
				int num = text.IndexOf('\0');
				if (num >= 0)
				{
					text = text.Substring(0, num);
				}
			}
			return text;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003008 File Offset: 0x00001208
		internal static void WriteStringInternalStatic(this Stream stream, Encoding encoding, string value)
		{
			byte[] bytes = encoding.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003028 File Offset: 0x00001228
		internal static void WriteStringInternalStatic(this Stream stream, Encoding encoding, string value, int size)
		{
			byte[] bytes = encoding.GetBytes(value);
			Array.Resize<byte>(ref bytes, size);
			stream.Write(bytes, 0, size);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003050 File Offset: 0x00001250
		internal static string ReadStringInternalDynamic(this Stream stream, Encoding encoding, char end)
		{
			int byteCount = encoding.GetByteCount("e");
			string b = end.ToString(CultureInfo.InvariantCulture);
			int num = 0;
			byte[] array = new byte[128 * byteCount];
			for (;;)
			{
				if (num + byteCount > array.Length)
				{
					Array.Resize<byte>(ref array, array.Length + 128 * byteCount);
				}
				stream.ReadExactly(array, num, byteCount);
				if (encoding.GetString(array, num, byteCount) == b)
				{
					break;
				}
				num += byteCount;
			}
			if (num == 0)
			{
				return "";
			}
			return encoding.GetString(array, 0, num);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000030D4 File Offset: 0x000012D4
		internal static void WriteStringInternalDynamic(this Stream stream, Encoding encoding, string value, char end)
		{
			byte[] bytes = encoding.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
			bytes = encoding.GetBytes(end.ToString(CultureInfo.InvariantCulture));
			stream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003112 File Offset: 0x00001312
		public static string ReadString(this Stream stream, int size, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, size, false);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x0000311D File Offset: 0x0000131D
		public static string ReadString(this Stream stream, int size, bool trailingNull, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, size, trailingNull);
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00003128 File Offset: 0x00001328
		public static string ReadStringZ(this Stream stream, Encoding encoding)
		{
			return stream.ReadStringInternalDynamic(encoding, '\0');
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00003132 File Offset: 0x00001332
		public static void WriteString(this Stream stream, string value, Encoding encoding)
		{
			stream.WriteStringInternalStatic(encoding, value);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x0000313C File Offset: 0x0000133C
		public static void WriteString(this Stream stream, string value, int size, Encoding encoding)
		{
			stream.WriteStringInternalStatic(encoding, value, size);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003147 File Offset: 0x00001347
		public static void WriteStringZ(this Stream stream, string value, Encoding encoding)
		{
			stream.WriteStringInternalDynamic(encoding, value, '\0');
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003154 File Offset: 0x00001354
		public static T ReadStructure<T>(this Stream stream)
		{
			int num = Marshal.SizeOf(typeof(T));
			byte[] array = new byte[num];
			stream.ReadExactly(array, 0, num);
			GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				return (T)((object)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T)));
			}
			finally
			{
				gchandle.Free();
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x000031BC File Offset: 0x000013BC
		public static T ReadStructure<T>(this Stream stream, int size)
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			byte[] array = new byte[Math.Max(Marshal.SizeOf(typeof(T)), size)];
			stream.ReadExactly(array, 0, size);
			GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				return (T)((object)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T)));
			}
			finally
			{
				gchandle.Free();
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003228 File Offset: 0x00001428
		public static void WriteStructure<T>(this Stream stream, T structure)
		{
			byte[] array = new byte[Marshal.SizeOf(typeof(T))];
			GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.StructureToPtr<T>(structure, gchandle.AddrOfPinnedObject(), false);
			gchandle.Free();
			stream.Write(array, 0, array.Length);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003274 File Offset: 0x00001474
		public static void WriteStructure<T>(this Stream stream, T structure, int size)
		{
			byte[] array = new byte[Math.Max(Marshal.SizeOf(typeof(T)), size)];
			GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.StructureToPtr<T>(structure, gchandle.AddrOfPinnedObject(), false);
			gchandle.Free();
			stream.Write(array, 0, array.Length);
		}

		// Token: 0x0400000A RID: 10
		public static Encoding DefaultEncoding = Encoding.ASCII;

		// Token: 0x0200000B RID: 11
		private static class EnumTypeCache
		{
			// Token: 0x060000A3 RID: 163 RVA: 0x000033A0 File Offset: 0x000015A0
			private static TypeCode TranslateType(Type type)
			{
				if (type.IsEnum)
				{
					TypeCode typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(type));
					if (typeCode - TypeCode.SByte <= 7)
					{
						return typeCode;
					}
				}
				throw new ArgumentException("unknown enum type", "type");
			}

			// Token: 0x060000A4 RID: 164 RVA: 0x000033D8 File Offset: 0x000015D8
			public static TypeCode Get(Type type)
			{
				return StreamHelpers.EnumTypeCache.TranslateType(type);
			}
		}
	}
}
