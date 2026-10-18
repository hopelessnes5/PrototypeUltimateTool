using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000011 RID: 17
	[TypeConverter(typeof(MyTypeConverter))]
	public class Vector2 : ISerializable
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000063 RID: 99 RVA: 0x00003644 File Offset: 0x00001844
		// (set) Token: 0x06000064 RID: 100 RVA: 0x0000364C File Offset: 0x0000184C
		[Descriptable]
		public float X { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00003655 File Offset: 0x00001855
		// (set) Token: 0x06000066 RID: 102 RVA: 0x0000365D File Offset: 0x0000185D
		[Descriptable]
		public float Y { get; set; }

		// Token: 0x06000067 RID: 103 RVA: 0x00003666 File Offset: 0x00001866
		public override string ToString()
		{
			return string.Format("Vector2: X = {0} | Y = {1}", this.X, this.Y);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector2()
		{
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00003688 File Offset: 0x00001888
		public Vector2(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003698 File Offset: 0x00001898
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.X, endian);
			output.WriteValueF32(this.Y, endian);
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000036B4 File Offset: 0x000018B4
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueF32(endian);
			this.Y = input.ReadValueF32(endian);
		}
	}
}
