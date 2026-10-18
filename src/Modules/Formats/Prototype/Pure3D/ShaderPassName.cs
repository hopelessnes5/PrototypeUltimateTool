using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005A RID: 90
	[KnownType(69667U)]
	public class ShaderPassName : BaseNode
	{
		// Token: 0x1700012F RID: 303
		// (get) Token: 0x060003BC RID: 956 RVA: 0x00009996 File Offset: 0x00007B96
		// (set) Token: 0x060003BD RID: 957 RVA: 0x0000999E File Offset: 0x00007B9E
		public string Name { get; set; }

		// Token: 0x060003BE RID: 958 RVA: 0x000099A7 File Offset: 0x00007BA7
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x060003BF RID: 959 RVA: 0x000099E3 File Offset: 0x00007BE3
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x000099F1 File Offset: 0x00007BF1
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
		}
	}
}
