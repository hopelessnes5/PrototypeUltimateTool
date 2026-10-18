using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    [KnownGame(PrototypeGame.P1)]
    [KnownType(0x0012110EU)]
    public class VertexMorphChannel : AnimationChannel
    {
        public VertexMorphKey[] Keys { get; set; } = new VertexMorphKey[0];
        [Browsable(false)] public byte[] ExtraData { get; set; } = new byte[0];

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);
            output.WriteValueU32((uint)(Keys == null ? 0 : Keys.Length), endian);
            if (Keys != null) foreach (var key in Keys) output.WriteValueU16(key.Frame, endian);
            if (Keys != null) foreach (var key in Keys) output.WriteValueU32(key.Value, endian);
            if (ExtraData != null && ExtraData.Length > 0) output.Write(ExtraData, 0, ExtraData.Length);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);
            Mapping = 0;
            BaseValues = new Vector3();
            NumberOfFrames = input.ReadValueU32(endian);
            if (NumberOfFrames > int.MaxValue) throw new FormatException("Vertex morph channel has too many keys.");
            long end = (long)StartPosition + HeaderSize;
            long remaining = end - input.Position;
            long required = (long)NumberOfFrames * 6L;
            if (remaining < required) throw new FormatException("Vertex morph key table exceeds its chunk size.");

            int count = (int)NumberOfFrames;
            FrameOrder = new ushort[count];
            for (int i = 0; i < count; i++) FrameOrder[i] = input.ReadValueU16(endian);
            Keys = new VertexMorphKey[count];
            Frames = new System.Collections.Generic.Dictionary<ushort, Vector4>();
            for (int i = 0; i < count; i++)
            {
                uint value = input.ReadValueU32(endian);
                ushort key = FrameOrder[i];
                Keys[i] = new VertexMorphKey(key, value);
                Frames[key] = new Vector4 { X = value };
            }
            remaining = end - input.Position;
            ExtraData = remaining > 0 ? input.ReadBytes((int)remaining) : new byte[0];
        }

        public override void ReadFrames(byte[] animationData) { /* VRTX stores keys inline. */ }
        protected override void ReadChannel(Stream input, Endian endian) { }

        public override Vector4 CalculateValue(float frame)
        {
            if (Keys == null || Keys.Length == 0) return new Vector4();
            var ordered = Keys.OrderBy(k => k.Frame).ToArray();
            if (ordered.Length == 1 || frame <= ordered[0].Frame) return new Vector4 { X = ordered[0].Value };
            for (int i = 0; i + 1 < ordered.Length; i++)
            {
                if (frame > ordered[i + 1].Frame) continue;
                int span = ordered[i + 1].Frame - ordered[i].Frame;
                if (span <= 0) return new Vector4 { X = ordered[i + 1].Value };
                float t = (frame - ordered[i].Frame) / span;
                return new Vector4 { X = ordered[i].Value + (ordered[i + 1].Value - ordered[i].Value) * t };
            }
            return new Vector4 { X = ordered[ordered.Length - 1].Value };
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class VertexMorphKey
    {
        public VertexMorphKey(ushort frame, uint value) { Frame = frame; Value = value; }
        public ushort Frame { get; private set; }
        public uint Value { get; private set; }
        public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: {1}", Frame, Value);
    }
}
