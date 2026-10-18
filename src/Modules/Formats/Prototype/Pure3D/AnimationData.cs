using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001D RID: 29
	[KnownGame(PrototypeGame.Any)]
	[KnownType(49283072U)]
	public class AnimationData : BaseNode
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000EF RID: 239 RVA: 0x0000482C File Offset: 0x00002A2C
		// (set) Token: 0x060000F0 RID: 240 RVA: 0x00004834 File Offset: 0x00002A34
		public uint Version { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000F1 RID: 241 RVA: 0x0000483D File Offset: 0x00002A3D
		// (set) Token: 0x060000F2 RID: 242 RVA: 0x00004845 File Offset: 0x00002A45
		public string Compression { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x0000484E File Offset: 0x00002A4E
		// (set) Token: 0x060000F4 RID: 244 RVA: 0x00004856 File Offset: 0x00002A56
		public uint UncompressedSize { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000F5 RID: 245 RVA: 0x0000485F File Offset: 0x00002A5F
		// (set) Token: 0x060000F6 RID: 246 RVA: 0x00004867 File Offset: 0x00002A67
		public uint CompressedSize { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00004870 File Offset: 0x00002A70
		// (set) Token: 0x060000F8 RID: 248 RVA: 0x00004878 File Offset: 0x00002A78
		public byte[] CompressedData { get; set; }

		// Token: 0x060000F9 RID: 249 RVA: 0x00004881 File Offset: 0x00002A81
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version);
			output.WriteString(this.Compression);
			output.WriteValueU32(this.UncompressedSize);
			output.WriteValueU32(this.CompressedSize);
			output.WriteBytes(this.CompressedData);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000048C0 File Offset: 0x00002AC0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Compression = input.ReadString(4);
			this.UncompressedSize = input.ReadValueU32(endian);
			this.CompressedSize = input.ReadValueU32(endian);
			this.CompressedData = input.ReadBytes((int)this.CompressedSize);
		}
	}
}
