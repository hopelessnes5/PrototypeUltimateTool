using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>Prototype 1 legacy weight stream (chunk 0x0001000C).</summary>
    [KnownGame(PrototypeGame.P1)]
	[KnownType(65548U)]
    public sealed class RawWeightBuffer : BaseNode
    {
        public uint BufferSize { get; set; }
        public Vector3[] Weights { get; set; }

        public override void Serialize(Stream output, Endian endian)
        {
            uint count = Weights == null ? 0U : (uint)Weights.Length;
            output.WriteValueU32(count, endian);
            if (Weights != null)
            {
                foreach (Vector3 weight in Weights)
                    weight.Serialize(output, endian);
            }
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            BufferSize = input.ReadValueU32(endian);
            if (BufferSize > 2000000U || BufferSize > (uint)(int.MaxValue / 12) ||
                (input.CanSeek && (long)BufferSize * 12L > input.Length - input.Position))
                throw new FormatException("Prototype 1 raw weight count exceeds the remaining chunk payload.");

            Weights = new Vector3[(int)BufferSize];
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new Vector3(input, endian);
        }
    }
}
