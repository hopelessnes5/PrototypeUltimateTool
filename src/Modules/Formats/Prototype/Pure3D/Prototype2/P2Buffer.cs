using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x0200007D RID: 125
	[KnownGame(PrototypeGame.P2)]
	[KnownType(65602U)]
	public class P2Buffer : BaseNode
	{
		// Token: 0x17000197 RID: 407
		// (get) Token: 0x060004FF RID: 1279 RVA: 0x0000B786 File Offset: 0x00009986
		// (set) Token: 0x06000500 RID: 1280 RVA: 0x0000B78E File Offset: 0x0000998E
		public uint BufferSize { get; set; }

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x0000B797 File Offset: 0x00009997
		// (set) Token: 0x06000502 RID: 1282 RVA: 0x0000B79F File Offset: 0x0000999F
		public P2BufferDescriptor Description { get; set; }

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000503 RID: 1283 RVA: 0x0000B7A8 File Offset: 0x000099A8
		// (set) Token: 0x06000504 RID: 1284 RVA: 0x0000B7B0 File Offset: 0x000099B0
		public BufferItem[] BufferItems { get; set; }

		// Token: 0x06000505 RID: 1285 RVA: 0x0000B7BC File Offset: 0x000099BC
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.BufferSize, endian);
			uint num = 0U;
			while ((ulong)num < (ulong)((long)this.BufferItems.Length))
			{
				output.WriteBytes(this.BufferItems[(int)num].Data);
				num += 1U;
			}
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0000B800 File Offset: 0x00009A00
		public override void Deserialize(Stream input, Endian endian)
		{
			this.BufferSize = input.ReadValueU32(endian);
			this.Description = this.ParentNode == null ? null : this.ParentNode.GetChildNode<P2BufferDescriptor>();
			P2Primitive primitive = this.ParentNode as P2Primitive;
			if (this.Description == null || primitive == null || this.Description.DescriptionSize == 0U ||
				this.Description.Descriptions == null || this.Description.AmountOfDescriptions == 0U)
				throw new FormatException("P2Buffer is missing its parent primitive or descriptor.");
			if (input.CanSeek && (long)this.BufferSize > input.Length - input.Position)
				throw new FormatException("P2Buffer size exceeds its chunk payload.");
			if (this.BufferSize % this.Description.DescriptionSize != 0U)
				throw new FormatException("P2Buffer size is not divisible by descriptor size.");
			uint vertexCount = this.BufferSize / this.Description.DescriptionSize;
			if (vertexCount != primitive.NumberOfVertices)
				throw new FormatException("P2Buffer vertex count does not match its parent primitive.");
			long itemCount = (long)vertexCount * this.Description.AmountOfDescriptions;
			if (itemCount > 1000000L || itemCount > int.MaxValue || this.Description.Descriptions.Length < this.Description.AmountOfDescriptions)
				throw new FormatException("P2Buffer item count is too large or its descriptor table is incomplete.");
			for (int d = 0; d < this.Description.AmountOfDescriptions; d++)
			{
				uint begin = this.Description.Descriptions[d].Offset;
				uint finish = d + 1 < this.Description.AmountOfDescriptions ? this.Description.Descriptions[d + 1].Offset : this.Description.DescriptionSize;
				if (begin > finish || finish > this.Description.DescriptionSize) throw new FormatException("P2Buffer descriptor offset is out of bounds.");
			}
			this.BufferItems = new BufferItem[(int)itemCount];
			for (int vertex = 0; vertex < vertexCount; vertex++)
			{
				byte[] record = input.ReadBytes((int)this.Description.DescriptionSize);
				if (record == null || record.Length != (int)this.Description.DescriptionSize) throw new EndOfStreamException("P2Buffer vertex data is truncated.");
				for (int d = 0; d < this.Description.AmountOfDescriptions; d++)
				{
					uint begin = this.Description.Descriptions[d].Offset;
					uint finish = d + 1 < this.Description.AmountOfDescriptions ? this.Description.Descriptions[d + 1].Offset : this.Description.DescriptionSize;
					int length = checked((int)(finish - begin));
					var item = new BufferItem(endian) { BufferType = this.Description.Descriptions[d].BufferType, Data = new byte[length] };
					Buffer.BlockCopy(record, (int)begin, item.Data, 0, length);
					if (endian == Endian.Big) Array.Reverse(item.Data);
					this.BufferItems[vertex * (int)this.Description.AmountOfDescriptions + d] = item;
				}
			}
		}
	}
}
