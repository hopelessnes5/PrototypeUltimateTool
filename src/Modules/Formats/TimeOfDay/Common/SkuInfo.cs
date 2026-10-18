using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod.Common
{
	// Token: 0x0200001C RID: 28
	[Tod("engine::SkuInfo")]
	public class SkuInfo : MetaObject
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600011D RID: 285 RVA: 0x00003AFC File Offset: 0x00001CFC
		// (set) Token: 0x0600011E RID: 286 RVA: 0x00003B04 File Offset: 0x00001D04
		public string SkuName { get; set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00003B0D File Offset: 0x00001D0D
		// (set) Token: 0x06000120 RID: 288 RVA: 0x00003B15 File Offset: 0x00001D15
		public string SkuVersion { get; set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00003B1E File Offset: 0x00001D1E
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00003B26 File Offset: 0x00001D26
		public List<string> Languages { get; set; } = new List<string>();

		// Token: 0x06000123 RID: 291 RVA: 0x00003B30 File Offset: 0x00001D30
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32(this.Languages.Count, endian);
			for (int i = 0; i < this.Languages.Count; i++)
			{
				output.WriteStringU32NoTerminator(this.Languages[i], endian);
			}
			output.WriteStringU32NoTerminator(this.SkuName, endian);
			output.WriteStringU32NoTerminator(this.SkuVersion, endian);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00003B94 File Offset: 0x00001D94
		public override void Deserialize(Stream input, Endian endian)
		{
			int num = input.ReadValueS32(endian);
			for (int i = 0; i < num; i++)
			{
				this.Languages.Add(input.ReadString(input.ReadValueS32(endian)));
			}
			this.SkuName = input.ReadString(input.ReadValueS32(endian));
			this.SkuVersion = input.ReadString(input.ReadValueS32(endian));
		}
	}
}
