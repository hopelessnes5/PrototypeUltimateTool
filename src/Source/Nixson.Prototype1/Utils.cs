using System;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;

namespace Nixson.Prototype1
{
    internal class Utils
    {
        public static int FindIndexOfDescription(DescriptionTypeEnum descriptionType, VertexDescriptionList vertexDescription)
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
        public static bool IsRawBuffer(PrimitiveGroup primitiveGroup)
        {
            return primitiveGroup.GetChildNodes<RawPositionBuffer>().Count > 0;
        }
    }
}
