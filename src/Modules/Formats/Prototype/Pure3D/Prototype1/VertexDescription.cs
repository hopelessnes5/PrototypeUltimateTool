using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype1
{
	// Token: 0x02000085 RID: 133
	[TypeConverter(typeof(MyTypeConverter))]
	public class VertexDescription
	{
		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000570 RID: 1392 RVA: 0x0000C39F File Offset: 0x0000A59F
		// (set) Token: 0x06000571 RID: 1393 RVA: 0x0000C3A7 File Offset: 0x0000A5A7
		[Descriptable(true)]
		public DescriptionType BufferType { get; set; }

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000572 RID: 1394 RVA: 0x0000C3B0 File Offset: 0x0000A5B0
		// (set) Token: 0x06000573 RID: 1395 RVA: 0x0000C3B8 File Offset: 0x0000A5B8
		[Descriptable(true)]
		public uint Unknown1 { get; set; }

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000574 RID: 1396 RVA: 0x0000C3C1 File Offset: 0x0000A5C1
		// (set) Token: 0x06000575 RID: 1397 RVA: 0x0000C3C9 File Offset: 0x0000A5C9
		[Descriptable(true)]
		public uint Offset { get; set; }

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000576 RID: 1398 RVA: 0x0000C3D2 File Offset: 0x0000A5D2
		// (set) Token: 0x06000577 RID: 1399 RVA: 0x0000C3DA File Offset: 0x0000A5DA
		[Descriptable(true)]
		public uint VertexObjectSize { get; set; }

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000578 RID: 1400 RVA: 0x0000C3E3 File Offset: 0x0000A5E3
		// (set) Token: 0x06000579 RID: 1401 RVA: 0x0000C3EB File Offset: 0x0000A5EB
		[Descriptable(true)]
		public uint Unknown2 { get; set; }

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x0000C3F4 File Offset: 0x0000A5F4
		// (set) Token: 0x0600057B RID: 1403 RVA: 0x0000C3FC File Offset: 0x0000A5FC
		[Descriptable(true)]
		protected uint UnknownType { get; set; }

		// Token: 0x0600057C RID: 1404 RVA: 0x0000C405 File Offset: 0x0000A605
		public override string ToString()
		{
			return "Vertex Description";
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public VertexDescription()
		{
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0000C40C File Offset: 0x0000A60C
		public VertexDescription(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0000C41C File Offset: 0x0000A61C
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferType.ToP1Type(), endian);
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Offset, endian);
			output.WriteByte((byte)this.VertexObjectSize);
			output.WriteValueU32(this.Unknown2, endian);
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0000C470 File Offset: 0x0000A670
		public void Deserialize(Stream input, Endian endian)
		{
			this.BufferType = new DescriptionType(input.ReadValueU32(endian));
			this.Unknown1 = input.ReadValueU32(endian);
			this.Offset = input.ReadValueU32(endian);
			this.VertexObjectSize = (uint)input.ReadByte();
			this.Unknown2 = input.ReadValueU32(endian);
		}
	}
}
