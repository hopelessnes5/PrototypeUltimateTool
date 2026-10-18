using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype1
{
	// Token: 0x02000084 RID: 132
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65554U)]
	public class VertexBuffer : BaseNode
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000562 RID: 1378 RVA: 0x0000C182 File Offset: 0x0000A382
		// (set) Token: 0x06000563 RID: 1379 RVA: 0x0000C18A File Offset: 0x0000A38A
		public uint Version { get; set; }

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000564 RID: 1380 RVA: 0x0000C193 File Offset: 0x0000A393
		// (set) Token: 0x06000565 RID: 1381 RVA: 0x0000C19B File Offset: 0x0000A39B
		public uint Param { get; set; }

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x0000C1A4 File Offset: 0x0000A3A4
		// (set) Token: 0x06000567 RID: 1383 RVA: 0x0000C1AC File Offset: 0x0000A3AC
		[ReadOnly(true)]
		public uint BufferSize { get; set; }

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000568 RID: 1384 RVA: 0x0000C1B5 File Offset: 0x0000A3B5
		// (set) Token: 0x06000569 RID: 1385 RVA: 0x0000C1BD File Offset: 0x0000A3BD
		public BufferItem[] BufferItems { get; set; }

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x0600056A RID: 1386 RVA: 0x0000C1C6 File Offset: 0x0000A3C6
		// (set) Token: 0x0600056B RID: 1387 RVA: 0x0000C1CE File Offset: 0x0000A3CE
		public VertexDescriptionList Description { get; set; }

		// Token: 0x0600056C RID: 1388 RVA: 0x0000C1D8 File Offset: 0x0000A3D8
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Param, endian);
			output.WriteValueU32(this.BufferSize, endian);
			uint num = 0U;
			while ((ulong)num < (ulong)((long)this.BufferItems.Length))
			{
				output.WriteBytes(this.BufferItems[(int)num].Data);
				num += 1U;
			}
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0000C234 File Offset: 0x0000A434
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Param = input.ReadValueU32(endian);
			this.BufferSize = input.ReadValueU32(endian);
			VertexDescriptionList vertexDescriptionList = this.ParentNode == null ? null : this.ParentNode.GetChildNodes<VertexDescriptionList>().Find((VertexDescriptionList x) => x.Param == this.Param);
			if (vertexDescriptionList == null || vertexDescriptionList.Descriptions == null || vertexDescriptionList.AmountOfDescriptions == 0U)
				throw new FormatException("VertexBuffer has no matching VertexDescriptionList.");
			uint objectSize = vertexDescriptionList.VertexObjectSize;
			if (objectSize == 0U || this.BufferSize % objectSize != 0U ||
				(input.CanSeek && (long)this.BufferSize > input.Length - input.Position))
				throw new FormatException("VertexBuffer size is inconsistent with the descriptor or chunk payload.");
			long vertexCount = this.BufferSize / objectSize;
			long itemCount = vertexCount * vertexDescriptionList.AmountOfDescriptions;
			if (itemCount > 1000000L || itemCount > int.MaxValue || vertexDescriptionList.Descriptions.Length < vertexDescriptionList.AmountOfDescriptions)
				throw new FormatException("VertexBuffer item count is too large or descriptors are incomplete.");
			for (int d = 0; d < vertexDescriptionList.AmountOfDescriptions; d++)
			{
				uint begin = vertexDescriptionList.Descriptions[d].Offset;
				uint finish = d + 1 < vertexDescriptionList.AmountOfDescriptions ? vertexDescriptionList.Descriptions[d + 1].Offset : objectSize;
				if (begin > finish || finish > objectSize) throw new FormatException("VertexBuffer descriptor offset is out of bounds.");
			}
			this.Description = vertexDescriptionList;
			this.BufferItems = new BufferItem[(int)itemCount];
			for (int vertex = 0; vertex < vertexCount; vertex++)
			{
				byte[] record = input.ReadBytes((int)objectSize);
				if (record == null || record.Length != (int)objectSize) throw new EndOfStreamException("VertexBuffer vertex data is truncated.");
				for (int d = 0; d < vertexDescriptionList.AmountOfDescriptions; d++)
				{
					uint begin = vertexDescriptionList.Descriptions[d].Offset;
					uint finish = d + 1 < vertexDescriptionList.AmountOfDescriptions ? vertexDescriptionList.Descriptions[d + 1].Offset : objectSize;
					int length = checked((int)(finish - begin));
					var item = new BufferItem(endian) { BufferType = vertexDescriptionList.Descriptions[d].BufferType, Data = new byte[length] };
					Buffer.BlockCopy(record, (int)begin, item.Data, 0, length);
					this.BufferItems[vertex * (int)vertexDescriptionList.AmountOfDescriptions + d] = item;
				}
			}
		}
	}
}
