using System;
using System.Diagnostics;
using System.Globalization;

namespace System
{
	// Token: 0x02000002 RID: 2
	[Serializable]
	public struct Half : IComparable, IFormattable, IConvertible, IComparable<Half>, IEquatable<Half>
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public Half(float value)
		{
			this = HalfHelper.SingleToHalf(value);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000205E File Offset: 0x0000025E
		public Half(int value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		public Half(long value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002072 File Offset: 0x00000272
		public Half(double value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000207C File Offset: 0x0000027C
		public Half(decimal value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000208B File Offset: 0x0000028B
		public Half(uint value)
		{
			this = new Half(value);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002096 File Offset: 0x00000296
		public Half(ulong value)
		{
			this = new Half(value);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020A1 File Offset: 0x000002A1
		public static Half Negate(Half half)
		{
			return -half;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020A9 File Offset: 0x000002A9
		public static Half Add(Half half1, Half half2)
		{
			return half1 + half2;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000020B2 File Offset: 0x000002B2
		public static Half Subtract(Half half1, Half half2)
		{
			return half1 - half2;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000020BB File Offset: 0x000002BB
		public static Half Multiply(Half half1, Half half2)
		{
			return half1 * half2;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000020C4 File Offset: 0x000002C4
		public static Half Divide(Half half1, Half half2)
		{
			return half1 / half2;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000020CD File Offset: 0x000002CD
		public static Half operator +(Half half)
		{
			return half;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000020D0 File Offset: 0x000002D0
		public static Half operator -(Half half)
		{
			return HalfHelper.Negate(half);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000020D8 File Offset: 0x000002D8
		public static Half operator ++(Half half)
		{
			return (Half)(half + 1f);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000020EB File Offset: 0x000002EB
		public static Half operator --(Half half)
		{
			return (Half)(half - 1f);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000020FE File Offset: 0x000002FE
		public static Half operator +(Half half1, Half half2)
		{
			return (Half)(half1 + half2);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002113 File Offset: 0x00000313
		public static Half operator -(Half half1, Half half2)
		{
			return (Half)(half1 - half2);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002128 File Offset: 0x00000328
		public static Half operator *(Half half1, Half half2)
		{
			return (Half)(half1 * half2);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000213D File Offset: 0x0000033D
		public static Half operator /(Half half1, Half half2)
		{
			return (Half)(half1 / half2);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002152 File Offset: 0x00000352
		public static bool operator ==(Half half1, Half half2)
		{
			return !Half.IsNaN(half1) && half1.Value == half2.Value;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000216C File Offset: 0x0000036C
		public static bool operator !=(Half half1, Half half2)
		{
			return half1.Value != half2.Value;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x0000217F File Offset: 0x0000037F
		public static bool operator <(Half half1, Half half2)
		{
			return half1 < half2;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002190 File Offset: 0x00000390
		public static bool operator >(Half half1, Half half2)
		{
			return half1 > half2;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000021A1 File Offset: 0x000003A1
		public static bool operator <=(Half half1, Half half2)
		{
			return half1 == half2 || half1 < half2;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000021B5 File Offset: 0x000003B5
		public static bool operator >=(Half half1, Half half2)
		{
			return half1 == half2 || half1 > half2;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000021C9 File Offset: 0x000003C9
		public static implicit operator Half(byte value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000021D2 File Offset: 0x000003D2
		public static implicit operator Half(short value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000021DB File Offset: 0x000003DB
		public static implicit operator Half(char value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000021E4 File Offset: 0x000003E4
		public static implicit operator Half(int value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000021ED File Offset: 0x000003ED
		public static implicit operator Half(long value)
		{
			return new Half((float)value);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000021F6 File Offset: 0x000003F6
		public static explicit operator Half(float value)
		{
			return new Half(value);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000021FE File Offset: 0x000003FE
		public static explicit operator Half(double value)
		{
			return new Half((float)value);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002207 File Offset: 0x00000407
		public static explicit operator Half(decimal value)
		{
			return new Half((float)value);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002215 File Offset: 0x00000415
		public static explicit operator byte(Half value)
		{
			return (byte)value;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000221F File Offset: 0x0000041F
		public static explicit operator char(Half value)
		{
			return (char)value;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002229 File Offset: 0x00000429
		public static explicit operator short(Half value)
		{
			return (short)value;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002233 File Offset: 0x00000433
		public static explicit operator int(Half value)
		{
			return (int)value;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000223D File Offset: 0x0000043D
		public static explicit operator long(Half value)
		{
			return (long)value;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002247 File Offset: 0x00000447
		public static implicit operator float(Half value)
		{
			return HalfHelper.HalfToSingle(value);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x0000224F File Offset: 0x0000044F
		public static implicit operator double(Half value)
		{
			return (double)value;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002259 File Offset: 0x00000459
		public static explicit operator decimal(Half value)
		{
			return (decimal)value;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002267 File Offset: 0x00000467
		public static implicit operator Half(sbyte value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002270 File Offset: 0x00000470
		public static implicit operator Half(ushort value)
		{
			return new Half((float)value);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002279 File Offset: 0x00000479
		public static implicit operator Half(uint value)
		{
			return new Half(value);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002283 File Offset: 0x00000483
		public static implicit operator Half(ulong value)
		{
			return new Half(value);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x0000228D File Offset: 0x0000048D
		public static explicit operator sbyte(Half value)
		{
			return (sbyte)value;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002297 File Offset: 0x00000497
		public static explicit operator ushort(Half value)
		{
			return (ushort)value;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000022A1 File Offset: 0x000004A1
		public static explicit operator uint(Half value)
		{
			return (uint)value;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000022AB File Offset: 0x000004AB
		public static explicit operator ulong(Half value)
		{
			return (ulong)value;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000022B8 File Offset: 0x000004B8
		public int CompareTo(Half other)
		{
			int result = 0;
			if (this < other)
			{
				result = -1;
			}
			else if (this > other)
			{
				result = 1;
			}
			else if (this != other)
			{
				if (!Half.IsNaN(this))
				{
					result = 1;
				}
				else if (!Half.IsNaN(other))
				{
					result = -1;
				}
			}
			return result;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002318 File Offset: 0x00000518
		public int CompareTo(object obj)
		{
			int result;
			if (obj == null)
			{
				result = 1;
			}
			else
			{
				if (!(obj is Half))
				{
					throw new ArgumentException("Object must be of type Half.");
				}
				result = this.CompareTo((Half)obj);
			}
			return result;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002351 File Offset: 0x00000551
		public bool Equals(Half other)
		{
			return other == this || (Half.IsNaN(other) && Half.IsNaN(this));
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002378 File Offset: 0x00000578
		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj is Half)
			{
				Half half = (Half)obj;
				if (half == this || (Half.IsNaN(half) && Half.IsNaN(this)))
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000023BC File Offset: 0x000005BC
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000023C9 File Offset: 0x000005C9
		public TypeCode GetTypeCode()
		{
			return (TypeCode)255;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000023D0 File Offset: 0x000005D0
		public static byte[] GetBytes(Half value)
		{
			return BitConverter.GetBytes(value.Value);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000023DD File Offset: 0x000005DD
		public static ushort GetBits(Half value)
		{
			return value.Value;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x000023E5 File Offset: 0x000005E5
		public static Half ToHalf(byte[] value, int startIndex)
		{
			return Half.ToHalf((ushort)BitConverter.ToInt16(value, startIndex));
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000023F4 File Offset: 0x000005F4
		public static Half ToHalf(ushort bits)
		{
			return new Half
			{
				Value = bits
			};
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002412 File Offset: 0x00000612
		public static int Sign(Half value)
		{
			if (value < 0)
			{
				return -1;
			}
			if (value > 0)
			{
				return 1;
			}
			if (value != 0)
			{
				throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
			}
			return 0;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000244E File Offset: 0x0000064E
		public static Half Abs(Half value)
		{
			return HalfHelper.Abs(value);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002456 File Offset: 0x00000656
		public static Half Max(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value1;
			}
			return value2;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002464 File Offset: 0x00000664
		public static Half Min(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value2;
			}
			return value1;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002472 File Offset: 0x00000672
		public static bool IsNaN(Half half)
		{
			return HalfHelper.IsNaN(half);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x0000247A File Offset: 0x0000067A
		public static bool IsInfinity(Half half)
		{
			return HalfHelper.IsInfinity(half);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002482 File Offset: 0x00000682
		public static bool IsNegativeInfinity(Half half)
		{
			return HalfHelper.IsNegativeInfinity(half);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000248A File Offset: 0x0000068A
		public static bool IsPositiveInfinity(Half half)
		{
			return HalfHelper.IsPositiveInfinity(half);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002492 File Offset: 0x00000692
		public static Half Parse(string value)
		{
			return (Half)float.Parse(value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000024A4 File Offset: 0x000006A4
		public static Half Parse(string value, IFormatProvider provider)
		{
			return (Half)float.Parse(value, provider);
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000024B2 File Offset: 0x000006B2
		public static Half Parse(string value, NumberStyles style)
		{
			return (Half)float.Parse(value, style, CultureInfo.InvariantCulture);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000024C5 File Offset: 0x000006C5
		public static Half Parse(string value, NumberStyles style, IFormatProvider provider)
		{
			return (Half)float.Parse(value, style, provider);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000024D4 File Offset: 0x000006D4
		public static bool TryParse(string value, out Half result)
		{
			float value2;
			if (float.TryParse(value, out value2))
			{
				result = (Half)value2;
				return true;
			}
			result = default(Half);
			return false;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00002504 File Offset: 0x00000704
		public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out Half result)
		{
			bool result2 = false;
			float value2;
			if (float.TryParse(value, style, provider, out value2))
			{
				result = (Half)value2;
				result2 = true;
			}
			else
			{
				result = default(Half);
			}
			return result2;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00002538 File Offset: 0x00000738
		public override string ToString()
		{
			return this.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002560 File Offset: 0x00000760
		public string ToString(IFormatProvider formatProvider)
		{
			return this.ToString(formatProvider);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002584 File Offset: 0x00000784
		public string ToString(string format)
		{
			return this.ToString(format, CultureInfo.InvariantCulture);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000025AC File Offset: 0x000007AC
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000025CF File Offset: 0x000007CF
		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return this;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000025DC File Offset: 0x000007DC
		TypeCode IConvertible.GetTypeCode()
		{
			return this.GetTypeCode();
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000025E4 File Offset: 0x000007E4
		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000025F6 File Offset: 0x000007F6
		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00002608 File Offset: 0x00000808
		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"));
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00002628 File Offset: 0x00000828
		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"));
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00002648 File Offset: 0x00000848
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000265A File Offset: 0x0000085A
		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000266C File Offset: 0x0000086C
		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x0000267E File Offset: 0x0000087E
		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00002690 File Offset: 0x00000890
		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000026A2 File Offset: 0x000008A2
		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000026B4 File Offset: 0x000008B4
		string IConvertible.ToString(IFormatProvider provider)
		{
			return Convert.ToString(this, CultureInfo.InvariantCulture);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000026CB File Offset: 0x000008CB
		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return ((IConvertible)this).ToType(conversionType, provider);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000026E5 File Offset: 0x000008E5
		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000026F7 File Offset: 0x000008F7
		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002709 File Offset: 0x00000909
		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}

		// Token: 0x04000001 RID: 1
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal ushort Value;

		// Token: 0x04000002 RID: 2
		public static readonly Half Epsilon = Half.ToHalf(1);

		// Token: 0x04000003 RID: 3
		public static readonly Half MaxValue = Half.ToHalf(31743);

		// Token: 0x04000004 RID: 4
		public static readonly Half MinValue = Half.ToHalf(64511);

		// Token: 0x04000005 RID: 5
		public static readonly Half NaN = Half.ToHalf(65024);

		// Token: 0x04000006 RID: 6
		public static readonly Half NegativeInfinity = Half.ToHalf(64512);

		// Token: 0x04000007 RID: 7
		public static readonly Half PositiveInfinity = Half.ToHalf(31744);
	}
}
