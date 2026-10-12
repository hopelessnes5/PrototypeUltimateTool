using System;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;

namespace Nixson.Prototype2
{
    public static class Utils
    {
        public static int FindIndexOfDescription(DescriptionTypeEnum descriptionType, P2BufferDescriptor vertexDescription)
        {
            int num = 0;
            while ((long)num < (long)((ulong)vertexDescription.AmountOfDescriptions))
            {
                if (vertexDescription.Descriptions[num].BufferType.EnumValue == descriptionType)
                {
                    return num;
                }
                num++;
            }
            return -1;
        }
    }
}
