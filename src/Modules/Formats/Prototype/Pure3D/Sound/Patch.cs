using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000079 RID: 121
	public class Patch : AudioType
	{
		// Token: 0x17000187 RID: 391
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0000B3E8 File Offset: 0x000095E8
		// (set) Token: 0x060004D3 RID: 1235 RVA: 0x0000B3F0 File Offset: 0x000095F0
		public byte[] Unknown1 { get; set; }

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x0000B3F9 File Offset: 0x000095F9
		// (set) Token: 0x060004D5 RID: 1237 RVA: 0x0000B401 File Offset: 0x00009601
		public string Type { get; set; }

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060004D6 RID: 1238 RVA: 0x0000B40A File Offset: 0x0000960A
		// (set) Token: 0x060004D7 RID: 1239 RVA: 0x0000B412 File Offset: 0x00009612
		public uint AmountOfBlocks { get; set; }

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060004D8 RID: 1240 RVA: 0x0000B41B File Offset: 0x0000961B
		// (set) Token: 0x060004D9 RID: 1241 RVA: 0x0000B423 File Offset: 0x00009623
		public DataBlock[] DataBlocks { get; set; }

		// Token: 0x060004DA RID: 1242 RVA: 0x0000AD2D File Offset: 0x00008F2D
		public Patch()
		{
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0000AD35 File Offset: 0x00008F35
		public Patch(Stream input, Endian endian) : base(input, endian)
		{
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0000B42C File Offset: 0x0000962C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Unknown1);
			output.WriteValueU32((uint)(this.Type.Length - 1), endian);
			output.WriteString(this.Type);
			output.WriteByte(0);
			output.WriteValueU32(this.AmountOfBlocks, endian);
			int num = 0;
			while ((long)num < (long)((ulong)this.AmountOfBlocks))
			{
				this.DataBlocks[num].Serialize(output, endian);
				num++;
			}
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0000B49C File Offset: 0x0000969C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadBytes(9);
			if (this.Unknown1 == null || this.Unknown1.Length != 9)
				throw new EndOfStreamException("Sound Patch header is truncated.");
			uint num = input.ReadValueU32(endian);
			if (num >= int.MaxValue || (input.CanSeek && (long)num + 1L > input.Length - input.Position))
				throw new FormatException("Sound Patch type string exceeds the chunk payload.");
			this.Type = input.ReadString((int)num + 1);
			if (input.ReadByte() < 0) throw new EndOfStreamException("Sound Patch separator is missing.");
			this.AmountOfBlocks = input.ReadValueU32(endian);
			if (this.AmountOfBlocks > 100000U || this.AmountOfBlocks > int.MaxValue || (input.CanSeek && (long)this.AmountOfBlocks > (input.Length - input.Position) / 29L))
				throw new FormatException("Sound Patch block count exceeds the remaining chunk payload.");
			this.DataBlocks = new DataBlock[(int)this.AmountOfBlocks];
			int num2 = 0;
			while ((long)num2 < (long)((ulong)this.AmountOfBlocks))
			{
				this.DataBlocks[num2] = new DataBlock(input, endian);
				num2++;
			}
		}
	}
}
