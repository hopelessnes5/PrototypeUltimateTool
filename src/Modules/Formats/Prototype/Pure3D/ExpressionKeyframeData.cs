using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200006F RID: 111
	[KnownType(69376U)]
	public class ExpressionKeyframeData : BaseNode
	{
		// Token: 0x17000168 RID: 360
		// (get) Token: 0x0600046E RID: 1134 RVA: 0x0000A9D9 File Offset: 0x00008BD9
		// (set) Token: 0x0600046F RID: 1135 RVA: 0x0000A9E1 File Offset: 0x00008BE1
		public uint Unknown1 { get; set; }

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000470 RID: 1136 RVA: 0x0000A9EA File Offset: 0x00008BEA
		// (set) Token: 0x06000471 RID: 1137 RVA: 0x0000A9F2 File Offset: 0x00008BF2
		public string Type { get; set; }

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000472 RID: 1138 RVA: 0x0000A9FB File Offset: 0x00008BFB
		// (set) Token: 0x06000473 RID: 1139 RVA: 0x0000AA03 File Offset: 0x00008C03
		public uint NumberOfVertices { get; set; }

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000474 RID: 1140 RVA: 0x0000AA0C File Offset: 0x00008C0C
		// (set) Token: 0x06000475 RID: 1141 RVA: 0x0000AA14 File Offset: 0x00008C14
		public VertexAnimBuffer[] AnimBuffers { get; set; }

		// Token: 0x06000476 RID: 1142 RVA: 0x0000AA20 File Offset: 0x00008C20
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteString(this.Type);
			output.WriteValueU32(this.NumberOfVertices, endian);
			int num = 0;
			while ((long)num < (long)((ulong)this.NumberOfVertices))
			{
				output.WriteValueU32(this.AnimBuffers[num].Index, endian);
				output.WriteBytes(this.AnimBuffers[num].Data);
				num++;
			}
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x0000AA90 File Offset: 0x00008C90
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Type = input.ReadString(4);
			this.NumberOfVertices = input.ReadValueU32(endian);
			if (this.NumberOfVertices > 1000000U || this.NumberOfVertices > int.MaxValue ||
				(input.CanSeek && (long)this.NumberOfVertices * 16L > input.Length - input.Position))
				throw new FormatException("ExpressionKeyframeData vertex count exceeds the remaining chunk payload.");
			this.AnimBuffers = new VertexAnimBuffer[(int)this.NumberOfVertices];
			int num = 0;
			while ((long)num < (long)((ulong)this.NumberOfVertices))
			{
				this.AnimBuffers[num] = new VertexAnimBuffer();
				this.AnimBuffers[num].Index = input.ReadValueU32(endian);
				this.AnimBuffers[num].Data = input.ReadBytes(12);
				num++;
			}
		}
	}
}
