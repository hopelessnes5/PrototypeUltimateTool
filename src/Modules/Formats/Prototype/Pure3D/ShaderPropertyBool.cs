using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000059 RID: 89
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69661U)]
	public class ShaderPropertyBool : BaseNode
	{
		// Token: 0x1700012D RID: 301
		// (get) Token: 0x060003B4 RID: 948 RVA: 0x00009904 File Offset: 0x00007B04
		// (set) Token: 0x060003B5 RID: 949 RVA: 0x0000990C File Offset: 0x00007B0C
		public string Name { get; set; }

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x060003B6 RID: 950 RVA: 0x00009915 File Offset: 0x00007B15
		// (set) Token: 0x060003B7 RID: 951 RVA: 0x0000991D File Offset: 0x00007B1D
		public bool Enabled { get; set; }

		// Token: 0x060003B8 RID: 952 RVA: 0x00009926 File Offset: 0x00007B26
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00009962 File Offset: 0x00007B62
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueB8(this.Enabled);
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0000997C File Offset: 0x00007B7C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Enabled = input.ReadValueB8();
		}
	}
}
