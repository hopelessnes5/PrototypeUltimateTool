using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001B RID: 27
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1183745U)]
	public class AnimationBone : BaseNode
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000CE RID: 206 RVA: 0x000044D5 File Offset: 0x000026D5
		// (set) Token: 0x060000CF RID: 207 RVA: 0x000044DD File Offset: 0x000026DD
		public uint Pad { get; set; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x000044E6 File Offset: 0x000026E6
		// (set) Token: 0x060000D1 RID: 209 RVA: 0x000044EE File Offset: 0x000026EE
		public string Name { get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x000044F7 File Offset: 0x000026F7
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x000044FF File Offset: 0x000026FF
		public uint BoneID { get; set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x00004508 File Offset: 0x00002708
		// (set) Token: 0x060000D5 RID: 213 RVA: 0x00004510 File Offset: 0x00002710
		public uint NumberOfChannels { get; set; }

		// Token: 0x060000D6 RID: 214 RVA: 0x00004519 File Offset: 0x00002719
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00004558 File Offset: 0x00002758
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Pad, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.BoneID, endian);
			// NumberOfChannels is an on-disk field. Preserve its original value for a lossless
			// round-trip; counting typed children can silently rewrite valid files when some
			// channel variants are represented by unknown/opaque nodes.
			output.WriteValueU32(this.NumberOfChannels, endian);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000459C File Offset: 0x0000279C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Pad = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.BoneID = input.ReadValueU32(endian);
			this.NumberOfChannels = input.ReadValueU32(endian);
		}
	}
}
