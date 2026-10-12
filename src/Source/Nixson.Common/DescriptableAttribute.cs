using System;

namespace Nixson.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptableAttribute : Attribute
    {
        public DescriptableAttribute()
        {
        }
        public DescriptableAttribute(bool readOnly)
        {
            this.readOnly = readOnly;
        }
        public readonly bool readOnly;
    }
}
