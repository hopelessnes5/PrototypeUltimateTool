using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype2
{
	// Token: 0x02000010 RID: 16
	[Tod("engine::StreamPackage", PrototypeGame.P2)]
	public class StreamPackage : MetaObject
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000027A2 File Offset: 0x000009A2
		// (set) Token: 0x06000050 RID: 80 RVA: 0x000027AA File Offset: 0x000009AA
		public string SlotGroup { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000027B3 File Offset: 0x000009B3
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000027BB File Offset: 0x000009BB
		public List<string> Dependencies { get; set; } = new List<string>();

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000053 RID: 83 RVA: 0x000027C4 File Offset: 0x000009C4
		// (set) Token: 0x06000054 RID: 84 RVA: 0x000027CC File Offset: 0x000009CC
		public List<string> Packages { get; set; } = new List<string>();

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000055 RID: 85 RVA: 0x000027D5 File Offset: 0x000009D5
		// (set) Token: 0x06000056 RID: 86 RVA: 0x000027DD File Offset: 0x000009DD
		public List<string> Tags { get; set; } = new List<string>();

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000027E6 File Offset: 0x000009E6
		// (set) Token: 0x06000058 RID: 88 RVA: 0x000027EE File Offset: 0x000009EE
		public string UnknownString { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000059 RID: 89 RVA: 0x000027F7 File Offset: 0x000009F7
		// (set) Token: 0x0600005A RID: 90 RVA: 0x000027FF File Offset: 0x000009FF
		public uint Unknown1 { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00002808 File Offset: 0x00000A08
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00002810 File Offset: 0x00000A10
		public uint Unknown2 { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00002819 File Offset: 0x00000A19
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00002821 File Offset: 0x00000A21
		public uint XenonMemorySize { get; set; }

		// Token: 0x0600005F RID: 95 RVA: 0x0000282C File Offset: 0x00000A2C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.SlotGroup, endian);
			string text = Utils.ListToString(this.Dependencies);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			text = Utils.ListToString(this.Packages);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			text = Utils.ListToString(this.Tags);
			output.WriteValueS32(text.Length);
			output.WriteString(text);
			output.WriteStringU32NoTerminator(this.UnknownString, endian);
			output.WriteValueU32(this.Unknown1);
			output.WriteValueU32(this.Unknown2);
			output.WriteValueU32(this.XenonMemorySize);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000028D4 File Offset: 0x00000AD4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.SlotGroup = input.ReadString(input.ReadValueS32(endian));
			this.Dependencies = Utils.StringToList(input);
			this.Packages = Utils.StringToList(input);
			this.Tags = Utils.StringToList(input);
			this.UnknownString = input.ReadString(input.ReadValueS32(endian));
			this.Unknown1 = input.ReadValueU32();
			this.Unknown2 = input.ReadValueU32();
			this.XenonMemorySize = input.ReadValueU32();
		}
	}
}
