using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000040 RID: 64
	[KnownGame(PrototypeGame.P1)]
	[KnownType(117571584U)]
	public class Physics : BaseNode
	{
		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x0600023D RID: 573 RVA: 0x00007A5C File Offset: 0x00005C5C
		// (set) Token: 0x0600023E RID: 574 RVA: 0x00007A64 File Offset: 0x00005C64
		public string Name { get; set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x0600023F RID: 575 RVA: 0x00007A6D File Offset: 0x00005C6D
		// (set) Token: 0x06000240 RID: 576 RVA: 0x00007A75 File Offset: 0x00005C75
		public uint Unknown2 { get; set; }

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000241 RID: 577 RVA: 0x00007A7E File Offset: 0x00005C7E
		// (set) Token: 0x06000242 RID: 578 RVA: 0x00007A86 File Offset: 0x00005C86
		public uint Unknown3 { get; set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000243 RID: 579 RVA: 0x00007A8F File Offset: 0x00005C8F
		// (set) Token: 0x06000244 RID: 580 RVA: 0x00007A97 File Offset: 0x00005C97
		public uint Unknown4 { get; set; }

		// Token: 0x06000245 RID: 581 RVA: 0x00007AA0 File Offset: 0x00005CA0
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x06000246 RID: 582 RVA: 0x00007ADC File Offset: 0x00005CDC
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteValueU32(this.Unknown3, endian);
			output.WriteValueU32(this.Unknown4, endian);
		}

		// Token: 0x06000247 RID: 583 RVA: 0x00007B11 File Offset: 0x00005D11
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown2 = input.ReadValueU32(endian);
			this.Unknown3 = input.ReadValueU32(endian);
			this.Unknown4 = input.ReadValueU32(endian);
		}
	}
}
