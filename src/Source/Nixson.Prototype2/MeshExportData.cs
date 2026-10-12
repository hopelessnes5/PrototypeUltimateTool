using System;
using System.Collections.Generic;
using Gibbed.Prototype.FileFormats;
using Nixson.Common;

namespace Nixson.Prototype2
{
    public class MeshExportData
    {
        public int vertices;
        public List<Vector3> position;
        public List<Vector3> normal;
        public List<Vector4> tangent;
        public List<Vector4> weight;
        public List<UVCoordinate> uv;
        public List<byte[]> group;
        public List<byte[]> colour;
    }
}
