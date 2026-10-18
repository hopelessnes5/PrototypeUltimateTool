using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000029 RID: 41
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69658U)]
	public class ShaderPropertyVector4 : BaseNode
	{
		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000141 RID: 321 RVA: 0x00006390 File Offset: 0x00004590
		// (set) Token: 0x06000142 RID: 322 RVA: 0x00006398 File Offset: 0x00004598
		public string Name { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000143 RID: 323 RVA: 0x000063A1 File Offset: 0x000045A1
		// (set) Token: 0x06000144 RID: 324 RVA: 0x000063A9 File Offset: 0x000045A9
		public Vector4 Value { get; set; }

		// Token: 0x06000145 RID: 325 RVA: 0x000063B2 File Offset: 0x000045B2
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x06000146 RID: 326 RVA: 0x000063EE File Offset: 0x000045EE
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			this.Value.Serialize(output, endian);
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00006409 File Offset: 0x00004609
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Value = new Vector4(input, endian);
		}
	}
}
