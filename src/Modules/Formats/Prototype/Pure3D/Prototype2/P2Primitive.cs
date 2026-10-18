using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x0200007E RID: 126
	[KnownGame(PrototypeGame.P2)]
	[KnownType(65601U)]
	[KnownType(65600U)]
	public class P2Primitive : BaseNode
	{
		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000508 RID: 1288 RVA: 0x0000B993 File Offset: 0x00009B93
		// (set) Token: 0x06000509 RID: 1289 RVA: 0x0000B99B File Offset: 0x00009B9B
		public uint Version { get; set; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x0600050A RID: 1290 RVA: 0x0000B9A4 File Offset: 0x00009BA4
		// (set) Token: 0x0600050B RID: 1291 RVA: 0x0000B9AC File Offset: 0x00009BAC
		public string Name { get; set; }

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x0600050C RID: 1292 RVA: 0x0000B9B5 File Offset: 0x00009BB5
		// (set) Token: 0x0600050D RID: 1293 RVA: 0x0000B9BD File Offset: 0x00009BBD
		public uint Unknown1 { get; set; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x0000B9C6 File Offset: 0x00009BC6
		// (set) Token: 0x0600050F RID: 1295 RVA: 0x0000B9CE File Offset: 0x00009BCE
		public uint Unknown2 { get; set; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x0000B9D7 File Offset: 0x00009BD7
		// (set) Token: 0x06000511 RID: 1297 RVA: 0x0000B9DF File Offset: 0x00009BDF
		public uint NumberOfVertices { get; set; }

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000512 RID: 1298 RVA: 0x0000B9E8 File Offset: 0x00009BE8
		// (set) Token: 0x06000513 RID: 1299 RVA: 0x0000B9F0 File Offset: 0x00009BF0
		public uint Unknown3 { get; set; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x0000B9F9 File Offset: 0x00009BF9
		// (set) Token: 0x06000515 RID: 1301 RVA: 0x0000BA01 File Offset: 0x00009C01
		public uint Unknown4 { get; set; }

		// Token: 0x06000516 RID: 1302 RVA: 0x0000BA0A File Offset: 0x00009C0A
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0000BA3C File Offset: 0x00009C3C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteValueU32(this.NumberOfVertices, endian);
			output.WriteValueU32(this.Unknown3, endian);
			output.WriteValueU32(this.Unknown4, endian);
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0000BAA4 File Offset: 0x00009CA4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			this.NumberOfVertices = input.ReadValueU32(endian);
			this.Unknown3 = input.ReadValueU32(endian);
			this.Unknown4 = input.ReadValueU32(endian);
		}
	}
}
