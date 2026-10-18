using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002E RID: 46
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1191936U)]
	[KnownType(151552U)]
	public class CompositeDrawable : BaseNode
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600017E RID: 382 RVA: 0x000067EB File Offset: 0x000049EB
		// (set) Token: 0x0600017F RID: 383 RVA: 0x000067F3 File Offset: 0x000049F3
		public uint Version { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000180 RID: 384 RVA: 0x000067FC File Offset: 0x000049FC
		// (set) Token: 0x06000181 RID: 385 RVA: 0x00006804 File Offset: 0x00004A04
		public string Name { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000182 RID: 386 RVA: 0x0000680D File Offset: 0x00004A0D
		// (set) Token: 0x06000183 RID: 387 RVA: 0x00006815 File Offset: 0x00004A15
		public string SkeletonName { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000184 RID: 388 RVA: 0x0000681E File Offset: 0x00004A1E
		// (set) Token: 0x06000185 RID: 389 RVA: 0x00006826 File Offset: 0x00004A26
		public uint NumPrimitives { get; set; }

		// Token: 0x06000186 RID: 390 RVA: 0x0000682F File Offset: 0x00004A2F
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000686B File Offset: 0x00004A6B
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.SkeletonName);
			output.WriteValueU32(this.NumPrimitives, endian);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000689F File Offset: 0x00004A9F
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.SkeletonName = input.ReadStringAlignedU8();
			this.NumPrimitives = input.ReadValueU32(endian);
		}
	}
}
