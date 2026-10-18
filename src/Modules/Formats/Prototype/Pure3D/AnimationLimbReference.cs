using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001E RID: 30
	[KnownType(1184769U)]
	public class AnimationLimbReference : BaseNode
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000FC RID: 252 RVA: 0x00004913 File Offset: 0x00002B13
		// (set) Token: 0x060000FD RID: 253 RVA: 0x0000491B File Offset: 0x00002B1B
		public string Limb { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00004924 File Offset: 0x00002B24
		// (set) Token: 0x060000FF RID: 255 RVA: 0x0000492C File Offset: 0x00002B2C
		public byte[] Unknown { get; set; }

		// Token: 0x06000100 RID: 256 RVA: 0x00004935 File Offset: 0x00002B35
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Limb))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Limb + ")";
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00004966 File Offset: 0x00002B66
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Limb);
			output.WriteBytes(this.Unknown);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00004980 File Offset: 0x00002B80
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Limb = input.ReadStringAlignedU8();
			this.Unknown = input.ReadBytes(24);
		}
	}
}
