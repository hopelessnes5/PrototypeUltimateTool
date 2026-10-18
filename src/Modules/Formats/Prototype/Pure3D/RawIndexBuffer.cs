using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000068 RID: 104
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65546U)]
	public class RawIndexBuffer : BaseNode
	{
		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x0000A59F File Offset: 0x0000879F
		// (set) Token: 0x0600043C RID: 1084 RVA: 0x0000A5A7 File Offset: 0x000087A7
		public uint BufferSize { get; set; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x0000A5B0 File Offset: 0x000087B0
		// (set) Token: 0x0600043E RID: 1086 RVA: 0x0000A5B8 File Offset: 0x000087B8
		public Vector3Int[] Faces { get; set; }

		// Token: 0x0600043F RID: 1087 RVA: 0x0000A5C4 File Offset: 0x000087C4
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			for (int i = 0; i < this.Faces.Length; i++)
			{
				this.Faces[i].Serialize(output, endian);
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x0000A600 File Offset: 0x00008800
		public override void Deserialize(Stream input, Endian endian)
		{
			this.BufferSize = input.ReadValueU32(endian);
			if (this.BufferSize % 3U != 0U || this.BufferSize > 6000000U || this.BufferSize > int.MaxValue ||
				(input.CanSeek && (long)this.BufferSize * 4L > input.Length - input.Position))
				throw new FormatException("Prototype 1 raw index count exceeds the remaining chunk payload.");
			uint num = this.BufferSize / 3U;
			this.Faces = new Vector3Int[(int)num];
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				this.Faces[num2] = new Vector3Int(input, endian);
				num2++;
			}
		}
	}
}
