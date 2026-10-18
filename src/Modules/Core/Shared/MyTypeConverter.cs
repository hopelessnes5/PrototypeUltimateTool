using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Gibbed.IO;

namespace Nixson.Common
{
    public class MyTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value == null)
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            return value.ToString();
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
            {
                DescriptableAttribute descriptableAttribute = (DescriptableAttribute)propertyInfo.GetCustomAttribute(typeof(DescriptableAttribute));
                if (descriptableAttribute != null)
                {
                    list.Add(new MyTypeConverter.Descriptor(value.GetType(), propertyInfo.PropertyType, propertyInfo.Name, descriptableAttribute.readOnly));
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        private class Descriptor : TypeConverter.SimplePropertyDescriptor
        {
            public override string DisplayName
            {
                get
                {
                    return this._DisplayName;
                }
            }
            public override bool IsReadOnly
            {
                get
                {
                    return this._ReadOnly;
                }
            }
            public Descriptor(Type componentType, Type elementType, string name, bool readOnly = false) : base(componentType, name, elementType, null)
            {
                this._Name = name;
                this._DisplayName = name.SeparateCamelCase();
                this._ReadOnly = readOnly;
            }
            public override object GetValue(object instance)
            {
                return instance.GetType().GetProperty(this._Name).GetValue(instance, null);
            }
            public override void SetValue(object instance, object value)
            {
                instance.GetType().GetProperty(this._Name).SetValue(instance, value);
            }
            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
            private readonly string _DisplayName;
            private readonly string _Name;
            private readonly bool _ReadOnly;
        }
    }
}
