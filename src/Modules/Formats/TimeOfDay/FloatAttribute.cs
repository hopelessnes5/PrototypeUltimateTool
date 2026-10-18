using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000004 RID: 4
	public class FloatAttribute : MetaAttribute
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000020DD File Offset: 0x000002DD
		// (set) Token: 0x0600000C RID: 12 RVA: 0x000020E5 File Offset: 0x000002E5
		public float Value { get; set; }

		// Token: 0x0600000D RID: 13 RVA: 0x000020EE File Offset: 0x000002EE
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueF32(this.Value, endian);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002105 File Offset: 0x00000305
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Value = input.ReadValueF32(endian);
		}
	}
}
