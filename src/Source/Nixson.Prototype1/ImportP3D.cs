using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Nixson.Common;
using Nixson.Prototype1.Importing;

namespace Nixson.Prototype1
{
    public class ImportP3D
    {
        public static Pure3DFile ReadP3DFile(string path, bool bigEndian = false)
        {
            Pure3DFile pure3DFile = new Pure3DFile();
            using (FileStream fileStream = File.OpenRead(path))
            {
                pure3DFile.Deserialize(fileStream, bigEndian ? Endian.Big : Endian.Little);
            }
            return pure3DFile;
        }
        public static List<Vector3> GetPositionVertices(PrimitiveGroup primitiveGroup)
        {
            List<Vector3> list;
            if (Utils.IsRawBuffer(primitiveGroup))
            {
                list = Mesh.GetRawPositionBuffer(primitiveGroup);
            }
            else
            {
                list = Mesh.GetBuffer<Vector3>(DescriptionTypeEnum.Position, primitiveGroup, false);
            }
            if (list.Count <= 0)
            {
                throw new Exception("Vertex buffer is empty.");
            }
            return list;
        }
        public static List<Face> GetFaces(PrimitiveGroup primitiveGroup)
        {
            List<Face> list;
            if (Utils.IsRawBuffer(primitiveGroup))
            {
                list = Mesh.GetRawFaceBuffer(primitiveGroup);
            }
            else
            {
                list = Mesh.GetFaces(primitiveGroup);
            }
            if (list.Count <= 0)
            {
                throw new Exception("Face buffer is empty.");
            }
            return list;
        }
        public static List<UVCoordinate> GetUVs(PrimitiveGroup primitiveGroup)
        {
            List<UVCoordinate> list;
            if (Utils.IsRawBuffer(primitiveGroup))
            {
                list = Mesh.GetRawUVBuffer(primitiveGroup);
            }
            else
            {
                list = Mesh.GetBuffer<UVCoordinate>(DescriptionTypeEnum.UV, primitiveGroup, true);
            }
            if (list.Count <= 0)
            {
                throw new Exception("UV buffer is empty.");
            }
            return list;
        }
        public static List<Vector3> GetNormals(PrimitiveGroup primitiveGroup)
        {
            return Mesh.GetBuffer<Vector3>(DescriptionTypeEnum.Normal, primitiveGroup, false);
        }
        public static List<Vector3> GetWeights(PrimitiveGroup primitiveGroup)
        {
            return Mesh.GetBuffer<Vector3>(DescriptionTypeEnum.Weight, primitiveGroup, false);
        }
        public static List<byte[]> GetGroups(PrimitiveGroup primitiveGroup)
        {
            return Mesh.GetBuffer<byte[]>(DescriptionTypeEnum.Group, primitiveGroup, false);
        }
        public static List<uint> GetMatrixIndices(PrimitiveGroup primitiveGroup)
        {
            return Mesh.GetMatrixIndices(primitiveGroup);
        }
        public static byte[] UncompressAnimationData(AnimationData animationData)
        {
            return Anim.UncompressAnimationData(animationData);
        }
        public static Dictionary<ushort, FrameData> GetFrameData(AnimationBone bone, byte[] animationData)
        {
            return Anim.GetFrameData(bone, animationData);
        }
    }
}
