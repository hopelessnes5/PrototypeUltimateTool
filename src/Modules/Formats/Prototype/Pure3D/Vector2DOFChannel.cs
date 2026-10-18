using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000023 RID: 35
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184003U)]
	public class Vector2DOFChannel : AnimationChannel
	{
		// Token: 0x0600011C RID: 284 RVA: 0x000053AC File Offset: 0x000035AC
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
				output.WriteValueF32(vector.X, endian);
				output.WriteValueF32(vector.Y, endian);
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00005498 File Offset: 0x00003698
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.Mapping = input.ReadValueU16(endian);
			base.BaseValues = new Vector3(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00005514 File Offset: 0x00003714
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				Vector2 vector2 = new Vector2(input, endian);
				vector.X = vector2.X;
				vector.Y = vector2.Y;
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00005584 File Offset: 0x00003784
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
					vector[array[0]] = base.Frames[base.OrderedFrameKeys[index]].X + (base.Frames[base.OrderedFrameKeys[index2]].X - base.Frames[base.OrderedFrameKeys[index]].X) * num2;
					vector[array[1]] = base.Frames[base.OrderedFrameKeys[index]].Y + (base.Frames[base.OrderedFrameKeys[index2]].Y - base.Frames[base.OrderedFrameKeys[index]].Y) * num2;
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
