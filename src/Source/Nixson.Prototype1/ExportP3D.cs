using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Nixson.Common;
using Nixson.Prototype1.Exporting;

namespace Nixson.Prototype1
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
        public static PrimitiveGroup CreatePrimitiveGroup(bool isGeo, bool isBigEndian, List<Vector3> position, List<Vector3Int> faces, List<Vector3> normal, List<float> weights, List<byte> groups, List<UVCoordinate> uvs, List<uint> skeletonGroups)
        {
            Endian endian = isBigEndian ? Endian.Big : Endian.Little;
            if (isGeo)
            {
                return Mesh.CreatePrimitiveGroupGeometry(endian, position, faces, normal, uvs);
            }
            return Mesh.CreatePrimitiveGroup(endian, position, faces, normal, weights, groups, uvs, skeletonGroups);
        }
    }
}
