using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001F RID: 31
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184032U)]
	[KnownType(1184033U)]
	public class AnimationDataReference : BaseNode
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000104 RID: 260 RVA: 0x0000499C File Offset: 0x00002B9C
		// (set) Token: 0x06000105 RID: 261 RVA: 0x000049A4 File Offset: 0x00002BA4
		public uint Version { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000106 RID: 262 RVA: 0x000049AD File Offset: 0x00002BAD
		// (set) Token: 0x06000107 RID: 263 RVA: 0x000049B5 File Offset: 0x00002BB5
		public uint Frames { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000108 RID: 264 RVA: 0x000049BE File Offset: 0x00002BBE
		// (set) Token: 0x06000109 RID: 265 RVA: 0x000049C6 File Offset: 0x00002BC6
		public uint Offset { get; set; }

		/// <summary>Optional trailing data; the first byte selects the animation-data block.</summary>
		public byte[] ExtraData { get; set; } = new byte[0];

		// Token: 0x0600010A RID: 266 RVA: 0x000049CF File Offset: 0x00002BCF
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Frames, endian);
			output.WriteValueU32(this.Offset, endian);
            if (this.ExtraData != null && this.ExtraData.Length > 0)
                output.Write(this.ExtraData, 0, this.ExtraData.Length);
		}

		// Token: 0x0600010B RID: 267 RVA: 0x000049F5 File Offset: 0x00002BF5
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Frames = input.ReadValueU32(endian);
			this.Offset = input.ReadValueU32(endian);
            long end = (long)this.StartPosition + this.HeaderSize;
            long remaining = end - input.Position;
            if (remaining < 0 || remaining > int.MaxValue) throw new FormatException("Invalid AnimationDataReference chunk size.");
            if (remaining > int.MaxValue || (input.CanSeek && remaining > input.Length - input.Position))
                throw new FormatException("AnimationDataReference payload exceeds the available bytes.");
            this.ExtraData = remaining == 0 ? new byte[0] : input.ReadBytes((int)remaining);
            if (this.ExtraData == null || this.ExtraData.Length != remaining)
                throw new EndOfStreamException("AnimationDataReference payload is truncated.");
		}
	}
}
