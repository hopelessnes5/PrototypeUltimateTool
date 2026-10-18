using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000010 RID: 16
	public class Vector1 : ISerializable
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003605 File Offset: 0x00001805
		// (set) Token: 0x0600005E RID: 94 RVA: 0x0000360D File Offset: 0x0000180D
		public float X { get; set; }

		// Token: 0x0600005F RID: 95 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector1()
		{
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00003616 File Offset: 0x00001816
		public Vector1(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003626 File Offset: 0x00001826
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.X, endian);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003635 File Offset: 0x00001835
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueF32(endian);
		}
	}
}
