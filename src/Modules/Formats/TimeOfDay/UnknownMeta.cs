using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000F RID: 15
	public class UnknownMeta : MetaObject
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002750 File Offset: 0x00000950
		// (set) Token: 0x0600004A RID: 74 RVA: 0x00002758 File Offset: 0x00000958
		[Browsable(false)]
		public uint MetaLength { get; set; }

		// Token: 0x0600004B RID: 75 RVA: 0x00002761 File Offset: 0x00000961
		public UnknownMeta()
		{
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002769 File Offset: 0x00000969
		public UnknownMeta(Stream input, Endian endian, uint length)
		{
			this.MetaLength = length;
			this.Deserialize(input, endian);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002780 File Offset: 0x00000980
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Data);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x0000278E File Offset: 0x0000098E
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Data = input.ReadBytes((int)this.MetaLength);
		}

		// Token: 0x04000016 RID: 22
		public byte[] Data;
	}
}
