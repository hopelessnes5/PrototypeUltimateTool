using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x0200000F RID: 15
	[TypeConverter(typeof(VectorTypeConverter))]
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class UVCoordinate
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000055 RID: 85 RVA: 0x0000358F File Offset: 0x0000178F
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00003597 File Offset: 0x00001797
		[DataMember(Name = "U", Order = 1)]
		public float U { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000035A0 File Offset: 0x000017A0
		// (set) Token: 0x06000058 RID: 88 RVA: 0x000035A8 File Offset: 0x000017A8
		[DataMember(Name = "V", Order = 2)]
		public float V { get; set; }

		// Token: 0x06000059 RID: 89 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public UVCoordinate()
		{
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000035B1 File Offset: 0x000017B1
		public UVCoordinate(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000035C1 File Offset: 0x000017C1
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.U, endian);
			output.WriteValueF32(1f - this.V, endian);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000035E3 File Offset: 0x000017E3
		public void Deserialize(Stream input, Endian endian)
		{
			this.U = input.ReadValueF32(endian);
			this.V = 1f - input.ReadValueF32(endian);
		}
	}
}
