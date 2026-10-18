using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000078 RID: 120
	public class DataBlock
	{
		// Token: 0x17000181 RID: 385
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x0000B06D File Offset: 0x0000926D
		// (set) Token: 0x060004C3 RID: 1219 RVA: 0x0000B075 File Offset: 0x00009275
		public byte[] Unknown1 { get; set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0000B07E File Offset: 0x0000927E
		// (set) Token: 0x060004C5 RID: 1221 RVA: 0x0000B086 File Offset: 0x00009286
		public string Type { get; set; }

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0000B08F File Offset: 0x0000928F
		// (set) Token: 0x060004C7 RID: 1223 RVA: 0x0000B097 File Offset: 0x00009297
		public byte[] Unknown2 { get; set; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x060004C8 RID: 1224 RVA: 0x0000B0A0 File Offset: 0x000092A0
		// (set) Token: 0x060004C9 RID: 1225 RVA: 0x0000B0A8 File Offset: 0x000092A8
		public string Name { get; set; }

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x0000B0B1 File Offset: 0x000092B1
		// (set) Token: 0x060004CB RID: 1227 RVA: 0x0000B0B9 File Offset: 0x000092B9
		public uint Length { get; set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x060004CC RID: 1228 RVA: 0x0000B0C2 File Offset: 0x000092C2
		// (set) Token: 0x060004CD RID: 1229 RVA: 0x0000B0CA File Offset: 0x000092CA
		public byte[][] Value { get; set; }

		// Token: 0x060004CE RID: 1230 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public DataBlock()
		{
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x0000B0D3 File Offset: 0x000092D3
		public DataBlock(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x0000B0E4 File Offset: 0x000092E4
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Unknown1);
			output.WriteValueU32((uint)(this.Type.Length - 1), endian);
			output.WriteString(this.Type);
			output.WriteBytes(this.Unknown2);
			output.WriteValueU32((uint)(this.Name.Length - 1), endian);
			output.WriteString(this.Name);
			string type = this.Type;
			if (type == "String\0")
			{
				output.WriteValueU32((uint)(this.Value[0].Length - 1), endian);
				output.WriteBytes(this.Value[0]);
				return;
			}
			if (type == "String List\0")
			{
				output.WriteValueU32(this.Length, endian);
				int num = 0;
				while ((long)num < (long)((ulong)this.Length))
				{
					output.WriteValueU32((uint)(this.Value[num].Length - 1), endian);
					output.WriteBytes(this.Value[num]);
					num++;
				}
				return;
			}
			if (!(type == "EnvelopeData\0"))
			{
				output.WriteBytes(this.Value[0]);
				return;
			}
			output.WriteValueU32(this.Length, endian);
			int num2 = 0;
			while ((long)num2 < (long)((ulong)this.Length))
			{
				output.WriteBytes(this.Value[num2]);
				num2++;
			}
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x0000B220 File Offset: 0x00009420
		public void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = ReadExact(input, 9);
			uint num = input.ReadValueU32(endian);
			this.Type = ReadLengthPrefixedString(input, num);
			this.Unknown2 = ReadExact(input, 9);
			num = input.ReadValueU32(endian);
			this.Name = ReadLengthPrefixedString(input, num);
			string type = this.Type;
			if (type == "Float32\0") { this.Value = new byte[1][]; this.Value[0] = ReadExact(input, 4); return; }
			if (type == "Vector\0") { this.Value = new byte[1][]; this.Value[0] = ReadExact(input, 12); return; }
			if (type == "Bool\0") { this.Value = new byte[1][]; this.Value[0] = ReadExact(input, 1); return; }
			if (type == "String\0")
			{
				this.Value = new byte[1][];
				num = input.ReadValueU32(endian);
				this.Value[0] = ReadLengthPrefixedBytes(input, num);
				return;
			}
			if (type == "String List\0")
			{
				this.Length = input.ReadValueU32(endian);
				if (this.Length > 1000000U || this.Length > int.MaxValue || (input.CanSeek && (long)this.Length > (input.Length - input.Position) / 4L))
					throw new FormatException("Sound String List count exceeds its chunk payload.");
				this.Value = new byte[(int)this.Length][];
				for (int i = 0; i < this.Value.Length; i++)
				{
					num = input.ReadValueU32(endian);
					this.Value[i] = ReadLengthPrefixedBytes(input, num);
				}
				return;
			}
			if (type != "EnvelopeData\0") throw new FormatException("Unknown patch type");
			this.Length = input.ReadValueU32(endian);
			if (this.Length > 1000000U || this.Length > int.MaxValue || (input.CanSeek && (long)this.Length > (input.Length - input.Position) / 8L))
				throw new FormatException("Sound EnvelopeData count exceeds its chunk payload.");
			this.Value = new byte[(int)this.Length][];
			for (int i = 0; i < this.Value.Length; i++) this.Value[i] = ReadExact(input, 8);
		}

        private static byte[] ReadExact(Stream input, int count)
        {
            if (count < 0 || (input.CanSeek && count > input.Length - input.Position))
                throw new FormatException("Sound data exceeds its chunk payload.");
            byte[] bytes = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = input.Read(bytes, offset, count - offset);
                if (read <= 0) throw new EndOfStreamException("Sound data is truncated.");
                offset += read;
            }
            return bytes;
        }

        private static string ReadLengthPrefixedString(Stream input, uint lengthMinusTerminator)
        {
            return System.Text.Encoding.ASCII.GetString(ReadLengthPrefixedBytes(input, lengthMinusTerminator));
        }

        private static byte[] ReadLengthPrefixedBytes(Stream input, uint lengthMinusTerminator)
        {
            if (lengthMinusTerminator >= int.MaxValue) throw new FormatException("Sound string length is too large.");
            return ReadExact(input, checked((int)lengthMinusTerminator + 1));
        }
	}
}
