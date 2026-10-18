using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000013 RID: 19
	[Tod("engine::StreamPackage", PrototypeGame.P1)]
	public class StreamPackage : MetaObject
	{
		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000AA RID: 170 RVA: 0x00002F29 File Offset: 0x00001129
		// (set) Token: 0x060000AB RID: 171 RVA: 0x00002F31 File Offset: 0x00001131
		[Descriptable]
		public string SlotGroup { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00002F3A File Offset: 0x0000113A
		// (set) Token: 0x060000AD RID: 173 RVA: 0x00002F42 File Offset: 0x00001142
		public List<string> Prerequisite { get; set; } = new List<string>();

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00002F4B File Offset: 0x0000114B
		// (set) Token: 0x060000AF RID: 175 RVA: 0x00002F53 File Offset: 0x00001153
		public List<string> Resources { get; set; } = new List<string>();

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00002F5C File Offset: 0x0000115C
		// (set) Token: 0x060000B1 RID: 177 RVA: 0x00002F64 File Offset: 0x00001164
		public List<string> Tags { get; set; } = new List<string>();

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x00002F6D File Offset: 0x0000116D
		// (set) Token: 0x060000B3 RID: 179 RVA: 0x00002F75 File Offset: 0x00001175
		public uint Unknown1 { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x00002F7E File Offset: 0x0000117E
		// (set) Token: 0x060000B5 RID: 181 RVA: 0x00002F86 File Offset: 0x00001186
		public uint Weight { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x00002F8F File Offset: 0x0000118F
		// (set) Token: 0x060000B7 RID: 183 RVA: 0x00002F97 File Offset: 0x00001197
		public uint XenonMemorySize { get; set; }

		// Token: 0x060000B8 RID: 184 RVA: 0x00002FA0 File Offset: 0x000011A0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.SlotGroup, endian);
			string text = Utils.ListToString(this.Prerequisite);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			text = Utils.ListToString(this.Resources);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			text = Utils.ListToString(this.Tags);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			output.WriteValueU32(this.Unknown1);
			output.WriteValueU32(this.Weight);
			output.WriteValueU32(this.XenonMemorySize);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0000303C File Offset: 0x0000123C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.SlotGroup = input.ReadString(input.ReadValueS32(endian));
			this.Prerequisite = Utils.StringToList(input);
			this.Resources = Utils.StringToList(input);
			this.Tags = Utils.StringToList(input);
			this.Unknown1 = input.ReadValueU32();
			this.Weight = input.ReadValueU32();
			this.XenonMemorySize = input.ReadValueU32();
		}
	}
}
