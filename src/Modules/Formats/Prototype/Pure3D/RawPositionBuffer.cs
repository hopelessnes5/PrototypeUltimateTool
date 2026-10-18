using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000069 RID: 105
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65541U)]
	public class RawPositionBuffer : BaseNode
	{
		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x0000A64C File Offset: 0x0000884C
		// (set) Token: 0x06000443 RID: 1091 RVA: 0x0000A654 File Offset: 0x00008854
		public uint BufferSize { get; set; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x0000A65D File Offset: 0x0000885D
		// (set) Token: 0x06000445 RID: 1093 RVA: 0x0000A665 File Offset: 0x00008865
		public Vector3[] Position { get; set; }

		// Token: 0x06000446 RID: 1094 RVA: 0x0000A670 File Offset: 0x00008870
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			for (int i = 0; i < this.Position.Length; i++)
			{
				this.Position[i].Serialize(output, endian);
			}
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x0000A6AC File Offset: 0x000088AC
		public override void Deserialize(Stream input, Endian endian)
		{
			// EditP3D's LegacyPositionStream (same chunk type 0x00010005) reads this field
			// as the number of Vector3 elements. Do not infer the stream length from the
			// parent PrimitiveGroup.NumVertices: some assets have a mismatch between these values.
			this.BufferSize = input.ReadValueU32(endian);
			if (this.BufferSize > 2000000U || this.BufferSize > (uint)(int.MaxValue / 12))
				throw new FormatException("Prototype 1 raw position buffer is too large.");
			if (input.CanSeek && (long)this.BufferSize * 12L > input.Length - input.Position)
				throw new FormatException("Prototype 1 raw position count exceeds the remaining chunk payload.");

			this.Position = new Vector3[(int)this.BufferSize];
			for (int i = 0; i < this.Position.Length; i++)
			{
				this.Position[i] = new Vector3(input, endian);
			}
		}
	}
}
