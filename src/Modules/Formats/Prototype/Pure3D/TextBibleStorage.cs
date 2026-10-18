using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000045 RID: 69
	[KnownType(98817U)]
	public class TextBibleStorage : BaseNode
	{
		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000293 RID: 659 RVA: 0x0000812D File Offset: 0x0000632D
		// (set) Token: 0x06000294 RID: 660 RVA: 0x00008135 File Offset: 0x00006335
		public string Unknown1 { get; set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000295 RID: 661 RVA: 0x0000813E File Offset: 0x0000633E
		// (set) Token: 0x06000296 RID: 662 RVA: 0x00008146 File Offset: 0x00006346
		public uint Unknown2 { get; set; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000297 RID: 663 RVA: 0x0000814F File Offset: 0x0000634F
		// (set) Token: 0x06000298 RID: 664 RVA: 0x00008157 File Offset: 0x00006357
		[ReadOnly(true)]
		public byte[] Data { get; set; }

		// Token: 0x06000299 RID: 665 RVA: 0x00008160 File Offset: 0x00006360
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Unknown1);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteValueU32((uint)this.Data.Length, endian);
			output.Write(this.Data, 0, this.Data.Length);
		}

		// Token: 0x0600029A RID: 666 RVA: 0x000081A0 File Offset: 0x000063A0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadStringAlignedU8();
			this.Unknown2 = input.ReadValueU32(endian);
			uint num = input.ReadValueU32(endian);
			if (num > int.MaxValue) throw new FormatException("TextBibleStorage payload is too large.");
			if (input.CanSeek && (long)num > input.Length - input.Position)
				throw new FormatException("TextBibleStorage byte count exceeds the declared chunk payload.");
			this.Data = new byte[(int)num];
			int offset = 0;
			while (offset < this.Data.Length)
			{
				int read = input.Read(this.Data, offset, this.Data.Length - offset);
				if (read <= 0) throw new EndOfStreamException("TextBibleStorage payload is truncated.");
				offset += read;
			}
		}
	}
}
