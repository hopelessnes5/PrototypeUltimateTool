using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000042 RID: 66
	[KnownType(69643U)]
	public class ShaderProgram : BaseNode
	{
		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000255 RID: 597 RVA: 0x00007C2E File Offset: 0x00005E2E
		// (set) Token: 0x06000256 RID: 598 RVA: 0x00007C36 File Offset: 0x00005E36
		public string Name { get; set; }

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000257 RID: 599 RVA: 0x00007C3F File Offset: 0x00005E3F
		// (set) Token: 0x06000258 RID: 600 RVA: 0x00007C47 File Offset: 0x00005E47
		public uint Unknown02 { get; set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000259 RID: 601 RVA: 0x00007C50 File Offset: 0x00005E50
		// (set) Token: 0x0600025A RID: 602 RVA: 0x00007C58 File Offset: 0x00005E58
		public ShaderProgram.ShaderProgramType ShaderType { get; set; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x0600025B RID: 603 RVA: 0x00007C61 File Offset: 0x00005E61
		// (set) Token: 0x0600025C RID: 604 RVA: 0x00007C69 File Offset: 0x00005E69
		public uint ChildCount2 { get; set; }

		// Token: 0x0600025D RID: 605 RVA: 0x00007C72 File Offset: 0x00005E72
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00007CA3 File Offset: 0x00005EA3
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown02, endian);
			output.WriteValueU32((uint)this.ShaderType, endian);
			output.WriteValueU32(this.ChildCount2, endian);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00007CD8 File Offset: 0x00005ED8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown02 = input.ReadValueU32(endian);
			this.ShaderType = (ShaderProgram.ShaderProgramType)input.ReadValueU32(endian);
			this.ChildCount2 = input.ReadValueU32(endian);
		}

		// Token: 0x0200008E RID: 142
		public enum ShaderProgramType : uint
		{
			// Token: 0x04000211 RID: 529
			VertexShader,
			// Token: 0x04000212 RID: 530
			PixelShader
		}
	}
}
