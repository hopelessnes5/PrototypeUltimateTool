using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000012 RID: 18
	public class Vector2Half
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600006C RID: 108 RVA: 0x000036D0 File Offset: 0x000018D0
		// (set) Token: 0x0600006D RID: 109 RVA: 0x000036D8 File Offset: 0x000018D8
		public float X { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000036E1 File Offset: 0x000018E1
		// (set) Token: 0x0600006F RID: 111 RVA: 0x000036E9 File Offset: 0x000018E9
		public float Y { get; set; }

		// Token: 0x06000070 RID: 112 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector2Half()
		{
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000036F2 File Offset: 0x000018F2
		public Vector2Half(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003704 File Offset: 0x00001904
		public void Serialize(Stream output, Endian endian)
		{
			Half value = (Half)this.X;
			output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value)));
			value = (Half)this.Y;
			output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value)));
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00003744 File Offset: 0x00001944
		public void Deserialize(Stream input, Endian endian)
		{
			ushort bits = input.ReadValueU16(endian);
			this.X = (float)BitConverter.UInt16BitsToHalf(bits);
			ushort bits2 = input.ReadValueU16(endian);
			this.Y = (float)BitConverter.UInt16BitsToHalf(bits2);
		}
	}
}
