using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000033 RID: 51
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65536U)]
	public class Geometry : BaseNode
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00006D44 File Offset: 0x00004F44
		// (set) Token: 0x060001BD RID: 445 RVA: 0x00006D4C File Offset: 0x00004F4C
		public string Name { get; set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00006D55 File Offset: 0x00004F55
		// (set) Token: 0x060001BF RID: 447 RVA: 0x00006D5D File Offset: 0x00004F5D
		public uint Version { get; set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x00006D66 File Offset: 0x00004F66
		// (set) Token: 0x060001C1 RID: 449 RVA: 0x00006D6E File Offset: 0x00004F6E
		public uint NumPrimitiveGroups { get; set; }

		// Token: 0x060001C2 RID: 450 RVA: 0x00006D77 File Offset: 0x00004F77
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00006DA8 File Offset: 0x00004FA8
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.NumPrimitiveGroups, endian);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00006DD0 File Offset: 0x00004FD0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.NumPrimitiveGroups = input.ReadValueU32(endian);
		}
	}
}
