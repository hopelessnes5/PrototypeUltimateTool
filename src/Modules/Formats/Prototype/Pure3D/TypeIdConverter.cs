using System;
using System.ComponentModel;
using System.Globalization;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004F RID: 79
	internal class TypeIdConverter : TypeConverter
	{
		// Token: 0x0600033A RID: 826 RVA: 0x00008F0F File Offset: 0x0000710F
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		// Token: 0x0600033B RID: 827 RVA: 0x000030F2 File Offset: 0x000012F2
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00008F2D File Offset: 0x0000712D
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is uint)
			{
				return string.Format("0x{0:X8}", value);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00008F64 File Offset: 0x00007164
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string text = value as string;
			if (text != null)
			{
				if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(2);
				}
				return uint.Parse(text, NumberStyles.HexNumber, culture);
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
