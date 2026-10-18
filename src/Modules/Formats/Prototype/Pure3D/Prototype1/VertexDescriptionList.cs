using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype1
{
	// Token: 0x02000086 RID: 134
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65556U)]
	public class VertexDescriptionList : BaseNode
	{
		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000581 RID: 1409 RVA: 0x0000C4C2 File Offset: 0x0000A6C2
		// (set) Token: 0x06000582 RID: 1410 RVA: 0x0000C4CA File Offset: 0x0000A6CA
		public uint Version { get; set; }

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x0000C4D3 File Offset: 0x0000A6D3
		// (set) Token: 0x06000584 RID: 1412 RVA: 0x0000C4DB File Offset: 0x0000A6DB
		public uint Param { get; set; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000585 RID: 1413 RVA: 0x0000C4E4 File Offset: 0x0000A6E4
		// (set) Token: 0x06000586 RID: 1414 RVA: 0x0000C4EC File Offset: 0x0000A6EC
		[ReadOnly(true)]
		public uint BufferSize { get; set; }

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000587 RID: 1415 RVA: 0x0000C4F5 File Offset: 0x0000A6F5
		// (set) Token: 0x06000588 RID: 1416 RVA: 0x0000C4FD File Offset: 0x0000A6FD
		[ReadOnly(true)]
		public uint DescriptionSize { get; set; }

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000589 RID: 1417 RVA: 0x0000C506 File Offset: 0x0000A706
		// (set) Token: 0x0600058A RID: 1418 RVA: 0x0000C50E File Offset: 0x0000A70E
		[ReadOnly(true)]
		public uint AmountOfDescriptions { get; set; }

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x0600058B RID: 1419 RVA: 0x0000C517 File Offset: 0x0000A717
		[ReadOnly(true)]
		public uint VertexObjectSize
		{
			get
			{
				return this.Descriptions[0].VertexObjectSize;
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x0600058C RID: 1420 RVA: 0x0000C526 File Offset: 0x0000A726
		// (set) Token: 0x0600058D RID: 1421 RVA: 0x0000C52E File Offset: 0x0000A72E
		public VertexDescription[] Descriptions { get; set; }

		// Token: 0x0600058E RID: 1422 RVA: 0x0000C538 File Offset: 0x0000A738
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Param, endian);
			output.WriteValueU32(this.BufferSize, endian);
			output.WriteValueU32(this.DescriptionSize, endian);
			for (uint num = 0U; num < this.AmountOfDescriptions; num += 1U)
			{
				this.Descriptions[(int)num].Serialize(output, endian);
			}
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0000C59C File Offset: 0x0000A79C
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Param = input.ReadValueU32(endian);
			this.BufferSize = input.ReadValueU32(endian);
			this.DescriptionSize = input.ReadValueU32(endian);
			long available = input.CanSeek ? input.Length - input.Position : -1L;
			if (this.DescriptionSize == 0U || this.DescriptionSize % 17U != 0U || this.DescriptionSize / 17U > 100000U || this.DescriptionSize > int.MaxValue ||
				(input.CanSeek && (long)this.DescriptionSize > available))
				throw new FormatException("VertexDescriptionList has an invalid descriptor table size.");
			this.AmountOfDescriptions = this.DescriptionSize / 17U;
			this.Descriptions = new VertexDescription[(int)this.AmountOfDescriptions];
			for (uint num = 0U; num < this.AmountOfDescriptions; num += 1U)
				this.Descriptions[(int)num] = new VertexDescription(input, endian);
		}
	}
}
