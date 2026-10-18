using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000051 RID: 81
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69653U)]
	public class NewShader : BaseNode
	{
		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000355 RID: 853 RVA: 0x00009189 File Offset: 0x00007389
		// (set) Token: 0x06000356 RID: 854 RVA: 0x00009191 File Offset: 0x00007391
		public string Name { get; set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000357 RID: 855 RVA: 0x0000919A File Offset: 0x0000739A
		// (set) Token: 0x06000358 RID: 856 RVA: 0x000091A2 File Offset: 0x000073A2
		public uint Version { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000359 RID: 857 RVA: 0x000091AB File Offset: 0x000073AB
		// (set) Token: 0x0600035A RID: 858 RVA: 0x000091B3 File Offset: 0x000073B3
		public string ShaderName { get; set; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x0600035B RID: 859 RVA: 0x000091BC File Offset: 0x000073BC
		// (set) Token: 0x0600035C RID: 860 RVA: 0x000091C4 File Offset: 0x000073C4
		public uint Unknown4 { get; set; }

		// Token: 0x0600035D RID: 861 RVA: 0x000091CD File Offset: 0x000073CD
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x0600035E RID: 862 RVA: 0x000091FE File Offset: 0x000073FE
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.ShaderName);
			output.WriteValueU32(this.Unknown4, endian);
		}

		// Token: 0x0600035F RID: 863 RVA: 0x00009232 File Offset: 0x00007432
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.ShaderName = input.ReadStringAlignedU8();
			this.Unknown4 = input.ReadValueU32(endian);
		}
	}
}
