using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000014 RID: 20
	[TypeConverter(typeof(VectorTypeConverter))]
	public class Vector4
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600007E RID: 126 RVA: 0x00003818 File Offset: 0x00001A18
		// (set) Token: 0x0600007F RID: 127 RVA: 0x00003820 File Offset: 0x00001A20
		public float X { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000080 RID: 128 RVA: 0x00003829 File Offset: 0x00001A29
		// (set) Token: 0x06000081 RID: 129 RVA: 0x00003831 File Offset: 0x00001A31
		public float Y { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000082 RID: 130 RVA: 0x0000383A File Offset: 0x00001A3A
		// (set) Token: 0x06000083 RID: 131 RVA: 0x00003842 File Offset: 0x00001A42
		public float Z { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000084 RID: 132 RVA: 0x0000384B File Offset: 0x00001A4B
		// (set) Token: 0x06000085 RID: 133 RVA: 0x00003853 File Offset: 0x00001A53
		public float W { get; set; }

		// Token: 0x1700001C RID: 28
		public float this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return this.X;
				case 1:
					return this.Y;
				case 2:
					return this.Z;
				case 3:
					return this.W;
				default:
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					this.X = value;
					return;
				case 1:
					this.Y = value;
					return;
				case 2:
					this.Z = value;
					return;
				case 3:
					this.W = value;
					return;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Vector4()
		{
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000038D6 File Offset: 0x00001AD6
		public Vector4(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000038E6 File Offset: 0x00001AE6
		public static implicit operator Vector4(Vector3 vec)
		{
			return new Vector4
			{
				X = vec.X,
				Y = vec.Y,
				Z = vec.Z
			};
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003911 File Offset: 0x00001B11
		public static implicit operator Vector4(Vector2 vec)
		{
			return new Vector4
			{
				X = vec.X,
				Y = vec.Y
			};
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00003930 File Offset: 0x00001B30
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.X, endian);
			output.WriteValueF32(this.Y, endian);
			output.WriteValueF32(this.Z, endian);
			output.WriteValueF32(this.W, endian);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003966 File Offset: 0x00001B66
		public void Deserialize(Stream input, Endian endian)
		{
			this.X = input.ReadValueF32(endian);
			this.Y = input.ReadValueF32(endian);
			this.Z = input.ReadValueF32(endian);
			this.W = input.ReadValueF32(endian);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000399C File Offset: 0x00001B9C
		public static Vector4 QuaternionSlerp(Vector4 q1, Vector4 q2, float delta)
		{
			float num = 1f;
			float num2 = q1.W * q2.W + q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z;
			if (num2 < 0f)
			{
				num = -1f;
				num2 = -num2;
			}
			float num3;
			if (1f - num2 < 0.0027777778f)
			{
				num3 = 1f - delta;
				num *= delta;
			}
			else
			{
				float num4 = (float)Math.Acos((double)num2);
				float num5 = 1f / (float)Math.Sin((double)num4);
				num3 = (float)Math.Sin((double)((1f - delta) * num4)) * num5;
				num *= (float)Math.Sin((double)(delta * num4)) * num5;
			}
			return new Vector4
			{
				W = num3 * q1.W + num * q2.W,
				X = num3 * q1.X + num * q2.X,
				Y = num3 * q1.Y + num * q2.Y,
				Z = num3 * q1.Z + num * q2.Z
			};
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003AB2 File Offset: 0x00001CB2
		public static explicit operator Vector4(Vector3Half value)
		{
			return new Vector4
			{
				X = value.X,
				Y = value.Y,
				Z = value.Z,
				W = 0f
			};
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003AE8 File Offset: 0x00001CE8
		public static explicit operator Vector4(Vector2Half value)
		{
			return new Vector4
			{
				X = value.X,
				Y = value.Y,
				Z = 0f,
				W = 0f
			};
		}
	}
}
