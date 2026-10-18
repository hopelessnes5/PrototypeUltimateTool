using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>
    /// A Pure3D skeleton joint (chunk 0x00023001). The payload is three basis vectors,
    /// their translation-column values, and a joint position. Some variants append extra bytes.
    /// This matches the structure used by Elan-S/EditP3D; treating this chunk as a 4x4 matrix
    /// followed by a fixed-size tail misaligns the P3D child stream.
    /// </summary>
    [KnownGame(PrototypeGame.Any)]
    [KnownType(0x00023001U)]
    public class SkeletonJoint : BaseNode
    {
        public string Name { get; set; }
        public int ParentIndex { get; set; }
        public Vector3 AxisX { get; set; }
        public Vector3 AxisY { get; set; }
        public Vector3 AxisZ { get; set; }
        public Vector3 Position { get; set; }
        public float Matrix03 { get; set; }
        public float Matrix13 { get; set; }
        public float Matrix23 { get; set; }
        public byte[] ExtraData { get; set; } = new byte[0];

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(this.Name) ? base.ToString() : base.ToString() + " (" + this.Name.TrimEnd('\0', ' ') + ")";
        }

        public override void Serialize(Stream output, Endian endian)
        {
            output.WriteStringAlignedU8(this.Name);
            output.WriteValueU32(unchecked((uint)this.ParentIndex), endian);
            EnsureVector(this.AxisX).Serialize(output, endian);
            output.WriteValueF32(this.Matrix03, endian);
            EnsureVector(this.AxisY).Serialize(output, endian);
            output.WriteValueF32(this.Matrix13, endian);
            EnsureVector(this.AxisZ).Serialize(output, endian);
            output.WriteValueF32(this.Matrix23, endian);
            EnsureVector(this.Position).Serialize(output, endian);
            if (this.ExtraData != null && this.ExtraData.Length > 0)
                output.WriteBytes(this.ExtraData);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            long end = (long)this.StartPosition + this.HeaderSize;
            this.Name = input.ReadStringAlignedU8();
            this.ParentIndex = unchecked((int)input.ReadValueU32(endian));
            this.AxisX = new Vector3(input, endian);
            this.Matrix03 = input.ReadValueF32(endian);
            this.AxisY = new Vector3(input, endian);
            this.Matrix13 = input.ReadValueF32(endian);
            this.AxisZ = new Vector3(input, endian);
            this.Matrix23 = input.ReadValueF32(endian);
            this.Position = new Vector3(input, endian);
            long remaining = end - input.Position;
            if (remaining < 0 || remaining > int.MaxValue)
                throw new FormatException("Invalid SkeletonJoint chunk size.");
            this.ExtraData = remaining == 0 ? new byte[0] : input.ReadBytes((int)remaining);
        }

        private static Vector3 EnsureVector(Vector3 value)
        {
            return value ?? new Vector3();
        }
    }
}
