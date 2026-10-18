using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000013 RID: 19
	[TypeConverter(typeof(VectorTypeConverter))]
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class Vector3Int
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00003783 File Offset: 0x00001983
		// (set) Token: 0x06000075 RID: 117 RVA: 0x0000378B File Offset: 0x0000198B
		[DataMember(Name = "x", Order = 1)]
		public uint X { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00003794 File Offset: 0x00001994
		// (set) Token: 0x06000077 RID: 119 RVA: 0x0000379C File Offset: 0x0000199C
		[DataMember(Name = "y", Order = 1)]
		public uint Y { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000078 RID: 120 RVA: 0x000037A5 File Offset: 0x000019A5
		// (set) Token: 0x06000079 RID: 121 RVA: 0x000037AD File Offset: 0x000019AD
		[DataMember(Name = "z", Order = 1)]
		public uint Z { get; set; }

		// Token: 0x0600007A RID: 122 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector3Int()
		{
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000037B6 File Offset: 0x000019B6
		public Vector3Int(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x000037C6 File Offset: 0x000019C6
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.X, endian);
			output.WriteValueU32(this.Y, endian);
			output.WriteValueU32(this.Z, endian);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000037EF File Offset: 0x000019EF
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueU32(endian);
			this.Y = input.ReadValueU32(endian);
			this.Z = input.ReadValueU32(endian);
		}
	}
}
