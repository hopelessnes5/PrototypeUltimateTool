using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005E RID: 94
	[KnownType(1183750U)]
	public class AnimationHeader : BaseNode
	{
		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060003E4 RID: 996 RVA: 0x00009CE2 File Offset: 0x00007EE2
		// (set) Token: 0x060003E5 RID: 997 RVA: 0x00009CEA File Offset: 0x00007EEA
		public uint Unknown1 { get; set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x00009CF3 File Offset: 0x00007EF3
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x00009CFB File Offset: 0x00007EFB
		public uint NumberOfAnimationBones { get; set; }

		// Token: 0x060003E8 RID: 1000 RVA: 0x00009D04 File Offset: 0x00007F04
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.NumberOfAnimationBones, endian);
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00009D20 File Offset: 0x00007F20
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.NumberOfAnimationBones = input.ReadValueU32(endian);
		}
	}
}
