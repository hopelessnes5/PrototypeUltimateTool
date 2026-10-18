using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000043 RID: 67
	[KnownType(69641U)]
	public class ShaderTemplate : BaseNode
	{
		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000261 RID: 609 RVA: 0x00007D0D File Offset: 0x00005F0D
		// (set) Token: 0x06000262 RID: 610 RVA: 0x00007D15 File Offset: 0x00005F15
		public string Name { get; set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000263 RID: 611 RVA: 0x00007D1E File Offset: 0x00005F1E
		// (set) Token: 0x06000264 RID: 612 RVA: 0x00007D26 File Offset: 0x00005F26
		public uint Unknown02 { get; set; }

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000265 RID: 613 RVA: 0x00007D2F File Offset: 0x00005F2F
		// (set) Token: 0x06000266 RID: 614 RVA: 0x00007D37 File Offset: 0x00005F37
		public uint NumPasses { get; set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000267 RID: 615 RVA: 0x00007D40 File Offset: 0x00005F40
		// (set) Token: 0x06000268 RID: 616 RVA: 0x00007D48 File Offset: 0x00005F48
		public uint Unknown04 { get; set; }

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000269 RID: 617 RVA: 0x00007D51 File Offset: 0x00005F51
		// (set) Token: 0x0600026A RID: 618 RVA: 0x00007D59 File Offset: 0x00005F59
		public uint Unknown05 { get; set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00007D62 File Offset: 0x00005F62
		// (set) Token: 0x0600026C RID: 620 RVA: 0x00007D6A File Offset: 0x00005F6A
		public uint NumFloat2 { get; set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600026D RID: 621 RVA: 0x00007D73 File Offset: 0x00005F73
		// (set) Token: 0x0600026E RID: 622 RVA: 0x00007D7B File Offset: 0x00005F7B
		public uint Unknown07 { get; set; }

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600026F RID: 623 RVA: 0x00007D84 File Offset: 0x00005F84
		// (set) Token: 0x06000270 RID: 624 RVA: 0x00007D8C File Offset: 0x00005F8C
		public uint Unknown08 { get; set; }

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x06000271 RID: 625 RVA: 0x00007D95 File Offset: 0x00005F95
		// (set) Token: 0x06000272 RID: 626 RVA: 0x00007D9D File Offset: 0x00005F9D
		public uint Unknown09 { get; set; }

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000273 RID: 627 RVA: 0x00007DA6 File Offset: 0x00005FA6
		// (set) Token: 0x06000274 RID: 628 RVA: 0x00007DAE File Offset: 0x00005FAE
		public uint NumFloat3 { get; set; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000275 RID: 629 RVA: 0x00007DB7 File Offset: 0x00005FB7
		// (set) Token: 0x06000276 RID: 630 RVA: 0x00007DBF File Offset: 0x00005FBF
		public uint Unknown11 { get; set; }

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000277 RID: 631 RVA: 0x00007DC8 File Offset: 0x00005FC8
		// (set) Token: 0x06000278 RID: 632 RVA: 0x00007DD0 File Offset: 0x00005FD0
		public uint Unknown12 { get; set; }

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000279 RID: 633 RVA: 0x00007DD9 File Offset: 0x00005FD9
		// (set) Token: 0x0600027A RID: 634 RVA: 0x00007DE1 File Offset: 0x00005FE1
		public uint NumFloat4 { get; set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x0600027B RID: 635 RVA: 0x00007DEA File Offset: 0x00005FEA
		// (set) Token: 0x0600027C RID: 636 RVA: 0x00007DF2 File Offset: 0x00005FF2
		public uint NumMatrices { get; set; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600027D RID: 637 RVA: 0x00007DFB File Offset: 0x00005FFB
		// (set) Token: 0x0600027E RID: 638 RVA: 0x00007E03 File Offset: 0x00006003
		public uint NumBools { get; set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600027F RID: 639 RVA: 0x00007E0C File Offset: 0x0000600C
		// (set) Token: 0x06000280 RID: 640 RVA: 0x00007E14 File Offset: 0x00006014
		public uint Unknown16 { get; set; }

		// Token: 0x06000281 RID: 641 RVA: 0x00007E1D File Offset: 0x0000601D
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00007E50 File Offset: 0x00006050
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown02, endian);
			output.WriteValueU32(this.NumPasses, endian);
			output.WriteValueU32(this.Unknown04, endian);
			output.WriteValueU32(this.Unknown05, endian);
			output.WriteValueU32(this.NumFloat2, endian);
			output.WriteValueU32(this.Unknown07, endian);
			output.WriteValueU32(this.Unknown08, endian);
			output.WriteValueU32(this.Unknown09, endian);
			output.WriteValueU32(this.NumFloat3, endian);
			output.WriteValueU32(this.Unknown11, endian);
			output.WriteValueU32(this.Unknown12, endian);
			output.WriteValueU32(this.NumFloat4, endian);
			output.WriteValueU32(this.NumMatrices, endian);
			output.WriteValueU32(this.NumBools, endian);
			output.WriteValueU32(this.Unknown16, endian);
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00007F2C File Offset: 0x0000612C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown02 = input.ReadValueU32(endian);
			this.NumPasses = input.ReadValueU32(endian);
			this.Unknown04 = input.ReadValueU32(endian);
			this.Unknown05 = input.ReadValueU32(endian);
			this.NumFloat2 = input.ReadValueU32(endian);
			this.Unknown07 = input.ReadValueU32(endian);
			this.Unknown08 = input.ReadValueU32(endian);
			this.Unknown09 = input.ReadValueU32(endian);
			this.NumFloat3 = input.ReadValueU32(endian);
			this.Unknown11 = input.ReadValueU32(endian);
			this.Unknown12 = input.ReadValueU32(endian);
			this.NumFloat4 = input.ReadValueU32(endian);
			this.NumMatrices = input.ReadValueU32(endian);
			this.NumBools = input.ReadValueU32(endian);
			this.Unknown16 = input.ReadValueU32(endian);
		}
	}
}
