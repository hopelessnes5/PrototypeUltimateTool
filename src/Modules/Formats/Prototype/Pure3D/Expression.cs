using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000030 RID: 48
	[KnownType(135168U)]
	public class Expression : BaseNode
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000197 RID: 407 RVA: 0x000069C5 File Offset: 0x00004BC5
		// (set) Token: 0x06000198 RID: 408 RVA: 0x000069CD File Offset: 0x00004BCD
		public uint Version { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000199 RID: 409 RVA: 0x000069D6 File Offset: 0x00004BD6
		// (set) Token: 0x0600019A RID: 410 RVA: 0x000069DE File Offset: 0x00004BDE
		public string Name { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600019B RID: 411 RVA: 0x000069E7 File Offset: 0x00004BE7
		// (set) Token: 0x0600019C RID: 412 RVA: 0x000069EF File Offset: 0x00004BEF
		public uint Count { get; set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600019D RID: 413 RVA: 0x000069F8 File Offset: 0x00004BF8
		// (set) Token: 0x0600019E RID: 414 RVA: 0x00006A00 File Offset: 0x00004C00
		public float[] Unknown3 { get; set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600019F RID: 415 RVA: 0x00006A09 File Offset: 0x00004C09
		// (set) Token: 0x060001A0 RID: 416 RVA: 0x00006A11 File Offset: 0x00004C11
		public uint[] Unknown4 { get; set; }

		// Token: 0x060001A1 RID: 417 RVA: 0x00006A1A File Offset: 0x00004C1A
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00006A4C File Offset: 0x00004C4C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Count, endian);
			uint num = 0U;
			while ((ulong)num < (ulong)((long)this.Unknown3.Length))
			{
				output.WriteValueF32(this.Unknown3[(int)num], endian);
				num += 1U;
			}
			uint num2 = 0U;
			while ((ulong)num2 < (ulong)((long)this.Unknown4.Length))
			{
				output.WriteValueU32(this.Unknown4[(int)num2], endian);
				num2 += 1U;
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00006AC8 File Offset: 0x00004CC8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.Count = input.ReadValueU32(endian);
			if (this.Count > 1000000U || this.Count > int.MaxValue ||
				(input.CanSeek && (long)this.Count * 8L > input.Length - input.Position))
				throw new FormatException("Expression count exceeds the remaining chunk payload.");
			this.Unknown3 = new float[(int)this.Count];
			for (uint num = 0U; num < this.Count; num += 1U)
			{
				this.Unknown3[(int)num] = input.ReadValueF32(endian);
			}
			this.Unknown4 = new uint[this.Count];
			for (uint num2 = 0U; num2 < this.Count; num2 += 1U)
			{
				this.Unknown4[(int)num2] = input.ReadValueU32(endian);
			}
		}
	}
}
