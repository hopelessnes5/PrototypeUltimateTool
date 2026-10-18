using System;
using System.ComponentModel;
using System.Globalization;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x0200000D RID: 13
	public class MatrixTypeConverter : TypeConverter
	{
		// Token: 0x0600004A RID: 74 RVA: 0x000030F2 File Offset: 0x000012F2
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003110 File Offset: 0x00001310
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				JointMatrix jointMatrix = value as JointMatrix;
				if (jointMatrix != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "Joint Matrix (Height = {0:0.000000}, Width = {1:0.000000})", jointMatrix.Height, jointMatrix.Width);
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000316C File Offset: 0x0000136C
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptor[] properties = null;
			if (value is JointMatrix)
			{
				PropertyDescriptor[] array = new MatrixTypeConverter.Descriptor[]
				{
					new MatrixTypeConverter.Descriptor(typeof(JointMatrix), typeof(float[]), "Axis X"),
					new MatrixTypeConverter.Descriptor(typeof(JointMatrix), typeof(float[]), "Axis Y"),
					new MatrixTypeConverter.Descriptor(typeof(JointMatrix), typeof(float[]), "Axis Z"),
					new MatrixTypeConverter.Descriptor(typeof(JointMatrix), typeof(float[]), "Position")
				};
				properties = array;
			}
			return new PropertyDescriptorCollection(properties);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003219 File Offset: 0x00001419
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		// Token: 0x0200008C RID: 140
		private class Descriptor : TypeConverter.SimplePropertyDescriptor
		{
			// Token: 0x060005A1 RID: 1441 RVA: 0x0000C77F File Offset: 0x0000A97F
			public Descriptor(Type componentType, Type elementType, string name) : base(componentType, name, elementType, null)
			{
				this._Name = name;
			}

			// Token: 0x060005A2 RID: 1442 RVA: 0x0000C792 File Offset: 0x0000A992
			public override object GetValue(object instance)
			{
				if (!(instance is JointMatrix))
				{
					return null;
				}
				if (this._Name == "Axis X")
				{
					return "";
				}
				return null;
			}

			// Token: 0x170001D0 RID: 464
			// (get) Token: 0x060005A3 RID: 1443 RVA: 0x00003219 File Offset: 0x00001419
			public override bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			// Token: 0x060005A4 RID: 1444 RVA: 0x0000A320 File Offset: 0x00008520
			public override void SetValue(object instance, object value)
			{
			}

			// Token: 0x0400020E RID: 526
			private readonly string _Name;
		}
	}
}
