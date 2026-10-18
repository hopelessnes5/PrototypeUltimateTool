using System.IO;
using System.ComponentModel;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    [KnownGame(PrototypeGame.P1)]
    [KnownType(0x00014000U)]
    public class Locator : BaseNode
    {
        public string Name { get; set; }
        public uint Version { get; set; }
        public Vector3 Position { get; set; }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? base.ToString() : base.ToString() + " (" + Name.TrimEnd('\0', ' ') + ")";
        public override void Serialize(Stream output, Endian endian)
        {
            output.WriteStringAlignedU8(Name ?? string.Empty);
            output.WriteValueU32(Version, endian);
            (Position ?? new Vector3()).Serialize(output, endian);
        }
        public override void Deserialize(Stream input, Endian endian)
        {
            Name = input.ReadStringAlignedU8();
            Version = input.ReadValueU32(endian);
            Position = new Vector3(input, endian);
        }
    }
}
