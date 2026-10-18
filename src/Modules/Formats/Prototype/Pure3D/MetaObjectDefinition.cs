using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005F RID: 95
	[KnownGame(PrototypeGame.Any)]
	[KnownType(133169152U)]
	public class MetaObjectDefinition : BaseNode
	{
		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060003EB RID: 1003 RVA: 0x00009D3C File Offset: 0x00007F3C
		// (set) Token: 0x060003EC RID: 1004 RVA: 0x00009D44 File Offset: 0x00007F44
		public string Name { get; set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x00009D4D File Offset: 0x00007F4D
		// (set) Token: 0x060003EE RID: 1006 RVA: 0x00009D55 File Offset: 0x00007F55
		[ReadOnly(true)]
		public string TypeName { get; set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060003EF RID: 1007 RVA: 0x00009D5E File Offset: 0x00007F5E
		// (set) Token: 0x060003F0 RID: 1008 RVA: 0x00009D66 File Offset: 0x00007F66
		public ushort Flag1 { get; set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060003F1 RID: 1009 RVA: 0x00009D6F File Offset: 0x00007F6F
		// (set) Token: 0x060003F2 RID: 1010 RVA: 0x00009D77 File Offset: 0x00007F77
		public ushort Flag2 { get; set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x00009D80 File Offset: 0x00007F80
		[Browsable(false)]
		public string FullName
		{
			get
			{
				return string.Format("{0}(1):{1}", this.TypeName.Trim(new char[1]), this.Name.Trim(new char[1]));
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x00009DB0 File Offset: 0x00007FB0
		public override string ToString()
		{
			return base.ToString() + " (" + string.Format("{0}: {1}", this.TypeName.Trim(new char[1]), this.Name.Trim(new char[1])) + ")";
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00009E00 File Offset: 0x00008000
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.FullName);
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.TypeName);
			output.WriteValueU16(this.Flag1, endian);
			output.WriteValueU16(this.Flag2, endian);
			output.WriteValueS32(916723515, endian);
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00009E58 File Offset: 0x00008058
		public override void Deserialize(Stream input, Endian endian)
		{
			input.ReadStringAlignedU8();
			this.Name = input.ReadStringAlignedU8();
			this.TypeName = input.ReadStringAlignedU8();
			this.Flag1 = input.ReadValueU16(endian);
			this.Flag2 = input.ReadValueU16(endian);
			input.ReadValueS32(endian);
		}
	}
}
