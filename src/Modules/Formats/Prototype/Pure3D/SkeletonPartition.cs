using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200003D RID: 61
	[KnownGame(PrototypeGame.Any)]
	[KnownType(143362U)]
	public class SkeletonPartition : BaseNode
	{
		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000214 RID: 532 RVA: 0x0000766E File Offset: 0x0000586E
		// (set) Token: 0x06000215 RID: 533 RVA: 0x00007676 File Offset: 0x00005876
		public string Name { get; set; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000216 RID: 534 RVA: 0x0000767F File Offset: 0x0000587F
		// (set) Token: 0x06000217 RID: 535 RVA: 0x00007687 File Offset: 0x00005887
		public uint DataSize { get; set; }

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x06000218 RID: 536 RVA: 0x00007690 File Offset: 0x00005890
		// (set) Token: 0x06000219 RID: 537 RVA: 0x00007698 File Offset: 0x00005898
		public byte[] Data { get; set; }

		// Token: 0x0600021A RID: 538 RVA: 0x000076A1 File Offset: 0x000058A1
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x0600021B RID: 539 RVA: 0x000076DD File Offset: 0x000058DD
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.DataSize, endian);
			output.WriteBytes(this.Data);
		}

		// Token: 0x0600021C RID: 540 RVA: 0x00007704 File Offset: 0x00005904
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.DataSize = input.ReadValueU32(endian);
			this.Data = input.ReadBytes((int)(this.DataSize * 4U));
		}
	}
}
