using System;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000039 RID: 57
	public class Registry2 : BaseNode
	{
		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060001F8 RID: 504 RVA: 0x00007451 File Offset: 0x00005651
		// (set) Token: 0x060001F9 RID: 505 RVA: 0x00007459 File Offset: 0x00005659
		public uint Unknown1 { get; set; }

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060001FA RID: 506 RVA: 0x00007462 File Offset: 0x00005662
		// (set) Token: 0x060001FB RID: 507 RVA: 0x0000746A File Offset: 0x0000566A
		public string Unknown2 { get; set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060001FC RID: 508 RVA: 0x00007473 File Offset: 0x00005673
		// (set) Token: 0x060001FD RID: 509 RVA: 0x0000747B File Offset: 0x0000567B
		public uint Unknown3 { get; set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060001FE RID: 510 RVA: 0x00007484 File Offset: 0x00005684
		// (set) Token: 0x060001FF RID: 511 RVA: 0x0000748C File Offset: 0x0000568C
		public string Unknown4 { get; set; }

		// Token: 0x06000200 RID: 512 RVA: 0x00007495 File Offset: 0x00005695
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteString(this.Unknown2, Encoding.ASCII);
			output.WriteValueU32(this.Unknown3, endian);
			output.WriteString(this.Unknown4, Encoding.ASCII);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x000074D4 File Offset: 0x000056D4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadString((int)(input.ReadValueU32() + 1U), true, Encoding.ASCII);
			this.Unknown3 = input.ReadValueU32(endian);
			this.Unknown4 = input.ReadString((int)(input.ReadValueU32() + 1U), true, Encoding.ASCII);
		}
	}
}
