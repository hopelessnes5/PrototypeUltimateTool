using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x0200007F RID: 127
	[KnownGame(PrototypeGame.P2)]
	[KnownType(151554U)]
	[KnownType(151555U)]
	public class P2PolySkinMetadata : BaseNode
	{
		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x0600051A RID: 1306 RVA: 0x0000BB0B File Offset: 0x00009D0B
		// (set) Token: 0x0600051B RID: 1307 RVA: 0x0000BB13 File Offset: 0x00009D13
		public uint Unknown1 { get; set; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x0600051C RID: 1308 RVA: 0x0000BB1C File Offset: 0x00009D1C
		// (set) Token: 0x0600051D RID: 1309 RVA: 0x0000BB24 File Offset: 0x00009D24
		public string ShaderName { get; set; }

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x0000BB2D File Offset: 0x00009D2D
		// (set) Token: 0x0600051F RID: 1311 RVA: 0x0000BB35 File Offset: 0x00009D35
		public uint Unknown2 { get; set; }

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000520 RID: 1312 RVA: 0x0000BB3E File Offset: 0x00009D3E
		// (set) Token: 0x06000521 RID: 1313 RVA: 0x0000BB46 File Offset: 0x00009D46
		public string IndicesName { get; set; }

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000522 RID: 1314 RVA: 0x0000BB4F File Offset: 0x00009D4F
		// (set) Token: 0x06000523 RID: 1315 RVA: 0x0000BB57 File Offset: 0x00009D57
		public string VerticesName { get; set; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000524 RID: 1316 RVA: 0x0000BB60 File Offset: 0x00009D60
		// (set) Token: 0x06000525 RID: 1317 RVA: 0x0000BB68 File Offset: 0x00009D68
		public string SkinName { get; set; }

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000526 RID: 1318 RVA: 0x0000BB71 File Offset: 0x00009D71
		// (set) Token: 0x06000527 RID: 1319 RVA: 0x0000BB79 File Offset: 0x00009D79
		public byte Unknown3 { get; set; }

		// Token: 0x06000528 RID: 1320 RVA: 0x0000BB84 File Offset: 0x00009D84
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteStringAlignedU8(this.ShaderName);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteStringAlignedU8(this.IndicesName);
			output.WriteStringAlignedU8(this.VerticesName);
			output.WriteStringAlignedU8(this.SkinName);
			output.WriteByte(this.Unknown3);
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0000BBE8 File Offset: 0x00009DE8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.ShaderName = input.ReadStringAlignedU8();
			this.Unknown2 = input.ReadValueU32(endian);
			this.IndicesName = input.ReadStringAlignedU8();
			this.VerticesName = input.ReadStringAlignedU8();
			this.SkinName = input.ReadStringAlignedU8();
			this.Unknown3 = (byte)input.ReadByte();
		}
	}
}
