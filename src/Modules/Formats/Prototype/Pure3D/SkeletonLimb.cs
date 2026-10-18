using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200003E RID: 62
	[KnownGame(PrototypeGame.Any)]
	[KnownType(143363U)]
	public class SkeletonLimb : BaseNode
	{
		// Token: 0x17000096 RID: 150
		// (get) Token: 0x0600021E RID: 542 RVA: 0x00007733 File Offset: 0x00005933
		// (set) Token: 0x0600021F RID: 543 RVA: 0x0000773B File Offset: 0x0000593B
		public string Name { get; set; }

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000220 RID: 544 RVA: 0x00007744 File Offset: 0x00005944
		// (set) Token: 0x06000221 RID: 545 RVA: 0x0000774C File Offset: 0x0000594C
		public uint RootBone { get; set; }

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000222 RID: 546 RVA: 0x00007755 File Offset: 0x00005955
		// (set) Token: 0x06000223 RID: 547 RVA: 0x0000775D File Offset: 0x0000595D
		public uint MidBone { get; set; }

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000224 RID: 548 RVA: 0x00007766 File Offset: 0x00005966
		// (set) Token: 0x06000225 RID: 549 RVA: 0x0000776E File Offset: 0x0000596E
		public uint EndBone { get; set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000226 RID: 550 RVA: 0x00007777 File Offset: 0x00005977
		// (set) Token: 0x06000227 RID: 551 RVA: 0x0000777F File Offset: 0x0000597F
		public uint DataSize { get; set; }

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000228 RID: 552 RVA: 0x00007788 File Offset: 0x00005988
		// (set) Token: 0x06000229 RID: 553 RVA: 0x00007790 File Offset: 0x00005990
		public byte[] Data { get; set; }

		// Token: 0x0600022A RID: 554 RVA: 0x00007799 File Offset: 0x00005999
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x0600022B RID: 555 RVA: 0x000077D8 File Offset: 0x000059D8
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.RootBone, endian);
			output.WriteValueU32(this.MidBone, endian);
			output.WriteValueU32(this.EndBone, endian);
			output.WriteValueU32(this.DataSize, endian);
			output.WriteBytes(this.Data);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x00007834 File Offset: 0x00005A34
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.RootBone = input.ReadValueU32(endian);
			this.MidBone = input.ReadValueU32(endian);
			this.EndBone = input.ReadValueU32(endian);
			this.DataSize = input.ReadValueU32(endian);
			this.Data = input.ReadBytes((int)(this.DataSize * 4U));
		}
	}
}
