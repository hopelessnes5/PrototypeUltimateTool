using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000018 RID: 24
	[TypeConverter(typeof(VectorTypeConverter))]
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class Weight
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000AC RID: 172 RVA: 0x000042C7 File Offset: 0x000024C7
		// (set) Token: 0x060000AD RID: 173 RVA: 0x000042CF File Offset: 0x000024CF
		[DataMember(Name = "x", Order = 1)]
		public float X { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000AE RID: 174 RVA: 0x000042D8 File Offset: 0x000024D8
		// (set) Token: 0x060000AF RID: 175 RVA: 0x000042E0 File Offset: 0x000024E0
		[DataMember(Name = "y", Order = 1)]
		public float Y { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x000042E9 File Offset: 0x000024E9
		// (set) Token: 0x060000B1 RID: 177 RVA: 0x000042F1 File Offset: 0x000024F1
		[DataMember(Name = "z", Order = 1)]
		public float Z { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x000042FA File Offset: 0x000024FA
		// (set) Token: 0x060000B3 RID: 179 RVA: 0x00004302 File Offset: 0x00002502
		[DataMember(Name = "Data?", Order = 1)]
		public byte[] Data { get; set; }

		// Token: 0x060000B4 RID: 180 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Weight()
		{
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x0000430B File Offset: 0x0000250B
		public Weight(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x0000431B File Offset: 0x0000251B
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.X, endian);
			output.WriteValueF32(this.Y, endian);
			output.WriteValueF32(this.Z, endian);
			output.WriteBytes(this.Data);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004350 File Offset: 0x00002550
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueF32(endian);
			this.Y = input.ReadValueF32(endian);
			this.Z = input.ReadValueF32(endian);
			this.Data = input.ReadBytes(4);
		}
	}
}
