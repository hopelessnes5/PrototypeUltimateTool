using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x02000082 RID: 130
	[KnownGame(PrototypeGame.P2)]
	[KnownType(65603U)]
	public class P2BufferDescriptor : BaseNode
	{
		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000549 RID: 1353 RVA: 0x0000BE91 File Offset: 0x0000A091
		// (set) Token: 0x0600054A RID: 1354 RVA: 0x0000BE99 File Offset: 0x0000A099
		public uint AmountOfDescriptions { get; set; }

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x0600054B RID: 1355 RVA: 0x0000BEA2 File Offset: 0x0000A0A2
		// (set) Token: 0x0600054C RID: 1356 RVA: 0x0000BEAA File Offset: 0x0000A0AA
		public uint DescriptionSize { get; set; }

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x0600054D RID: 1357 RVA: 0x0000BEB3 File Offset: 0x0000A0B3
		// (set) Token: 0x0600054E RID: 1358 RVA: 0x0000BEBB File Offset: 0x0000A0BB
		public P2Description[] Descriptions { get; set; }

		// Token: 0x0600054F RID: 1359 RVA: 0x0000BEC4 File Offset: 0x0000A0C4
		public override string ToString()
		{
			return "BufferDescriptor";
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0000BECC File Offset: 0x0000A0CC
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.AmountOfDescriptions, endian);
			for (uint num = 0U; num < this.AmountOfDescriptions; num += 1U)
			{
				this.Descriptions[(int)num].Serialize(output, endian);
			}
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0000BF08 File Offset: 0x0000A108
		public override void Deserialize(Stream input, Endian endian)
		{
			this.AmountOfDescriptions = input.ReadValueU32(endian);
			long available = input.CanSeek ? input.Length - input.Position : -1L;
			// A P2 descriptor contains seven 32-bit fields plus an aligned name. 28 bytes is
			// a conservative lower bound, used before allocating the descriptor array.
			if (this.AmountOfDescriptions == 0U || this.AmountOfDescriptions > 100000U || this.AmountOfDescriptions > int.MaxValue ||
				(input.CanSeek && (long)this.AmountOfDescriptions > available / 28L))
				throw new FormatException("P2BufferDescriptor has an invalid descriptor count.");
			this.Descriptions = new P2Description[(int)this.AmountOfDescriptions];
			for (uint num = 0U; num < this.AmountOfDescriptions; num += 1U)
				this.Descriptions[(int)num] = new P2Description(input, endian);
			this.DescriptionSize = this.Descriptions[0].ItemSize;
		}
	}
}
