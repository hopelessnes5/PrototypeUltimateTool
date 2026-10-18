using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000026 RID: 38
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184025U)]
	public class Vector3DOFCompressedChannel : AnimationChannel
	{
		// Token: 0x0600012B RID: 299 RVA: 0x00005ED4 File Offset: 0x000040D4
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
				Half value2 = (Half)vector.X;
				output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value2)));
				value2 = (Half)vector.Y;
				output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value2)));
				value2 = (Half)vector.Z;
				output.WriteBytes(BitConverter.GetBytes(BitConverter.HalfToUInt16Bits(value2)));
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00005FD0 File Offset: 0x000041D0
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00006030 File Offset: 0x00004230
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				Vector3Half vector3Half = new Vector3Half(input, endian);
				vector.X = vector3Half.X;
				vector.Y = vector3Half.Y;
				vector.Z = vector3Half.Z;
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x000060AC File Offset: 0x000042AC
		public override Vector4 CalculateValue(float frame)
		{
			Vector4 vector = new Vector4();
			int index;
			int index2;
			if (this.GetKey(frame, out index, out index2))
			{
				float num = frame - (float)base.OrderedFrameKeys[index];
				if (num == 0f)
				{
					vector = base.Frames[base.OrderedFrameKeys[index]];
				}
				else
				{
					float num2 = num / (float)(base.OrderedFrameKeys[index2] - base.OrderedFrameKeys[index]);
					vector.X = (base.Frames[base.OrderedFrameKeys[index2]].X - base.Frames[base.OrderedFrameKeys[index]].X) * num2 + base.Frames[base.OrderedFrameKeys[index]].X;
					vector.Y = (base.Frames[base.OrderedFrameKeys[index2]].Y - base.Frames[base.OrderedFrameKeys[index]].Y) * num2 + base.Frames[base.OrderedFrameKeys[index]].Y;
					vector.Z = (base.Frames[base.OrderedFrameKeys[index2]].Z - base.Frames[base.OrderedFrameKeys[index]].Z) * num2 + base.Frames[base.OrderedFrameKeys[index]].Z;
				}
			}
			else
			{
				vector = base.Frames[base.OrderedFrameKeys[index]];
			}
			return vector;
		}
	}
}
