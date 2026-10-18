using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000053 RID: 83
	[KnownType(69645U)]
	public class ShaderGlobalVariable : BaseNode
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x0600036D RID: 877 RVA: 0x000093BD File Offset: 0x000075BD
		// (set) Token: 0x0600036E RID: 878 RVA: 0x000093C5 File Offset: 0x000075C5
		public string Name { get; set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x0600036F RID: 879 RVA: 0x000093CE File Offset: 0x000075CE
		// (set) Token: 0x06000370 RID: 880 RVA: 0x000093D6 File Offset: 0x000075D6
		public ShaderVariableType VariableType { get; set; }

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000371 RID: 881 RVA: 0x000093DF File Offset: 0x000075DF
		// (set) Token: 0x06000372 RID: 882 RVA: 0x000093E7 File Offset: 0x000075E7
		public uint Register { get; set; }

		// Token: 0x06000373 RID: 883 RVA: 0x000093F0 File Offset: 0x000075F0
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00009421 File Offset: 0x00007621
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32((uint)this.VariableType, endian);
			output.WriteValueU32(this.Register, endian);
		}

		// Token: 0x06000375 RID: 885 RVA: 0x00009449 File Offset: 0x00007649
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.VariableType = (ShaderVariableType)input.ReadValueU32(endian);
			this.Register = input.ReadValueU32(endian);
		}
	}
}
