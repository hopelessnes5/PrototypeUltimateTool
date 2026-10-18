using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x0200007B RID: 123
	[KnownGame(PrototypeGame.P2)]
	[KnownType(98817U)]
	public class ScaleformData : BaseNode
	{
		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x0000B540 File Offset: 0x00009740
		// (set) Token: 0x060004E4 RID: 1252 RVA: 0x0000B548 File Offset: 0x00009748
		public string Name { get; set; }

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060004E5 RID: 1253 RVA: 0x0000B551 File Offset: 0x00009751
		// (set) Token: 0x060004E6 RID: 1254 RVA: 0x0000B559 File Offset: 0x00009759
		public int Unknown1 { get; set; }

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060004E7 RID: 1255 RVA: 0x0000B562 File Offset: 0x00009762
		// (set) Token: 0x060004E8 RID: 1256 RVA: 0x0000B56A File Offset: 0x0000976A
		public byte[] Data { get; set; }

		// Token: 0x060004E9 RID: 1257 RVA: 0x0000B573 File Offset: 0x00009773
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0000B5AF File Offset: 0x000097AF
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueS32(this.Unknown1, endian);
			output.WriteValueS32(this.Data.Length, endian);
			output.WriteBytes(this.Data);
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0000B5E8 File Offset: 0x000097E8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown1 = input.ReadValueS32(endian);
			int length = input.ReadValueS32(endian);
			this.Data = input.ReadBytes(length);
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x0000B623 File Offset: 0x00009823
		public override bool Exportable
		{
			get
			{
				return this.Data != null && this.Data.Length != 0;
			}
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x0000B639 File Offset: 0x00009839
		public override string ExportExtension
		{
			get
			{
				return "gfx";
			}
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0000B640 File Offset: 0x00009840
		public override void Export(Stream output)
		{
			output.Write(this.Data, 0, this.Data.Length);
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x00003219 File Offset: 0x00001419
		public override bool Importable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0000B657 File Offset: 0x00009857
		public override void Import(Stream input)
		{
			if (input.Length > 268435456L || (input.CanSeek && input.Length - input.Position > int.MaxValue))
				throw new FormatException("ScaleformData payload is too large.");
			int length = checked((int)(input.Length - input.Position));
			this.Data = new byte[length];
			int offset = 0;
			while (offset < this.Data.Length)
			{
				int read = input.Read(this.Data, offset, this.Data.Length - offset);
				if (read <= 0) throw new EndOfStreamException("ScaleformData payload is truncated.");
				offset += read;
			}
		}
	}
}
