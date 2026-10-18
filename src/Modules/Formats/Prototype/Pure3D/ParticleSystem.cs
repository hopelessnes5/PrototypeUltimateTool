using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000036 RID: 54
	[KnownType(88192U)]
	public class ParticleSystem : BaseNode
	{
		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x00007154 File Offset: 0x00005354
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x0000715C File Offset: 0x0000535C
		public uint Version { get; set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x00007165 File Offset: 0x00005365
		// (set) Token: 0x060001D9 RID: 473 RVA: 0x0000716D File Offset: 0x0000536D
		public string Name { get; set; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060001DA RID: 474 RVA: 0x00007176 File Offset: 0x00005376
		// (set) Token: 0x060001DB RID: 475 RVA: 0x0000717E File Offset: 0x0000537E
		public uint Unknown3 { get; set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060001DC RID: 476 RVA: 0x00007187 File Offset: 0x00005387
		// (set) Token: 0x060001DD RID: 477 RVA: 0x0000718F File Offset: 0x0000538F
		public uint Unknown4 { get; set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060001DE RID: 478 RVA: 0x00007198 File Offset: 0x00005398
		// (set) Token: 0x060001DF RID: 479 RVA: 0x000071A0 File Offset: 0x000053A0
		public uint Unknown5 { get; set; }

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x000071A9 File Offset: 0x000053A9
		// (set) Token: 0x060001E1 RID: 481 RVA: 0x000071B1 File Offset: 0x000053B1
		public Vector4 Rotation { get; set; }

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060001E2 RID: 482 RVA: 0x000071BA File Offset: 0x000053BA
		// (set) Token: 0x060001E3 RID: 483 RVA: 0x000071C2 File Offset: 0x000053C2
		public Vector3 Translation { get; set; }

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x000071CB File Offset: 0x000053CB
		// (set) Token: 0x060001E5 RID: 485 RVA: 0x000071D3 File Offset: 0x000053D3
		public uint NumEmitters { get; set; }

		// Token: 0x060001E6 RID: 486 RVA: 0x000071DC File Offset: 0x000053DC
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00007210 File Offset: 0x00005410
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown3, endian);
			output.WriteValueU32(this.Unknown4, endian);
			output.WriteValueU32(this.Unknown5, endian);
			this.Rotation.Serialize(output, endian);
			this.Translation.Serialize(output, endian);
			output.WriteValueU32(this.NumEmitters, endian);
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00007284 File Offset: 0x00005484
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.Unknown3 = input.ReadValueU32(endian);
			this.Unknown4 = input.ReadValueU32(endian);
			this.Unknown5 = input.ReadValueU32(endian);
			this.Rotation = new Vector4(input, endian);
			this.Translation = new Vector3(input, endian);
			this.NumEmitters = input.ReadValueU32(endian);
		}
	}
}
