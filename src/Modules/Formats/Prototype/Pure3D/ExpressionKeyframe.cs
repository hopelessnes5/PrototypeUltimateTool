using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200006D RID: 109
	[KnownType(1184518U)]
	public class ExpressionKeyframe : BaseNode
	{
		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000460 RID: 1120 RVA: 0x0000A932 File Offset: 0x00008B32
		// (set) Token: 0x06000461 RID: 1121 RVA: 0x0000A93A File Offset: 0x00008B3A
		public uint Unknown1 { get; set; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000462 RID: 1122 RVA: 0x0000A943 File Offset: 0x00008B43
		// (set) Token: 0x06000463 RID: 1123 RVA: 0x0000A94B File Offset: 0x00008B4B
		public uint Order { get; set; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000464 RID: 1124 RVA: 0x0000A954 File Offset: 0x00008B54
		// (set) Token: 0x06000465 RID: 1125 RVA: 0x0000A95C File Offset: 0x00008B5C
		public uint Unknown3 { get; set; }

		// Token: 0x06000466 RID: 1126 RVA: 0x0000A965 File Offset: 0x00008B65
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Order, endian);
			output.WriteValueU32(this.Unknown3, endian);
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0000A98E File Offset: 0x00008B8E
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Order = input.ReadValueU32(endian);
			this.Unknown3 = input.ReadValueU32(endian);
		}
	}
}
