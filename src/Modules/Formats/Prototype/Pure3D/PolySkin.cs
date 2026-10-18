using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000041 RID: 65
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65537U)]
	public class PolySkin : BaseNode
	{
		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000249 RID: 585 RVA: 0x00007B46 File Offset: 0x00005D46
		// (set) Token: 0x0600024A RID: 586 RVA: 0x00007B4E File Offset: 0x00005D4E
		public string Name { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600024B RID: 587 RVA: 0x00007B57 File Offset: 0x00005D57
		// (set) Token: 0x0600024C RID: 588 RVA: 0x00007B5F File Offset: 0x00005D5F
		public uint Version { get; set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600024D RID: 589 RVA: 0x00007B68 File Offset: 0x00005D68
		// (set) Token: 0x0600024E RID: 590 RVA: 0x00007B70 File Offset: 0x00005D70
		public string SkeletonName { get; set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x0600024F RID: 591 RVA: 0x00007B79 File Offset: 0x00005D79
		// (set) Token: 0x06000250 RID: 592 RVA: 0x00007B81 File Offset: 0x00005D81
		public uint PrimitiveGroup { get; set; }

		// Token: 0x06000251 RID: 593 RVA: 0x00007B8A File Offset: 0x00005D8A
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00007BC6 File Offset: 0x00005DC6
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.SkeletonName);
			output.WriteValueU32(this.PrimitiveGroup, endian);
		}

		// Token: 0x06000253 RID: 595 RVA: 0x00007BFA File Offset: 0x00005DFA
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.SkeletonName = input.ReadStringAlignedU8();
			this.PrimitiveGroup = input.ReadValueU32(endian);
		}
	}
}
