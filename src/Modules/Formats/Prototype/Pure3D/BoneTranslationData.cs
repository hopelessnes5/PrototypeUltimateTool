using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>Legacy inline bone translation track (Pure3D chunk 0x00121106).</summary>
    [KnownGame(PrototypeGame.Any)]
    [KnownType(0x00121106U)]
    public class BoneTranslationData : BaseNode
    {
        public uint Version { get; set; }
        public FourCC Param { get; set; }
        public uint NumFrames { get; set; }
        public ushort[] Frames { get; set; } = new ushort[0];
        public Vector3[] Values { get; set; } = new Vector3[0];

        public override void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32(Version, endian);
            (Param ?? (FourCC)"TRAN").Serialize(output);
            output.WriteValueU32(NumFrames, endian);
            int declaredCount = NumFrames > (uint)int.MaxValue ? int.MaxValue : (int)NumFrames;
            int count = Math.Min(declaredCount, Math.Min(Frames == null ? 0 : Frames.Length, Values == null ? 0 : Values.Length));
            for (int i = 0; i < count; i++) output.WriteValueU16(Frames[i], endian);
            for (int i = 0; i < count; i++) (Values[i] ?? new Vector3()).Serialize(output, endian);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            Version = input.ReadValueU32(endian);
            Param = new FourCC(input);
            NumFrames = input.ReadValueU32(endian);
            if (NumFrames > 1000000U) throw new FormatException("BoneTranslationData has an invalid frame count.");
            int count = (int)NumFrames;
            long required = (long)count * 14L;
            if (input.CanSeek && required > input.Length - input.Position) throw new FormatException("BoneTranslationData exceeds its chunk size.");
            Frames = new ushort[count];
            for (int i = 0; i < count; i++) Frames[i] = input.ReadValueU16(endian);
            Values = new Vector3[count];
            for (int i = 0; i < count; i++) Values[i] = new Vector3(input, endian);
        }
    }
}
