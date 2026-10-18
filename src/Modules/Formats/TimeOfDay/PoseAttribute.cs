using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000008 RID: 8
	public class PoseAttribute : MetaAttribute
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002256 File Offset: 0x00000456
		// (set) Token: 0x06000024 RID: 36 RVA: 0x0000225E File Offset: 0x0000045E
		public bool Unknown { get; set; }

		// Token: 0x06000025 RID: 37 RVA: 0x00002267 File Offset: 0x00000467
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueB8(this.Unknown);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x0000227D File Offset: 0x0000047D
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Unknown = input.ReadValueB8();
		}
	}
}
