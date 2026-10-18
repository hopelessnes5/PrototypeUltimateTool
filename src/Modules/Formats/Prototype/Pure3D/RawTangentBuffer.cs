using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200006A RID: 106
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65576U)]
	public class RawTangentBuffer : BaseNode
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000449 RID: 1097 RVA: 0x0000A705 File Offset: 0x00008905
		// (set) Token: 0x0600044A RID: 1098 RVA: 0x0000A70D File Offset: 0x0000890D
		public uint BufferSize { get; set; }

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600044B RID: 1099 RVA: 0x0000A716 File Offset: 0x00008916
		// (set) Token: 0x0600044C RID: 1100 RVA: 0x0000A71E File Offset: 0x0000891E
		public Vector4[] Tangents { get; set; }

		// Token: 0x0600044D RID: 1101 RVA: 0x0000A728 File Offset: 0x00008928
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			for (int i = 0; i < this.Tangents.Length; i++)
			{
				this.Tangents[i].Serialize(output, endian);
			}
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x0000A764 File Offset: 0x00008964
		public override void Deserialize(Stream input, Endian endian)
		{
			this.BufferSize = input.ReadValueU32(endian);
			if (this.BufferSize > 2000000U || this.BufferSize > (uint)(int.MaxValue / 16) ||
				(input.CanSeek && (long)this.BufferSize * 16L > input.Length - input.Position))
				throw new FormatException("Prototype 1 raw tangent count exceeds the remaining chunk payload.");
			this.Tangents = new Vector4[(int)this.BufferSize];
			int num = 0;
			while ((long)num < (long)((ulong)this.BufferSize))
			{
				this.Tangents[num] = new Vector4(input, endian);
				num++;
			}
		}
	}
}
