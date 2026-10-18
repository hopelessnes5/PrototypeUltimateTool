using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000007 RID: 7
	public class NameAttribute : MetaAttribute
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002208 File Offset: 0x00000408
		// (set) Token: 0x0600001F RID: 31 RVA: 0x00002210 File Offset: 0x00000410
		public string Value { get; set; }

		// Token: 0x06000020 RID: 32 RVA: 0x00002219 File Offset: 0x00000419
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteStringU32NoTerminator(this.Value, endian);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002230 File Offset: 0x00000430
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Value = input.ReadString(input.ReadValueS32(endian), false);
		}
	}
}
