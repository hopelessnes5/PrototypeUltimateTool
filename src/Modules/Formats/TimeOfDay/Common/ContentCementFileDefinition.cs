using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod.Common
{
	// Token: 0x0200001A RID: 26
	[Tod("ContentCementFileDefinition")]
	public class ContentCementFileDefinition : MetaObject
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00003815 File Offset: 0x00001A15
		// (set) Token: 0x06000111 RID: 273 RVA: 0x0000381D File Offset: 0x00001A1D
		public List<string> DefaultFiles { get; set; } = new List<string>();

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00003826 File Offset: 0x00001A26
		// (set) Token: 0x06000113 RID: 275 RVA: 0x0000382E File Offset: 0x00001A2E
		public List<string> CachedFiles { get; set; } = new List<string>();

		// Token: 0x06000114 RID: 276 RVA: 0x00003838 File Offset: 0x00001A38
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32(2, endian);
			output.WriteStringU32NoTerminator("default", endian);
			output.WriteValueS32(this.DefaultFiles.Count, endian);
			for (int i = 0; i < this.DefaultFiles.Count; i++)
			{
				output.WriteStringU32NoTerminator(this.DefaultFiles[i], endian);
			}
			output.WriteStringU32NoTerminator("cached", endian);
			output.WriteValueS32(this.CachedFiles.Count, endian);
			for (int j = 0; j < this.CachedFiles.Count; j++)
			{
				output.WriteStringU32NoTerminator(this.CachedFiles[j], endian);
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000038DC File Offset: 0x00001ADC
		public override void Deserialize(Stream input, Endian endian)
		{
			if (input.ReadValueS32(endian) != 2)
			{
				throw new Exception("that was unexpected");
			}
			if (input.ReadString(input.ReadValueS32(endian)) != "default")
			{
				throw new Exception("that was unexpected");
			}
			int num = input.ReadValueS32(endian);
			for (int i = 0; i < num; i++)
			{
				this.DefaultFiles.Add(input.ReadString(input.ReadValueS32(endian)));
			}
			if (input.ReadString(input.ReadValueS32(endian)) != "cached")
			{
				throw new Exception("that was unexpected");
			}
			num = input.ReadValueS32(endian);
			for (int j = 0; j < num; j++)
			{
				this.CachedFiles.Add(input.ReadString(input.ReadValueS32(endian)));
			}
		}
	}
}
