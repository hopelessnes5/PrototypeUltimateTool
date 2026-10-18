using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000062 RID: 98
	[KnownGame(PrototypeGame.Any)]
	[KnownType(536872705U)]
	public class FightDefinition : BaseNode
	{
		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x0000A20D File Offset: 0x0000840D
		// (set) Token: 0x06000404 RID: 1028 RVA: 0x0000A215 File Offset: 0x00008415
		public string Name { get; set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x0000A21E File Offset: 0x0000841E
		// (set) Token: 0x06000406 RID: 1030 RVA: 0x0000A226 File Offset: 0x00008426
		public ushort Unknown2 { get; set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000407 RID: 1031 RVA: 0x0000A22F File Offset: 0x0000842F
		// (set) Token: 0x06000408 RID: 1032 RVA: 0x0000A237 File Offset: 0x00008437
		public string Context { get; set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x0000A240 File Offset: 0x00008440
		// (set) Token: 0x0600040A RID: 1034 RVA: 0x0000A248 File Offset: 0x00008448
		public uint Unknown4 { get; set; }

		// Token: 0x0600040B RID: 1035 RVA: 0x0000A251 File Offset: 0x00008451
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0000A28D File Offset: 0x0000848D
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU16(this.Unknown2, endian);
			output.WriteStringAlignedU8(this.Context);
			output.WriteValueU32(this.Unknown4, endian);
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0000A2C1 File Offset: 0x000084C1
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown2 = input.ReadValueU16(endian);
			this.Context = input.ReadStringAlignedU8();
			this.Unknown4 = input.ReadValueU32(endian);
		}
	}
}
