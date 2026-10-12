using System;

namespace Nixson.Prototype2
{
    public enum MeshExportFlags
    {
        Position = 1,
        Normal,
        Tangent = 4,
        Weight = 8,
        Group = 16,
        UV = 32,
        Padding = 64,
        Faces = 128,
        Colour = 256
    }
}
