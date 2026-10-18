using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000020 RID: 32
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184020U)]
	public class Quaternion3CompressedChannel : AnimationChannel
	{
		// Token: 0x0600010D RID: 269 RVA: 0x00004A20 File Offset: 0x00002C20
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueU32(base.NumberOfFrames);
			if (!base.ContainsAnimData)
			{
				return;
			}
			foreach (ushort value in base.GetRawFrameKeys())
			{
				output.WriteValueU16(value, endian);
			}
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				byte value2 = (byte)(vector.X * 127f);
				byte value3 = (byte)(vector.Y * 127f);
				byte value4 = (byte)(vector.Z * 127f);
				output.WriteByte(value2);
				output.WriteByte(value3);
				output.WriteByte(value4);
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00004B18 File Offset: 0x00002D18
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00004B78 File Offset: 0x00002D78
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				byte[] array = input.ReadBytes(1);
				vector.X = Convert.ToSingle((sbyte)array[0]);
				array = input.ReadBytes(1);
				vector.Y = Convert.ToSingle((sbyte)array[0]);
				array = input.ReadBytes(1);
				vector.Z = Convert.ToSingle((sbyte)array[0]);
				vector.X *= 0.007874016f;
				vector.Y *= 0.007874016f;
				vector.Z *= 0.007874016f;
				float num = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
				float num2 = 1f - num;
				if (num2 <= 0f)
				{
					vector.W = 0f;
				}
				else
				{
					vector.W = (float)Math.Sqrt((double)num2);
				}
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00004CA4 File Offset: 0x00002EA4
		public override Vector4 CalculateValue(float frame)
		{
			int index;
			int index2;
			Vector4 result;
			if (this.GetKey(frame, out index, out index2))
			{
				float num = frame - (float)base.OrderedFrameKeys[index];
				if (num == 0f)
				{
					result = base.Frames[base.OrderedFrameKeys[index]];
				}
				else
				{
					float delta = num / (float)(base.OrderedFrameKeys[index2] - base.OrderedFrameKeys[index]);
					result = Vector4.QuaternionSlerp(base.Frames[base.OrderedFrameKeys[index]], base.Frames[base.OrderedFrameKeys[index2]], delta);
				}
			}
			else
			{
				result = base.Frames[base.OrderedFrameKeys[index]];
			}
			return result;
		}
	}
}
