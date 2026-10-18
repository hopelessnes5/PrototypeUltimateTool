using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004D RID: 77
	[KnownType(139264U)]
	public class TextureFont : BaseNode
	{
		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x0600030C RID: 780 RVA: 0x00008B26 File Offset: 0x00006D26
		// (set) Token: 0x0600030D RID: 781 RVA: 0x00008B2E File Offset: 0x00006D2E
		public uint Version { get; set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x0600030E RID: 782 RVA: 0x00008B37 File Offset: 0x00006D37
		// (set) Token: 0x0600030F RID: 783 RVA: 0x00008B3F File Offset: 0x00006D3F
		public string Name { get; set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000310 RID: 784 RVA: 0x00008B48 File Offset: 0x00006D48
		// (set) Token: 0x06000311 RID: 785 RVA: 0x00008B50 File Offset: 0x00006D50
		public string ShaderName { get; set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000312 RID: 786 RVA: 0x00008B59 File Offset: 0x00006D59
		// (set) Token: 0x06000313 RID: 787 RVA: 0x00008B61 File Offset: 0x00006D61
		public float FontSize { get; set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000314 RID: 788 RVA: 0x00008B6A File Offset: 0x00006D6A
		// (set) Token: 0x06000315 RID: 789 RVA: 0x00008B72 File Offset: 0x00006D72
		public float FontWidth { get; set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000316 RID: 790 RVA: 0x00008B7B File Offset: 0x00006D7B
		// (set) Token: 0x06000317 RID: 791 RVA: 0x00008B83 File Offset: 0x00006D83
		public float FontHeight { get; set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000318 RID: 792 RVA: 0x00008B8C File Offset: 0x00006D8C
		// (set) Token: 0x06000319 RID: 793 RVA: 0x00008B94 File Offset: 0x00006D94
		public float FontBaseLine { get; set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x0600031A RID: 794 RVA: 0x00008B9D File Offset: 0x00006D9D
		// (set) Token: 0x0600031B RID: 795 RVA: 0x00008BA5 File Offset: 0x00006DA5
		public uint NumTextures { get; set; }

		// Token: 0x0600031C RID: 796 RVA: 0x00008BAE File Offset: 0x00006DAE
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x0600031D RID: 797 RVA: 0x00008BE0 File Offset: 0x00006DE0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.ShaderName);
			output.WriteValueF32(this.FontSize, endian);
			output.WriteValueF32(this.FontWidth, endian);
			output.WriteValueF32(this.FontHeight, endian);
			output.WriteValueF32(this.FontBaseLine, endian);
			output.WriteValueU32(this.NumTextures, endian);
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00008C54 File Offset: 0x00006E54
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.ShaderName = input.ReadStringAlignedU8();
			this.FontSize = input.ReadValueF32(endian);
			this.FontWidth = input.ReadValueF32(endian);
			this.FontHeight = input.ReadValueF32(endian);
			this.FontBaseLine = input.ReadValueF32(endian);
			this.NumTextures = input.ReadValueU32(endian);
		}
	}
}
