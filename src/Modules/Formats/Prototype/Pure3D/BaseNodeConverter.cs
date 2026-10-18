using System;
using System.ComponentModel;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000049 RID: 73
	internal class BaseNodeConverter : ExpandableObjectConverter
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x00008340 File Offset: 0x00006540
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection properties = base.GetProperties(context, value, attributes);
			if (properties == null || properties.Count == 0)
			{
				return properties;
			}

			PropertyDescriptor[] descriptors = new PropertyDescriptor[properties.Count];
			for (int i = 0; i < properties.Count; i++)
			{
				PropertyDescriptor propertyDescriptor = properties[i];
				if (propertyDescriptor != null &&
					propertyDescriptor.ComponentType != null &&
					string.Equals(propertyDescriptor.Category, "Misc", StringComparison.Ordinal))
				{
					descriptors[i] = TypeDescriptor.CreateProperty(
						propertyDescriptor.ComponentType,
						propertyDescriptor,
						new CategoryAttribute("Node"));
				}
				else
				{
					descriptors[i] = propertyDescriptor;
				}
			}

			return new PropertyDescriptorCollection(descriptors, true);
		}
	}
}
