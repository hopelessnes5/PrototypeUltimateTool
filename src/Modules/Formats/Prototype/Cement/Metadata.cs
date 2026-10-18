using System;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Cement
{
	// Token: 0x02000088 RID: 136
	public class Metadata
	{
		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000595 RID: 1429 RVA: 0x0000C673 File Offset: 0x0000A873
		public int ByteSize
		{
			get
			{
				return 16 + Encoding.ASCII.GetByteCount(this.Name) + 1 + 3;
			}
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0000C68C File Offset: 0x0000A88C
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Date, endian);
			output.WriteValueU32(2048U, endian);
			output.WriteValueU32(0U);
			output.WriteStringU32(this.Name, endian);
			output.Seek(3L, SeekOrigin.Current);
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0000C6C5 File Offset: 0x0000A8C5
		public void Deserialize(Stream input, Endian endian)
		{
			this.Date = input.ReadValueU32(endian);
			this.Alignment = input.ReadValueU32(endian);
			input.ReadValueU32(endian);
			this.Name = input.ReadString(input.ReadValueS32(endian));
			input.ReadBytes(3);
		}

		// Token: 0x04000206 RID: 518
		public uint Date;

		// Token: 0x04000207 RID: 519
		public uint Alignment;

		// Token: 0x04000208 RID: 520
		public string Name;
	}
}
