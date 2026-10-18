using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000067 RID: 103
	[KnownType(135170U)]
	public class ExpressionMixer : BaseNode
	{
		// Token: 0x17000153 RID: 339
		// (get) Token: 0x0600042D RID: 1069 RVA: 0x0000A499 File Offset: 0x00008699
		// (set) Token: 0x0600042E RID: 1070 RVA: 0x0000A4A1 File Offset: 0x000086A1
		public uint Unknown1 { get; set; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x0600042F RID: 1071 RVA: 0x0000A4AA File Offset: 0x000086AA
		// (set) Token: 0x06000430 RID: 1072 RVA: 0x0000A4B2 File Offset: 0x000086B2
		public string Name { get; set; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000431 RID: 1073 RVA: 0x0000A4BB File Offset: 0x000086BB
		// (set) Token: 0x06000432 RID: 1074 RVA: 0x0000A4C3 File Offset: 0x000086C3
		public uint Unknown3 { get; set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000433 RID: 1075 RVA: 0x0000A4CC File Offset: 0x000086CC
		// (set) Token: 0x06000434 RID: 1076 RVA: 0x0000A4D4 File Offset: 0x000086D4
		public string CompositeDrawableName { get; set; }

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x0000A4DD File Offset: 0x000086DD
		// (set) Token: 0x06000436 RID: 1078 RVA: 0x0000A4E5 File Offset: 0x000086E5
		public string ExpressionGroupName { get; set; }

		// Token: 0x06000437 RID: 1079 RVA: 0x0000A4EE File Offset: 0x000086EE
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x0000A51F File Offset: 0x0000871F
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown3, endian);
			output.WriteStringAlignedU8(this.CompositeDrawableName);
			output.WriteStringAlignedU8(this.ExpressionGroupName);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0000A55F File Offset: 0x0000875F
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.Unknown3 = input.ReadValueU32(endian);
			this.CompositeDrawableName = input.ReadStringAlignedU8();
			this.ExpressionGroupName = input.ReadStringAlignedU8();
		}
	}
}
