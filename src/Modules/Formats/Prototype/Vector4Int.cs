using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000016 RID: 22
	[TypeConverter(typeof(VectorTypeConverter))]
	public class Vector4Int
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600009B RID: 155 RVA: 0x00003C10 File Offset: 0x00001E10
		// (set) Token: 0x0600009C RID: 156 RVA: 0x00003C18 File Offset: 0x00001E18
		public ushort X { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600009D RID: 157 RVA: 0x00003C21 File Offset: 0x00001E21
		// (set) Token: 0x0600009E RID: 158 RVA: 0x00003C29 File Offset: 0x00001E29
		public ushort Y { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600009F RID: 159 RVA: 0x00003C32 File Offset: 0x00001E32
		// (set) Token: 0x060000A0 RID: 160 RVA: 0x00003C3A File Offset: 0x00001E3A
		public ushort Z { get; set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00003C43 File Offset: 0x00001E43
		// (set) Token: 0x060000A2 RID: 162 RVA: 0x00003C4B File Offset: 0x00001E4B
		public ushort W { get; set; }

		// Token: 0x060000A3 RID: 163 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector4Int()
		{
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00003C54 File Offset: 0x00001E54
		public Vector4Int(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00003C64 File Offset: 0x00001E64
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU16(this.X, endian);
			output.WriteValueU16(this.Y, endian);
			output.WriteValueU16(this.Z, endian);
			output.WriteValueU16(this.W, endian);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00003C9A File Offset: 0x00001E9A
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueU16(endian);
			this.Y = input.ReadValueU16(endian);
			this.Z = input.ReadValueU16(endian);
			this.W = input.ReadValueU16(endian);
		}
	}
}
