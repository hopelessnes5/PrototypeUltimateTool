using System;
using System.ComponentModel;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000009 RID: 9
	[TypeConverter(typeof(MyTypeConverter))]
	public class DescriptionType
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002AE8 File Offset: 0x00000CE8
		// (set) Token: 0x06000024 RID: 36 RVA: 0x00002AF0 File Offset: 0x00000CF0
		[Descriptable(true)]
		public DescriptionTypeEnum EnumValue { get; set; }

		// Token: 0x06000025 RID: 37 RVA: 0x00002AFC File Offset: 0x00000CFC
		public override string ToString()
		{
			return "Description Type (" + this.EnumValue.ToString() + ")";
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002B2C File Offset: 0x00000D2C
		public DescriptionType(uint value)
		{
			this.EnumValue = this.GetType(value);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002B41 File Offset: 0x00000D41
		public DescriptionType(string value)
		{
			this.EnumValue = this.GetType(value);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002B56 File Offset: 0x00000D56
		public DescriptionType(DescriptionTypeEnum value)
		{
			this.EnumValue = value;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002B68 File Offset: 0x00000D68
		public DescriptionTypeEnum GetType(uint value)
		{
			if (value <= 949550084U)
			{
				if (value == 3556617U)
				{
					return DescriptionTypeEnum.UV;
				}
				if (value == 747804969U)
				{
					return DescriptionTypeEnum.Position;
				}
				if (value == 949550084U)
				{
					return DescriptionTypeEnum.UVPadding1;
				}
			}
			else if (value <= 1943391143U)
			{
				if (value == 1230441723U)
				{
					return DescriptionTypeEnum.Weight;
				}
				if (value == 1943391143U)
				{
					return DescriptionTypeEnum.Group;
				}
			}
			else
			{
				if (value == 2752995909U)
				{
					return DescriptionTypeEnum.Tangent;
				}
				if (value == 3255221479U)
				{
					return DescriptionTypeEnum.Normal;
				}
			}
			this.unknownValue = value;
			return DescriptionTypeEnum.Unknown;
		}

		// Map the Prototype 2 vertex-description tokens to the shared enum.
		// The decompiler emitted an invalid compiler-generated string-hash helper;
		// direct token comparisons preserve the same mapping without that helper.
		public DescriptionTypeEnum GetType(string value)
		{
			value = (value ?? string.Empty).TrimEnd('\0');
			switch (value)
			{
				case "tex0":
					return DescriptionTypeEnum.UV;
				case "tex1":
					return DescriptionTypeEnum.UV1;
				case "pad":
					return DescriptionTypeEnum.Padding1;
				case "tangent":
					return DescriptionTypeEnum.Tangent;
				case "weights":
					return DescriptionTypeEnum.Weight;
				case "position":
					return DescriptionTypeEnum.Position;
				case "colour0":
					return DescriptionTypeEnum.Colour0;
				case "indices":
					return DescriptionTypeEnum.Group;
				case "normal":
					return DescriptionTypeEnum.Normal;
				default:
					this.unknownValue = value;
					return DescriptionTypeEnum.Unknown;
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002D24 File Offset: 0x00000F24
		public uint ToP1Type()
		{
			switch (this.EnumValue)
			{
			case DescriptionTypeEnum.Unknown:
				return (uint)this.unknownValue;
			case DescriptionTypeEnum.UV:
				return 3556617U;
			case DescriptionTypeEnum.Position:
				return 747804969U;
			case DescriptionTypeEnum.Normal:
				return 3255221479U;
			case DescriptionTypeEnum.Tangent:
				return 2752995909U;
			case DescriptionTypeEnum.Weight:
				return 1230441723U;
			case DescriptionTypeEnum.Group:
				return 1943391143U;
			case DescriptionTypeEnum.UVPadding1:
				return 949550084U;
			default:
				return 0U;
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002D98 File Offset: 0x00000F98
		public string ToP2Type()
		{
			switch (this.EnumValue)
			{
			case DescriptionTypeEnum.Unknown:
				return (string)this.unknownValue;
			case DescriptionTypeEnum.UV:
				return "tex0";
			case DescriptionTypeEnum.Position:
				return "position";
			case DescriptionTypeEnum.Normal:
				return "normal";
			case DescriptionTypeEnum.Tangent:
				return "tangent";
			case DescriptionTypeEnum.Weight:
				return "weights";
			case DescriptionTypeEnum.Group:
				return "indices";
			case DescriptionTypeEnum.Colour0:
				return "colour0";
			case DescriptionTypeEnum.Padding1:
				return "pad";
			case DescriptionTypeEnum.UV1:
				return "tex1";
			}
			return "";
		}

		// Token: 0x04000047 RID: 71
		protected object unknownValue;
	}
}
