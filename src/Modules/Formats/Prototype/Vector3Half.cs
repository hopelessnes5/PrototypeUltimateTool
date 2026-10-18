using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000015 RID: 21
	public class Vector3Half
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00003B1D File Offset: 0x00001D1D
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00003B25 File Offset: 0x00001D25
		public float X { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000093 RID: 147 RVA: 0x00003B2E File Offset: 0x00001D2E
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00003B36 File Offset: 0x00001D36
		public float Y { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00003B3F File Offset: 0x00001D3F
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00003B47 File Offset: 0x00001D47
		public float Z { get; set; }

		// Token: 0x06000097 RID: 151 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector3Half()
		{
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00003B50 File Offset: 0x00001D50
		public Vector3Half(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003B60 File Offset: 0x00001D60
		public void Serialize(Stream output, Endian endian)
		{
			Half value = (Half)this.X;
			output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value)));
			value = (Half)this.Y;
			output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value)));
			value = (Half)this.Z;
			output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value)));
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003BB8 File Offset: 0x00001DB8
		public void Deserialize(Stream input, Endian endian)
		{
			ushort bits = input.ReadValueU16(endian);
			this.X = (float)BitConverter.UInt16BitsToHalf(bits);
			ushort bits2 = input.ReadValueU16(endian);
			this.Y = (float)BitConverter.UInt16BitsToHalf(bits2);
			ushort bits3 = input.ReadValueU16(endian);
			this.Z = (float)BitConverter.UInt16BitsToHalf(bits3);
		}
	}
}
