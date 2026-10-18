using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    [KnownGame(PrototypeGame.P1)]
    [KnownType(0x00022005U)]
    public class U00022005 : BaseNode
    {
        public uint Unknown { get; set; }
        public override void Serialize(Stream output, Endian endian) { output.WriteValueU32(Unknown, endian); }
        public override void Deserialize(Stream input, Endian endian) { Unknown = input.ReadValueU32(endian); }
    }
}
