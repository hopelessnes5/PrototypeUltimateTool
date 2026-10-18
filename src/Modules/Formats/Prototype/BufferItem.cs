using System;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000003 RID: 3
	public class BufferItem
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000003 RID: 3 RVA: 0x00002058 File Offset: 0x00000258
		[DataMember(Name = "Buffer Type", Order = 1)]
		public DescriptionType BufferType { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4 RVA: 0x00002061 File Offset: 0x00000261
		// (set) Token: 0x06000005 RID: 5 RVA: 0x00002069 File Offset: 0x00000269
		[DataMember(Name = "Data", Order = 1)]
		public byte[] Data { get; set; }

		// Token: 0x06000006 RID: 6 RVA: 0x00002074 File Offset: 0x00000274
		public override string ToString()
		{
			return "Buffer Item (" + this.BufferType.EnumValue.ToString() + ")";
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000020A9 File Offset: 0x000002A9
		public BufferItem(Endian endianess)
		{
			this.endianess = endianess;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020B8 File Offset: 0x000002B8
		public object GetValue()
		{
			Stream input = new MemoryStream(this.Data);
			switch (this.BufferType.EnumValue)
			{
			case DescriptionTypeEnum.UV:
			case DescriptionTypeEnum.UV1:
				return new UVCoordinate(input, this.endianess);
			case DescriptionTypeEnum.Position:
			case DescriptionTypeEnum.Normal:
				return new Vector3(input, this.endianess);
			case DescriptionTypeEnum.Tangent:
				return new Vector4(input, this.endianess);
			case DescriptionTypeEnum.Weight:
			case DescriptionTypeEnum.Group:
			case DescriptionTypeEnum.Colour0:
				return this.Data;
			case DescriptionTypeEnum.Padding1:
				return new Vector2(input, this.endianess);
			}
			return this.Data;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002150 File Offset: 0x00000350
		public object GetValueP1()
		{
			Stream input = new MemoryStream(this.Data);
			switch (this.BufferType.EnumValue)
			{
			case DescriptionTypeEnum.UV:
				return new UVCoordinate(input, this.endianess);
			case DescriptionTypeEnum.Position:
			case DescriptionTypeEnum.Normal:
			case DescriptionTypeEnum.Weight:
				return new Vector3(input, this.endianess);
			case DescriptionTypeEnum.Tangent:
				return new Vector4(input, this.endianess);
			case DescriptionTypeEnum.Group:
				return this.Data;
			case DescriptionTypeEnum.Padding1:
				return new Vector2(input, this.endianess);
			}
			return this.Data;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000021E4 File Offset: 0x000003E4
		public object GetValueP2()
		{
			Stream input = new MemoryStream(this.Data);
			switch (this.BufferType.EnumValue)
			{
			case DescriptionTypeEnum.UV:
			case DescriptionTypeEnum.UV1:
				return new UVCoordinate(input, this.endianess);
			case DescriptionTypeEnum.Position:
			case DescriptionTypeEnum.Normal:
				return new Vector3(input, this.endianess);
			case DescriptionTypeEnum.Tangent:
			case DescriptionTypeEnum.Weight:
				return new Vector4(input, this.endianess);
			case DescriptionTypeEnum.Group:
			case DescriptionTypeEnum.Colour0:
				return this.Data;
			case DescriptionTypeEnum.Padding1:
				return new Vector2(input, this.endianess);
			}
			return this.Data;
		}

		// Token: 0x04000003 RID: 3
		public Endian endianess;
	}
}
