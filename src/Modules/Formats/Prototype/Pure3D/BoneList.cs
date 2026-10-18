using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002B RID: 43
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1183746U)]
	public class BoneList : BaseNode
	{
		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600015D RID: 349 RVA: 0x000065C7 File Offset: 0x000047C7
		// (set) Token: 0x0600015E RID: 350 RVA: 0x000065CF File Offset: 0x000047CF
		public uint Version { get; set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600015F RID: 351 RVA: 0x000065D8 File Offset: 0x000047D8
		// (set) Token: 0x06000160 RID: 352 RVA: 0x000065E0 File Offset: 0x000047E0
		public uint NumberOfBones { get; set; }

		// Token: 0x06000161 RID: 353 RVA: 0x000065E9 File Offset: 0x000047E9
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version);
			output.WriteValueU32(this.NumberOfBones);
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00006603 File Offset: 0x00004803
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32();
			this.NumberOfBones = input.ReadValueU32();
		}
	}
}
