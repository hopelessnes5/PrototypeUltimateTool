using System;

namespace Nixson.Prototype1
{
    [Flags]
    public enum MeshExportFlags
    {
        Position = 1,
        Normal = 2,
        Tangent = 4,
        Weight = 8,
        Group = 16,
        UV = 32,
        Padding = 64
    }
}
