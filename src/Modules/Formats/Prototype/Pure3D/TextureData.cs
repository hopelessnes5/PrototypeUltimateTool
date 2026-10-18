using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005C RID: 92
	[KnownGame(PrototypeGame.Any)]
	[KnownType(102402U)]
	public class TextureData : BaseNode
	{
		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x00009A71 File Offset: 0x00007C71
		// (set) Token: 0x060003CA RID: 970 RVA: 0x00009A79 File Offset: 0x00007C79
		[Category("Image")]
		[ReadOnly(true)]
		public byte[] Data { get; set; }

		// Token: 0x060003CB RID: 971 RVA: 0x00009A84 File Offset: 0x00007C84
		public override string ToString()
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Data.Length.ToString(CultureInfo.InvariantCulture) + " bytes)";
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00009AD3 File Offset: 0x00007CD3
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32(this.Data.Length, endian);
			output.Write(this.Data, 0, this.Data.Length);
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00009AFC File Offset: 0x00007CFC
		public override void Deserialize(Stream input, Endian endian)
		{
			int num = input.ReadValueS32(endian);
			if (num < 0) throw new FormatException("TextureData declares a negative byte count.");
			if (input.CanSeek && (long)num > input.Length - input.Position)
				throw new FormatException("TextureData byte count exceeds the declared chunk payload.");
			this.Data = new byte[num];
			int offset = 0;
			while (offset < this.Data.Length)
			{
				int read = input.Read(this.Data, offset, this.Data.Length - offset);
				if (read <= 0) throw new EndOfStreamException("TextureData payload is truncated.");
				offset += read;
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x060003CE RID: 974 RVA: 0x00009B33 File Offset: 0x00007D33
		public override bool Exportable
		{
			get
			{
				return this.Data != null && this.Data.Length != 0;
			}
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060003CF RID: 975 RVA: 0x0000887B File Offset: 0x00006A7B
		public override string ExportExtension
		{
			get
			{
				return "dds";
			}
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00009B49 File Offset: 0x00007D49
		public override void Export(Stream output)
		{
			output.Write(this.Data, 0, this.Data.Length);
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x00003219 File Offset: 0x00001419
		public override bool Importable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00009B60 File Offset: 0x00007D60
		public override void Import(Stream input)
		{
			this.Data = new byte[input.Length];
			input.ReadExactly(this.Data);
		}
	}
}
