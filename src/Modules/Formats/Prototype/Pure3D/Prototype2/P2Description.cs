using System;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x02000081 RID: 129
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class P2Description
	{
		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000535 RID: 1333 RVA: 0x0000BD04 File Offset: 0x00009F04
		// (set) Token: 0x06000536 RID: 1334 RVA: 0x0000BD0C File Offset: 0x00009F0C
		public DescriptionType BufferType { get; set; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0000BD15 File Offset: 0x00009F15
		// (set) Token: 0x06000538 RID: 1336 RVA: 0x0000BD1D File Offset: 0x00009F1D
		public uint Unknown1 { get; set; }

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000539 RID: 1337 RVA: 0x0000BD26 File Offset: 0x00009F26
		// (set) Token: 0x0600053A RID: 1338 RVA: 0x0000BD2E File Offset: 0x00009F2E
		public uint Unknown2 { get; set; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x0600053B RID: 1339 RVA: 0x0000BD37 File Offset: 0x00009F37
		// (set) Token: 0x0600053C RID: 1340 RVA: 0x0000BD3F File Offset: 0x00009F3F
		public uint Offset { get; set; }

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x0600053D RID: 1341 RVA: 0x0000BD48 File Offset: 0x00009F48
		// (set) Token: 0x0600053E RID: 1342 RVA: 0x0000BD50 File Offset: 0x00009F50
		public uint ItemSize { get; set; }

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x0600053F RID: 1343 RVA: 0x0000BD59 File Offset: 0x00009F59
		// (set) Token: 0x06000540 RID: 1344 RVA: 0x0000BD61 File Offset: 0x00009F61
		public uint BufferSize { get; set; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000541 RID: 1345 RVA: 0x0000BD6A File Offset: 0x00009F6A
		// (set) Token: 0x06000542 RID: 1346 RVA: 0x0000BD72 File Offset: 0x00009F72
		public uint Unknown5 { get; set; }

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000543 RID: 1347 RVA: 0x0000BD7B File Offset: 0x00009F7B
		// (set) Token: 0x06000544 RID: 1348 RVA: 0x0000BD83 File Offset: 0x00009F83
		public uint Unknown6 { get; set; }

		// Token: 0x06000545 RID: 1349 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public P2Description()
		{
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x0000BD8C File Offset: 0x00009F8C
		public P2Description(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x0000BD9C File Offset: 0x00009F9C
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.BufferType.ToP2Type());
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteValueU32(this.Offset, endian);
			output.WriteValueU32(this.ItemSize, endian);
			output.WriteValueU32(this.BufferSize, endian);
			output.WriteValueU32(this.Unknown5, endian);
			output.WriteValueU32(this.Unknown6, endian);
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0000BE18 File Offset: 0x0000A018
		public void Deserialize(Stream input, Endian endian)
		{
			this.BufferType = new DescriptionType(input.ReadStringAlignedU8());
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			this.Offset = input.ReadValueU32(endian);
			this.ItemSize = input.ReadValueU32(endian);
			this.BufferSize = input.ReadValueU32(endian);
			this.Unknown5 = input.ReadValueU32(endian);
			this.Unknown6 = input.ReadValueU32(endian);
		}
	}
}
