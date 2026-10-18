using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000031 RID: 49
	[KnownType(1184517U)]
	public class ExpressionAnimation : BaseNode
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x00006B5D File Offset: 0x00004D5D
		// (set) Token: 0x060001A6 RID: 422 RVA: 0x00006B65 File Offset: 0x00004D65
		public uint Unknown1 { get; set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x00006B6E File Offset: 0x00004D6E
		// (set) Token: 0x060001A8 RID: 424 RVA: 0x00006B76 File Offset: 0x00004D76
		public uint AmountOfFrames { get; set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00006B7F File Offset: 0x00004D7F
		// (set) Token: 0x060001AA RID: 426 RVA: 0x00006B87 File Offset: 0x00004D87
		public byte[] Unknown2 { get; set; }

		// Token: 0x060001AB RID: 427 RVA: 0x00006B90 File Offset: 0x00004D90
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.AmountOfFrames, endian);
			output.WriteBytes(this.Unknown2);
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00006BB8 File Offset: 0x00004DB8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.AmountOfFrames = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadBytes((int)(this.AmountOfFrames * 8U));
		}
	}
}
