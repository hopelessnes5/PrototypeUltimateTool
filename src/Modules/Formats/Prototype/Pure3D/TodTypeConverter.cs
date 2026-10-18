using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gibbed.IO;
using Nixson.Prototype.Tod;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000060 RID: 96
	public class TodTypeConverter : ExpandableObjectConverter
	{
		// Token: 0x060003F8 RID: 1016 RVA: 0x00009EA8 File Offset: 0x000080A8
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			MetaObjectData metaObjectData = (MetaObjectData)value;
			List<PropertyDescriptor> list = new List<PropertyDescriptor>();
			foreach (object obj in TypeDescriptor.GetProperties(metaObjectData.metaObject))
			{
				PropertyDescriptor propertyDescriptor = (PropertyDescriptor)obj;
				if (((BrowsableAttribute)propertyDescriptor.Attributes[typeof(BrowsableAttribute)]).Browsable && (!(propertyDescriptor.Name == "Attributes") || metaObjectData.metaObject.Attributes.Count > 0))
				{
					list.Add(new TodTypeConverter.Descriptor(metaObjectData.metaObject.GetType(), propertyDescriptor.PropertyType, propertyDescriptor.Name, false));
				}
			}
			foreach (object obj2 in base.GetProperties(context, value, attributes))
			{
				PropertyDescriptor item = (PropertyDescriptor)obj2;
				list.Add(item);
			}
			for (int i = 0; i < list.Count; i++)
			{
				PropertyDescriptor propertyDescriptor2 = list[i];
				if (propertyDescriptor2.Category == "Misc" && propertyDescriptor2.ComponentType != null)
				{
					// Recreate the descriptor with a public CategoryAttribute rather than relying
					// on the private MemberDescriptor.category field from older .NET Framework.
					list[i] = TypeDescriptor.CreateProperty(
						propertyDescriptor2.ComponentType, propertyDescriptor2,
						new CategoryAttribute("\u200B" + metaObjectData.metaObject.GetType().Name));
				}
			}
			return new PropertyDescriptorCollection(list.ToArray());
		}

		// Token: 0x02000096 RID: 150
		private class Descriptor : TypeConverter.SimplePropertyDescriptor
		{
			// Token: 0x170001D4 RID: 468
			// (get) Token: 0x060005BB RID: 1467 RVA: 0x0000CD39 File Offset: 0x0000AF39
			public override string DisplayName
			{
				get
				{
					return this._DisplayName;
				}
			}

			// Token: 0x170001D5 RID: 469
			// (get) Token: 0x060005BC RID: 1468 RVA: 0x0000CD41 File Offset: 0x0000AF41
			public override bool IsReadOnly
			{
				get
				{
					return this._ReadOnly;
				}
			}

			// Token: 0x060005BD RID: 1469 RVA: 0x0000CD49 File Offset: 0x0000AF49
			public Descriptor(Type componentType, Type elementType, string name, bool readOnly = false) : base(componentType, name, elementType, null)
			{
				this._Name = name;
				this._DisplayName = name.SeparateCamelCase();
				this._ReadOnly = readOnly;
			}

			// Token: 0x060005BE RID: 1470 RVA: 0x0000CD70 File Offset: 0x0000AF70
			public override object GetValue(object instance)
			{
				MetaObject metaObject = ((MetaObjectData)instance).metaObject;
				return metaObject.GetType().GetProperty(this._Name).GetValue(metaObject, null);
			}

			// Token: 0x060005BF RID: 1471 RVA: 0x0000CDA1 File Offset: 0x0000AFA1
			public override void SetValue(object instance, object value)
			{
				MetaObject metaObject = ((MetaObjectData)instance).metaObject;
				var property = metaObject.GetType().GetProperty(this._Name);
				if (property == null || !property.CanWrite)
				{
					throw new InvalidOperationException("The TimeOfDay property '" + this._Name + "' cannot be edited.");
				}
				property.SetValue(metaObject, value, null);
			}

			// Token: 0x060005C0 RID: 1472 RVA: 0x00003219 File Offset: 0x00001419
			public override bool ShouldSerializeValue(object component)
			{
				return true;
			}

			// Token: 0x04000221 RID: 545
			private readonly string _DisplayName;

			// Token: 0x04000222 RID: 546
			private readonly string _Name;

			// Token: 0x04000223 RID: 547
			private readonly bool _ReadOnly;
		}
	}
}
