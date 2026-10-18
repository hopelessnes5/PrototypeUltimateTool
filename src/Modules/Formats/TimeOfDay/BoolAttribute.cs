using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000002 RID: 2
	public class BoolAttribute : MetaAttribute
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00000250
		public bool Value { get; set; }

		// Token: 0x06000003 RID: 3 RVA: 0x00002059 File Offset: 0x00000259
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueB32(this.Value, endian);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002070 File Offset: 0x00000270
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Value = input.ReadValueB32(endian);
		}
	}
}
