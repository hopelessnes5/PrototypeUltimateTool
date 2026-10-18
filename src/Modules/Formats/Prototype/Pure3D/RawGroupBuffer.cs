using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>Prototype 1 legacy group stream (chunk 0x0001000B). Each entry has four byte indices.</summary>
    [KnownGame(PrototypeGame.P1)]
	[KnownType(65547U)]
    public sealed class RawGroupBuffer : BaseNode
    {
        public uint BufferSize { get; set; }
        public byte[][] Groups { get; set; }

        public override void Serialize(Stream output, Endian endian)
        {
            uint count = Groups == null ? 0U : (uint)Groups.Length;
            output.WriteValueU32(count, endian);
            for (int i = 0; i < count; i++)
            {
                byte[] item = Groups[i];
                output.WriteByte(item != null && item.Length > 0 ? item[0] : (byte)0);
                output.WriteByte(item != null && item.Length > 1 ? item[1] : (byte)0);
                output.WriteByte(item != null && item.Length > 2 ? item[2] : (byte)0);
                output.WriteByte(item != null && item.Length > 3 ? item[3] : (byte)0);
            }
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            BufferSize = input.ReadValueU32(endian);
            if (BufferSize > 1000000U || BufferSize > (uint)(int.MaxValue / 4))
                throw new InvalidDataException("Prototype 1 raw group buffer is too large.");
            if (input.CanSeek && (long)BufferSize * 4L > input.Length - input.Position)
                throw new FormatException("Prototype 1 raw group count exceeds the remaining chunk payload.");

            byte[] bytes = input.ReadBytes((int)BufferSize * 4);
            if (bytes == null || bytes.Length != (int)BufferSize * 4)
                throw new EndOfStreamException("Unexpected end of Prototype 1 raw group buffer.");

            Groups = new byte[(int)BufferSize][];
            for (int i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new byte[4];
                Buffer.BlockCopy(bytes, i * 4, Groups[i], 0, 4);
            }
        }
    }
}
