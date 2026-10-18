using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>Prototype 1 legacy normal stream (chunk 0x00010006).</summary>
    [KnownGame(PrototypeGame.P1)]
	[KnownType(65542U)]
    public sealed class RawNormalBuffer : BaseNode
    {
        public uint BufferSize { get; set; }
        public Vector3[] Normals { get; set; }

        public override void Serialize(Stream output, Endian endian)
        {
            uint count = Normals == null ? 0U : (uint)Normals.Length;
            output.WriteValueU32(count, endian);
            if (Normals != null)
            {
                foreach (Vector3 normal in Normals)
                    normal.Serialize(output, endian);
            }
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            BufferSize = input.ReadValueU32(endian);
            if (BufferSize > 2000000U || BufferSize > (uint)(int.MaxValue / 12) ||
                (input.CanSeek && (long)BufferSize * 12L > input.Length - input.Position))
                throw new FormatException("Prototype 1 raw normal count exceeds the remaining chunk payload.");

            Normals = new Vector3[(int)BufferSize];
            for (int i = 0; i < Normals.Length; i++)
                Normals[i] = new Vector3(input, endian);
        }
    }
}
