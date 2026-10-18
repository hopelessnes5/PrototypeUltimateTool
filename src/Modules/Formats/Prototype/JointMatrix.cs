using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x0200000C RID: 12
	[TypeConverter(typeof(MatrixTypeConverter))]
	[DataContract(Namespace = "http://datacontract.gib.me/prototype")]
	public class JointMatrix
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00002FF8 File Offset: 0x000011F8
		// (set) Token: 0x06000041 RID: 65 RVA: 0x00003000 File Offset: 0x00001200
		public uint Width { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00003009 File Offset: 0x00001209
		// (set) Token: 0x06000043 RID: 67 RVA: 0x00003011 File Offset: 0x00001211
		public uint Height { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000044 RID: 68 RVA: 0x0000301A File Offset: 0x0000121A
		// (set) Token: 0x06000045 RID: 69 RVA: 0x00003022 File Offset: 0x00001222
		public float[,] Content { get; set; }

		// Token: 0x06000046 RID: 70 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public JointMatrix()
		{
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000302B File Offset: 0x0000122B
		public JointMatrix(Stream input, Endian endian, uint width, uint height)
		{
			this.Width = width;
			this.Height = height;
			this.Deserialize(input, endian);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x0000304C File Offset: 0x0000124C
		public void Serialize(Stream output, Endian endian)
		{
			int num = 0;
			while ((long)num < (long)((ulong)this.Height))
			{
				int num2 = 0;
				while ((long)num2 < (long)((ulong)this.Width))
				{
					output.WriteValueF32(this.Content[num, num2], endian);
					num2++;
				}
				num++;
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003094 File Offset: 0x00001294
		public void Deserialize(Stream input, Endian endian)
		{
			this.Content = new float[(int)this.Height, (int)this.Width];
			int num = 0;
			while ((long)num < (long)((ulong)this.Height))
			{
				int num2 = 0;
				while ((long)num2 < (long)((ulong)this.Width))
				{
					this.Content[num, num2] = input.ReadValueF32(endian);
					num2++;
				}
				num++;
			}
		}
	}
}
