using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200006C RID: 108
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65543U)]
	public class RawUVBuffer : BaseNode
	{
		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x0000A85D File Offset: 0x00008A5D
		// (set) Token: 0x06000458 RID: 1112 RVA: 0x0000A865 File Offset: 0x00008A65
		public uint BufferSize { get; set; }

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x0000A86E File Offset: 0x00008A6E
		// (set) Token: 0x0600045A RID: 1114 RVA: 0x0000A876 File Offset: 0x00008A76
		public byte[] Blank { get; set; }

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x0600045B RID: 1115 RVA: 0x0000A87F File Offset: 0x00008A7F
		// (set) Token: 0x0600045C RID: 1116 RVA: 0x0000A887 File Offset: 0x00008A87
		public UVCoordinate[] UVs { get; set; }

		// Token: 0x0600045D RID: 1117 RVA: 0x0000A890 File Offset: 0x00008A90
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			output.WriteBytes(this.Blank);
			for (int i = 0; i < this.UVs.Length; i++)
			{
				this.UVs[i].Serialize(output, endian);
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000A8D8 File Offset: 0x00008AD8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.BufferSize = input.ReadValueU32(endian);
			this.Blank = input.ReadBytes(4);
			if (this.Blank == null || this.Blank.Length != 4)
				throw new EndOfStreamException("Prototype 1 UV buffer header is truncated.");
			if (this.BufferSize > 2000000U || this.BufferSize > (uint)(int.MaxValue / 8) ||
				(input.CanSeek && (long)this.BufferSize * 8L > input.Length - input.Position))
				throw new FormatException("Prototype 1 UV count exceeds the remaining chunk payload.");
			this.UVs = new UVCoordinate[(int)this.BufferSize];
			int num = 0;
			while ((long)num < (long)((ulong)this.BufferSize))
			{
				this.UVs[num] = new UVCoordinate(input, endian);
				num++;
			}
		}
	}
}
