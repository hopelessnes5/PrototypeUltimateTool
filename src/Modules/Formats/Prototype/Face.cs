using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x0200000A RID: 10
	[TypeConverter(typeof(VectorTypeConverter))]
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class Face
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002E27 File Offset: 0x00001027
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00002E2F File Offset: 0x0000102F
		[DataMember(Name = "Point 1", Order = 1)]
		public ushort Point1 { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002E38 File Offset: 0x00001038
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00002E40 File Offset: 0x00001040
		[DataMember(Name = "Point 2", Order = 2)]
		public ushort Point2 { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002E49 File Offset: 0x00001049
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00002E51 File Offset: 0x00001051
		[DataMember(Name = "Point 3", Order = 3)]
		public ushort Point3 { get; set; }

		// Token: 0x06000033 RID: 51 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Face()
		{
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002E5A File Offset: 0x0000105A
		public Face(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002E6A File Offset: 0x0000106A
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU16(this.Point1, endian);
			output.WriteValueU16(this.Point2, endian);
			output.WriteValueU16(this.Point3, endian);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002E93 File Offset: 0x00001093
		public void Deserialize(Stream input, Endian endian)
		{
			this.Point1 = input.ReadValueU16(endian);
			this.Point2 = input.ReadValueU16(endian);
			this.Point3 = input.ReadValueU16(endian);
		}
	}
}
