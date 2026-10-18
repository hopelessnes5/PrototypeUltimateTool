using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000021 RID: 33
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184018U)]
	public class Quaternion6CompressedChannel : AnimationChannel
	{
		// Token: 0x06000112 RID: 274 RVA: 0x00004D74 File Offset: 0x00002F74
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
				int num = (int)(vector.X * 32767f);
				int num2 = (int)(vector.Y * 32767f);
				int num3 = (int)(vector.Z * 32767f);
				output.WriteValueS16((short)num);
				output.WriteValueS16((short)num2);
				output.WriteValueS16((short)num3);
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00004E70 File Offset: 0x00003070
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00004ED0 File Offset: 0x000030D0
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				vector.X = Convert.ToSingle((int)input.ReadValueS16());
				vector.Y = Convert.ToSingle((int)input.ReadValueS16());
				vector.Z = Convert.ToSingle((int)input.ReadValueS16());
				vector.X *= 3.051851E-05f;
				vector.Y *= 3.051851E-05f;
				vector.Z *= 3.051851E-05f;
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

		// Token: 0x06000115 RID: 277 RVA: 0x00004FE8 File Offset: 0x000031E8
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
