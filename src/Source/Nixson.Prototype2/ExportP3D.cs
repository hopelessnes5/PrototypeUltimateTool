using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;
using Nixson.Common;
using Nixson.Prototype2.Exporting;

namespace Nixson.Prototype2
{
    public static class ExportP3D
    {
        public static void SerializeP3D(bool isBigEndian, List<BaseNode> nodes, string path)
        {
            Endian endian = isBigEndian ? Endian.Big : Endian.Little;
            Pure3DFile pure3DFile = new Pure3DFile();
            pure3DFile.Endian = endian;
            pure3DFile.Nodes = nodes;
            FileStream fileStream = File.Create(path);
            pure3DFile.Serialize(fileStream, endian);
            fileStream.Close();
        }
        public static P2Primitive CreatePrimitiveIndices(bool isBigEndian, string name, List<Vector3Int> faces)
        {
            return Mesh.CreatePrimitiveIndices(isBigEndian ? Endian.Big : Endian.Little, name, faces);
        }
        public static P2Primitive CreatePrimitiveSkin(bool isBigEndian, string name, List<Vector3> position, List<Vector3> normal, List<float> weights, List<byte> groups)
        {
            return Mesh.CreatePrimitiveSkin(isBigEndian ? Endian.Big : Endian.Little, name, position, normal, weights, groups);
        }
        public static P2Primitive CreatePrimitiveVertices(bool isBigEndian, string name, List<UVCoordinate> uvs)
        {
            return Mesh.CreatePrimitiveVertices(isBigEndian ? Endian.Big : Endian.Little, name, uvs);
        }
    }
}
