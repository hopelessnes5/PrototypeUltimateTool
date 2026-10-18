using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x02000080 RID: 128
	[KnownGame(PrototypeGame.P2)]
	[KnownType(151553U)]
	public class P2PolySkin : BaseNode
	{
		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x0600052B RID: 1323 RVA: 0x0000BC4C File Offset: 0x00009E4C
		// (set) Token: 0x0600052C RID: 1324 RVA: 0x0000BC54 File Offset: 0x00009E54
		public uint Unknown1 { get; set; }

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x0600052D RID: 1325 RVA: 0x0000BC5D File Offset: 0x00009E5D
		// (set) Token: 0x0600052E RID: 1326 RVA: 0x0000BC65 File Offset: 0x00009E65
		public uint Unknown2 { get; set; }

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x0600052F RID: 1327 RVA: 0x0000BC6E File Offset: 0x00009E6E
		// (set) Token: 0x06000530 RID: 1328 RVA: 0x0000BC76 File Offset: 0x00009E76
		public string Name { get; set; }

		// Token: 0x06000531 RID: 1329 RVA: 0x0000BC7F File Offset: 0x00009E7F
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return "PolySkin (" + this.Name.Trim(new char[1]) + ")";
			}
			return "PolySkin";
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x0000BCB4 File Offset: 0x00009EB4
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteStringAlignedU8(this.Name);
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0000BCDC File Offset: 0x00009EDC
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
		}
	}
}
