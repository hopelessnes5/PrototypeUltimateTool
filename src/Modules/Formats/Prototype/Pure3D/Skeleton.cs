using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000044 RID: 68
	[KnownGame(PrototypeGame.Any)]
	[KnownType(143360U)]
	public class Skeleton : BaseNode
	{
		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000285 RID: 645 RVA: 0x00008008 File Offset: 0x00006208
		// (set) Token: 0x06000286 RID: 646 RVA: 0x00008010 File Offset: 0x00006210
		public string Name { get; set; }

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000287 RID: 647 RVA: 0x00008019 File Offset: 0x00006219
		// (set) Token: 0x06000288 RID: 648 RVA: 0x00008021 File Offset: 0x00006221
		public uint Version { get; set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000289 RID: 649 RVA: 0x0000802A File Offset: 0x0000622A
		// (set) Token: 0x0600028A RID: 650 RVA: 0x00008032 File Offset: 0x00006232
		public uint NumJoints { get; set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x0600028B RID: 651 RVA: 0x0000803B File Offset: 0x0000623B
		// (set) Token: 0x0600028C RID: 652 RVA: 0x00008043 File Offset: 0x00006243
		public uint NumPartitions { get; set; }

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600028D RID: 653 RVA: 0x0000804C File Offset: 0x0000624C
		// (set) Token: 0x0600028E RID: 654 RVA: 0x00008054 File Offset: 0x00006254
		public uint NumLimbs { get; set; }

		// Token: 0x0600028F RID: 655 RVA: 0x0000805D File Offset: 0x0000625D
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00008090 File Offset: 0x00006290
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.NumJoints, endian);
			output.WriteValueU32(this.NumPartitions, endian);
			output.WriteValueU32(this.NumLimbs, endian);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x000080E0 File Offset: 0x000062E0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.NumJoints = input.ReadValueU32(endian);
			this.NumPartitions = input.ReadValueU32(endian);
			this.NumLimbs = input.ReadValueU32(endian);
		}
	}
}
