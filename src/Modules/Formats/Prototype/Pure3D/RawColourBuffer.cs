using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000038 RID: 56
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65544U)]
	public class RawColourBuffer : BaseNode
	{
		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060001F1 RID: 497 RVA: 0x000073A3 File Offset: 0x000055A3
		// (set) Token: 0x060001F2 RID: 498 RVA: 0x000073AB File Offset: 0x000055AB
		public uint BufferSize { get; set; }

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060001F3 RID: 499 RVA: 0x000073B4 File Offset: 0x000055B4
		// (set) Token: 0x060001F4 RID: 500 RVA: 0x000073BC File Offset: 0x000055BC
		public uint[] Colours { get; set; }

		// Token: 0x060001F5 RID: 501 RVA: 0x000073C8 File Offset: 0x000055C8
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			for (int i = 0; i < this.Colours.Length; i++)
			{
				output.WriteValueU32(this.Colours[i], endian);
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00007404 File Offset: 0x00005604
		public override void Deserialize(Stream input, Endian endian)
		{
			this.BufferSize = input.ReadValueU32(endian);
			if (this.BufferSize > 2000000U || this.BufferSize > int.MaxValue ||
				(input.CanSeek && (long)this.BufferSize * 4L > input.Length - input.Position))
				throw new FormatException("Prototype 1 colour count exceeds the remaining chunk payload.");
			this.Colours = new uint[(int)this.BufferSize];
			int num = 0;
			while ((long)num < (long)((ulong)this.BufferSize))
			{
				this.Colours[num] = input.ReadValueU32(endian);
				num++;
			}
		}
	}
}
