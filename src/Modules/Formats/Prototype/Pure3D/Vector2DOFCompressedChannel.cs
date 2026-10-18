using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000024 RID: 36
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184024U)]
	public class Vector2DOFCompressedChannel : AnimationChannel
	{
		// Token: 0x06000121 RID: 289 RVA: 0x00005788 File Offset: 0x00003988
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteValueU16(base.Mapping);
			base.BaseValues.Serialize(output, endian);
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
				Half value2 = (Half)vector.X;
				output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value2)));
				value2 = (Half)vector.Y;
				output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value2)));
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00005888 File Offset: 0x00003A88
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.Mapping = input.ReadValueU16(endian);
			base.BaseValues = new Vector3(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00005904 File Offset: 0x00003B04
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				Vector2Half vector2Half = new Vector2Half(input, endian);
				vector.X = vector2Half.X;
				vector.Y = vector2Half.Y;
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00005974 File Offset: 0x00003B74
		public override Vector4 CalculateValue(float frame)
		{
			Vector4 vector = new Vector4
			{
				X = base.BaseValues.X,
				Y = base.BaseValues.Y,
				Z = base.BaseValues.Z,
				W = 0f
			};
			int[] array = Constants.STATIC_INDEX[(int)base.Mapping];
			int index;
			int index2;
			if (this.GetKey(frame, out index, out index2))
			{
				float num = frame - (float)base.OrderedFrameKeys[index];
				if (num == 0f)
				{
					vector[array[0]] = base.Frames[base.OrderedFrameKeys[index]].X;
					vector[array[1]] = base.Frames[base.OrderedFrameKeys[index]].Y;
				}
				else
				{
					float num2 = num / (float)(base.OrderedFrameKeys[index2] - base.OrderedFrameKeys[index]);
					vector[array[0]] = (base.Frames[base.OrderedFrameKeys[index2]].X - base.Frames[base.OrderedFrameKeys[index]].X) * num2 + base.Frames[base.OrderedFrameKeys[index]].X;
					vector[array[1]] = (base.Frames[base.OrderedFrameKeys[index2]].Y - base.Frames[base.OrderedFrameKeys[index]].Y) * num2 + base.Frames[base.OrderedFrameKeys[index]].Y;
				}
			}
			else
			{
				vector[array[0]] = base.Frames[base.OrderedFrameKeys[index]].X;
				vector[array[1]] = base.Frames[base.OrderedFrameKeys[index]].Y;
			}
			return vector;
		}
	}
}
