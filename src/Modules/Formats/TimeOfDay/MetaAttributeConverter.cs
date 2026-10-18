using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000A RID: 10
	public class MetaAttributeConverter : TypeConverter
	{
		// Token: 0x0600002D RID: 45 RVA: 0x000022E4 File Offset: 0x000004E4
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			List<PropertyDescriptor> list = new List<PropertyDescriptor>();
			MetaAttribute metaAttribute = (MetaAttribute)value;
			foreach (object obj in TypeDescriptor.GetProperties(metaAttribute))
			{
				PropertyDescriptor propertyDescriptor = (PropertyDescriptor)obj;
				if (((BrowsableAttribute)propertyDescriptor.Attributes[typeof(BrowsableAttribute)]).Browsable)
				{
					// Category is immutable through the public API. Create a decorated descriptor
					// instead of mutating MemberDescriptor's private implementation field.
					if (propertyDescriptor.Category != "\u200BAttribute" && propertyDescriptor.ComponentType != null)
					{
						propertyDescriptor = TypeDescriptor.CreateProperty(
							propertyDescriptor.ComponentType, propertyDescriptor, new CategoryAttribute(metaAttribute.TypeName));
					}
					list.Add(propertyDescriptor);
				}
			}
			return new PropertyDescriptorCollection(list.ToArray());
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000023C4 File Offset: 0x000005C4
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
