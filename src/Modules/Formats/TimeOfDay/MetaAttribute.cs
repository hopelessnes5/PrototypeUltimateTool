using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000B RID: 11
	[TypeConverter(typeof(MetaAttributeConverter))]
	public abstract class MetaAttribute
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000030 RID: 48 RVA: 0x000023CF File Offset: 0x000005CF
		// (set) Token: 0x06000031 RID: 49 RVA: 0x000023D7 File Offset: 0x000005D7
		[Category("​Attribute")]
		[DisplayName("Name")]
		public string AttributeName { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000032 RID: 50 RVA: 0x000023E0 File Offset: 0x000005E0
		// (set) Token: 0x06000033 RID: 51 RVA: 0x000023E8 File Offset: 0x000005E8
		[Category("​Attribute")]
		[ReadOnly(true)]
		public string TypeName { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000034 RID: 52 RVA: 0x000023F1 File Offset: 0x000005F1
		// (set) Token: 0x06000035 RID: 53 RVA: 0x000023F9 File Offset: 0x000005F9
		[Browsable(false)]
		public short Flag { get; set; }

		// Token: 0x06000036 RID: 54 RVA: 0x00002402 File Offset: 0x00000602
		public override string ToString()
		{
			return this.AttributeName;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x0000240A File Offset: 0x0000060A
		public virtual void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.AttributeName, endian);
			output.WriteStringU32NoTerminator(this.TypeName, endian);
			output.WriteValueS16(this.Flag, endian);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002433 File Offset: 0x00000633
		public virtual void Deserialize(Stream input, Endian endian)
		{
			this.Flag = input.ReadValueS16(endian);
		}
	}
}
