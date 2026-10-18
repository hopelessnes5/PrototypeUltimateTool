using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000058 RID: 88
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69657U)]
	public class ShaderPropertyColor : BaseNode
	{
		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060003A8 RID: 936 RVA: 0x0000981D File Offset: 0x00007A1D
		// (set) Token: 0x060003A9 RID: 937 RVA: 0x00009825 File Offset: 0x00007A25
		public string Name { get; set; }

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060003AA RID: 938 RVA: 0x0000982E File Offset: 0x00007A2E
		// (set) Token: 0x060003AB RID: 939 RVA: 0x00009836 File Offset: 0x00007A36
		public float R { get; set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060003AC RID: 940 RVA: 0x0000983F File Offset: 0x00007A3F
		// (set) Token: 0x060003AD RID: 941 RVA: 0x00009847 File Offset: 0x00007A47
		public float G { get; set; }

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x060003AE RID: 942 RVA: 0x00009850 File Offset: 0x00007A50
		// (set) Token: 0x060003AF RID: 943 RVA: 0x00009858 File Offset: 0x00007A58
		public float B { get; set; }

		// Token: 0x060003B0 RID: 944 RVA: 0x00009861 File Offset: 0x00007A61
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0000989D File Offset: 0x00007A9D
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueF32(this.R);
			output.WriteValueF32(this.G);
			output.WriteValueF32(this.B);
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x000098CF File Offset: 0x00007ACF
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.R = input.ReadValueF32(endian);
			this.G = input.ReadValueF32(endian);
			this.B = input.ReadValueF32(endian);
		}
	}
}
