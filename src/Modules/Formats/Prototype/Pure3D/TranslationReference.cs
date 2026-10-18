using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000028 RID: 40
	[KnownType(1183753U)]
	public class TranslationReference : BaseNode
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000137 RID: 311 RVA: 0x000062AB File Offset: 0x000044AB
		// (set) Token: 0x06000138 RID: 312 RVA: 0x000062B3 File Offset: 0x000044B3
		public uint Pad { get; set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000139 RID: 313 RVA: 0x000062BC File Offset: 0x000044BC
		// (set) Token: 0x0600013A RID: 314 RVA: 0x000062C4 File Offset: 0x000044C4
		public string Reference { get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600013B RID: 315 RVA: 0x000062CD File Offset: 0x000044CD
		// (set) Token: 0x0600013C RID: 316 RVA: 0x000062D5 File Offset: 0x000044D5
		public uint Count { get; set; }

		// Token: 0x0600013D RID: 317 RVA: 0x000062DE File Offset: 0x000044DE
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Reference))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Reference + ")";
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00006310 File Offset: 0x00004510
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Pad);
			uint value = Convert.ToUInt32(this.Reference, 16);
			output.WriteValueU32(value);
			output.WriteValueU32(this.Count);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0000634C File Offset: 0x0000454C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Pad = input.ReadValueU32(endian);
			uint num = input.ReadValueU32(endian);
			this.Reference = string.Format("0x{0:X}", num);
			this.Count = input.ReadValueU32();
		}
	}
}
