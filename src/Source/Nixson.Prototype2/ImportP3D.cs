using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;
using Nixson.Common;
using Nixson.Prototype2.Importing;

namespace Nixson.Prototype2
{
    public static class ImportP3D
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
        public static List<MeshImportData> GetDrawables(Pure3DFile p3d)
        {
            return Mesh.GetDrawables(p3d);
        }
        public static MeshImportData GetDrawable(Pure3DFile p3d, P2PolySkin polySkin)
        {
            return Mesh.GetDrawable(p3d, polySkin);
        }
        public static List<Vector3> GetVertices(P2Primitive primitive)
        {
            return Mesh.GetBuffer<Vector3>(DescriptionTypeEnum.Position, primitive, false);
        }
        public static List<Face> GetFaces(P2Primitive primitive)
        {
            return Mesh.GetFaces(primitive);
        }
        public static List<UVCoordinate> GetUVs(P2Primitive primitive)
        {
            return Mesh.GetBuffer<UVCoordinate>(DescriptionTypeEnum.UV, primitive, true);
        }
        public static List<Vector3> GetNormals(P2Primitive primitive)
        {
            return Mesh.GetBuffer<Vector3>(DescriptionTypeEnum.Normal, primitive, false);
        }
        public static List<Vector4> GetWeights(P2Primitive primitive)
        {
            return Mesh.GetBuffer<Vector4>(DescriptionTypeEnum.Weight, primitive, false);
        }
        public static List<byte[]> GetGroups(P2Primitive primitive)
        {
            return Mesh.GetBuffer<byte[]>(DescriptionTypeEnum.Group, primitive, false);
        }
    }
}
