using System;
using System.Collections.Generic;
using Gibbed.Prototype.FileFormats;
using Nixson.Common;

namespace Nixson.Prototype1
{
    public class MeshData
    {
        public List<Vector3> position;
        public List<Vector3> normal;
        public List<Vector3> weight;
        public List<Vector4> tangent;
        public List<byte[]> group;
        public List<UVCoordinate> uv;
    }
}
