using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000032 RID: 50
	[KnownType(135169U)]
	public class ExpressionGroup : BaseNode
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x060001AE RID: 430 RVA: 0x00006BE8 File Offset: 0x00004DE8
		// (set) Token: 0x060001AF RID: 431 RVA: 0x00006BF0 File Offset: 0x00004DF0
		public uint Version { get; set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x00006BF9 File Offset: 0x00004DF9
		// (set) Token: 0x060001B1 RID: 433 RVA: 0x00006C01 File Offset: 0x00004E01
		public string Name { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x060001B2 RID: 434 RVA: 0x00006C0A File Offset: 0x00004E0A
		// (set) Token: 0x060001B3 RID: 435 RVA: 0x00006C12 File Offset: 0x00004E12
		public string CompositeDrawableName { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x060001B4 RID: 436 RVA: 0x00006C1B File Offset: 0x00004E1B
		// (set) Token: 0x060001B5 RID: 437 RVA: 0x00006C23 File Offset: 0x00004E23
		public int Count { get; set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x00006C2C File Offset: 0x00004E2C
		// (set) Token: 0x060001B7 RID: 439 RVA: 0x00006C34 File Offset: 0x00004E34
		public uint[] Unknown4 { get; set; }

		// Token: 0x060001B8 RID: 440 RVA: 0x00006C3D File Offset: 0x00004E3D
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00006C70 File Offset: 0x00004E70
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.CompositeDrawableName);
			output.WriteValueS32(this.Unknown4.Length, endian);
			for (int i = 0; i < this.Unknown4.Length; i++)
			{
				output.WriteValueU32(this.Unknown4[i], endian);
			}
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00006CD4 File Offset: 0x00004ED4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.CompositeDrawableName = input.ReadStringAlignedU8();
			this.Count = input.ReadValueS32(endian);
			if (this.Count < 0 || this.Count > 1000000 || (input.CanSeek && (long)this.Count * 4L > input.Length - input.Position))
				throw new FormatException("ExpressionGroup count exceeds the remaining chunk payload.");
			this.Unknown4 = new uint[this.Count];
			for (int i = 0; i < this.Count; i++)
			{
				this.Unknown4[i] = input.ReadValueU32(endian);
			}
		}
	}
}
