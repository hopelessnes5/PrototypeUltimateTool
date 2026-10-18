using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000037 RID: 55
	[KnownType(65549U)]
	public class PrimitiveMatrix : BaseNode
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060001EA RID: 490 RVA: 0x000072F8 File Offset: 0x000054F8
		// (set) Token: 0x060001EB RID: 491 RVA: 0x00007300 File Offset: 0x00005500
		public uint Count { get; set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060001EC RID: 492 RVA: 0x00007309 File Offset: 0x00005509
		// (set) Token: 0x060001ED RID: 493 RVA: 0x00007311 File Offset: 0x00005511
		public uint[] Indices { get; set; }

		// Token: 0x060001EE RID: 494 RVA: 0x0000731C File Offset: 0x0000551C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Count, endian);
			for (uint num = 0U; num < this.Count; num += 1U)
			{
				output.WriteValueU32(this.Indices[(int)num], endian);
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00007358 File Offset: 0x00005558
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Count = input.ReadValueU32(endian);
			if (this.Count > 1000000U || this.Count > int.MaxValue ||
				(input.CanSeek && (long)this.Count * 4L > input.Length - input.Position))
				throw new FormatException("PrimitiveMatrix index count exceeds the remaining chunk payload.");
			this.Indices = new uint[(int)this.Count];
			for (uint num = 0U; num < this.Count; num += 1U)
			{
				this.Indices[(int)num] = input.ReadValueU32(endian);
			}
		}
	}
}
