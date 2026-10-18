using System;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000006 RID: 6
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class Colour
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002A00 File Offset: 0x00000C00
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002A08 File Offset: 0x00000C08
		[DataMember(Name = "r", Order = 1)]
		public float R { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002A11 File Offset: 0x00000C11
		// (set) Token: 0x0600001A RID: 26 RVA: 0x00002A19 File Offset: 0x00000C19
		[DataMember(Name = "g", Order = 2)]
		public float G { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001B RID: 27 RVA: 0x00002A22 File Offset: 0x00000C22
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002A2A File Offset: 0x00000C2A
		[DataMember(Name = "b", Order = 3)]
		public float B { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002A33 File Offset: 0x00000C33
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002A3B File Offset: 0x00000C3B
		[DataMember(Name = "a", Order = 4)]
		public float A { get; set; }

		// Token: 0x0600001F RID: 31 RVA: 0x00002A44 File Offset: 0x00000C44
		public void Serialize(Stream output)
		{
			output.WriteValueF32(this.R);
			output.WriteValueF32(this.G);
			output.WriteValueF32(this.B);
			output.WriteValueF32(this.A);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002A76 File Offset: 0x00000C76
		public void Deserialize(Stream input)
		{
			this.R = input.ReadValueF32();
			this.G = input.ReadValueF32();
			this.B = input.ReadValueF32();
			this.A = input.ReadValueF32();
		}
	}
}
