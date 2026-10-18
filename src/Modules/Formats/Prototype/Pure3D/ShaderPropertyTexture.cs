using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000055 RID: 85
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69654U)]
	public class ShaderPropertyTexture : BaseNode
	{
		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000390 RID: 912 RVA: 0x00009663 File Offset: 0x00007863
		// (set) Token: 0x06000391 RID: 913 RVA: 0x0000966B File Offset: 0x0000786B
		public string Name { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000392 RID: 914 RVA: 0x00009674 File Offset: 0x00007874
		// (set) Token: 0x06000393 RID: 915 RVA: 0x0000967C File Offset: 0x0000787C
		public string Value { get; set; }

		// Token: 0x06000394 RID: 916 RVA: 0x00009685 File Offset: 0x00007885
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000096C1 File Offset: 0x000078C1
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.Value);
		}

		// Token: 0x06000396 RID: 918 RVA: 0x000096DB File Offset: 0x000078DB
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Value = input.ReadStringAlignedU8();
		}
	}
}
