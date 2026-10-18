using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200003F RID: 63
	[KnownType(98818U)]
	public class TextBibleHolder : BaseNode
	{
		// Token: 0x1700009C RID: 156
		// (get) Token: 0x0600022E RID: 558 RVA: 0x00007895 File Offset: 0x00005A95
		// (set) Token: 0x0600022F RID: 559 RVA: 0x0000789D File Offset: 0x00005A9D
		public string Language { get; set; }

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000230 RID: 560 RVA: 0x000078A6 File Offset: 0x00005AA6
		// (set) Token: 0x06000231 RID: 561 RVA: 0x000078AE File Offset: 0x00005AAE
		public uint Version { get; set; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000232 RID: 562 RVA: 0x000078B7 File Offset: 0x00005AB7
		// (set) Token: 0x06000233 RID: 563 RVA: 0x000078BF File Offset: 0x00005ABF
		public uint Count { get; set; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000234 RID: 564 RVA: 0x000078C8 File Offset: 0x00005AC8
		// (set) Token: 0x06000235 RID: 565 RVA: 0x000078D0 File Offset: 0x00005AD0
		public List<string> Keys { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000236 RID: 566 RVA: 0x000078D9 File Offset: 0x00005AD9
		// (set) Token: 0x06000237 RID: 567 RVA: 0x000078E1 File Offset: 0x00005AE1
		public List<uint> StringStarts { get; set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000238 RID: 568 RVA: 0x000078EA File Offset: 0x00005AEA
		// (set) Token: 0x06000239 RID: 569 RVA: 0x000078F2 File Offset: 0x00005AF2
		public List<uint> StringStops { get; set; }

		// Token: 0x0600023A RID: 570 RVA: 0x000078FC File Offset: 0x00005AFC
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Language);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Count, endian);
			int num = 0;
			while ((long)num < (long)((ulong)this.Count))
			{
				output.WriteStringAlignedU8(this.Keys[num]);
				num++;
			}
			int num2 = 0;
			while ((long)num2 < (long)((ulong)this.Count))
			{
				output.WriteValueU32(this.StringStarts[num2], endian);
				num2++;
			}
			int num3 = 0;
			while ((long)num3 < (long)((ulong)this.Count))
			{
				output.WriteValueU32(this.StringStops[num3], endian);
				num3++;
			}
		}

		// Token: 0x0600023B RID: 571 RVA: 0x000079A0 File Offset: 0x00005BA0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Language = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.Count = input.ReadValueU32(endian);
			this.Keys = new List<string>();
			for (uint num = 0U; num < this.Count; num += 1U)
			{
				this.Keys.Add(input.ReadStringAlignedU8());
			}
			this.StringStarts = new List<uint>();
			for (uint num2 = 0U; num2 < this.Count; num2 += 1U)
			{
				this.StringStarts.Add(input.ReadValueU32(endian));
			}
			this.StringStops = new List<uint>();
			for (uint num3 = 0U; num3 < this.Count; num3 += 1U)
			{
				this.StringStops.Add(input.ReadValueU32(endian));
			}
		}
	}
}
