using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000005 RID: 5
	public class IntAttribute : MetaAttribute
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002124 File Offset: 0x00000324
		// (set) Token: 0x06000011 RID: 17 RVA: 0x0000212C File Offset: 0x0000032C
		public int Value { get; set; }

		// Token: 0x06000012 RID: 18 RVA: 0x00002135 File Offset: 0x00000335
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueS32(this.Value, endian);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000214C File Offset: 0x0000034C
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Value = input.ReadValueS32(endian);
		}
	}
}
