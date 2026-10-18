using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000064 RID: 100
	[KnownType(1191939U)]
	public class CompositeDrawableExpressionMixerReference : BaseNode
	{
		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000414 RID: 1044 RVA: 0x0000A322 File Offset: 0x00008522
		// (set) Token: 0x06000415 RID: 1045 RVA: 0x0000A32A File Offset: 0x0000852A
		public uint Unknown1 { get; set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000416 RID: 1046 RVA: 0x0000A333 File Offset: 0x00008533
		// (set) Token: 0x06000417 RID: 1047 RVA: 0x0000A33B File Offset: 0x0000853B
		public string ExpressionMixerName { get; set; }

		// Token: 0x06000418 RID: 1048 RVA: 0x0000A344 File Offset: 0x00008544
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteStringAlignedU8(this.ExpressionMixerName);
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x0000A35F File Offset: 0x0000855F
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.ExpressionMixerName = input.ReadStringAlignedU8();
		}
	}
}
