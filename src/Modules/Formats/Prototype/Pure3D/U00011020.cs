using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    [KnownGame(PrototypeGame.P1)]
    [KnownType(0x00011020U)]
    public class U00011020 : BaseNode
    {
        public ushort Unknown1 { get; set; }
        public ushort Unknown2 { get; set; }
        public override void Serialize(Stream output, Endian endian)
        { output.WriteValueU16(Unknown1, endian); output.WriteValueU16(Unknown2, endian); }
        public override void Deserialize(Stream input, Endian endian)
        { Unknown1 = input.ReadValueU16(endian); Unknown2 = input.ReadValueU16(endian); }
    }
}
