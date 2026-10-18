using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    [KnownGame(PrototypeGame.P1)]
    [KnownType(0x00121108U)]
    public class VisibilityChannel : AnimationChannel
    {
        public ushort InitialVisibility { get; set; }

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);
            output.WriteValueU16(InitialVisibility, endian);
            ushort[] keys = GetRawFrameKeys();
            output.WriteValueU32((uint)keys.Length, endian);
            foreach (ushort key in keys) output.WriteValueU16(key, endian);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);
            InitialVisibility = input.ReadValueU16(endian);
            NumberOfFrames = input.ReadValueU32(endian);
            ReadFrameKeys(input, endian);
            foreach (var key in Frames.Keys.ToArray()) Frames[key] = new Vector4 { X = InitialVisibility != 0 ? 1.0f : 0.0f };
        }

        public override void ReadFrames(byte[] animationData) { /* PVIS stores toggle keys inline. */ }
        protected override void ReadChannel(Stream input, Endian endian) { }

        public override Vector4 CalculateValue(float frame) => new Vector4 { X = IsVisible(frame) ? 1.0f : 0.0f };

        public bool IsVisible(float frame)
        {
            bool visible = InitialVisibility != 0;
            foreach (ushort key in OrderedFrameKeys)
            {
                if (frame < key) break;
                visible = !visible;
            }
            return visible;
        }
    }
}
