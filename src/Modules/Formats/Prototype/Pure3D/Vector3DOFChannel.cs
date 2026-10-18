using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000025 RID: 37
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184004U)]
	public class Vector3DOFChannel : AnimationChannel
	{
		// Token: 0x06000126 RID: 294 RVA: 0x00005B78 File Offset: 0x00003D78
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
				output.WriteValueF32(vector.X);
				output.WriteValueF32(vector.Y);
				output.WriteValueF32(vector.Z);
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00005C54 File Offset: 0x00003E54
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00005CB4 File Offset: 0x00003EB4
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				vector.X = input.ReadValueF32(endian);
				vector.Y = input.ReadValueF32(endian);
				vector.Z = input.ReadValueF32(endian);
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005D2C File Offset: 0x00003F2C
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
					vector.X = base.Frames[base.OrderedFrameKeys[index]].X + (base.Frames[base.OrderedFrameKeys[index2]].X - base.Frames[base.OrderedFrameKeys[index]].X) * num2;
					vector.Y = base.Frames[base.OrderedFrameKeys[index]].Y + (base.Frames[base.OrderedFrameKeys[index2]].Y - base.Frames[base.OrderedFrameKeys[index]].Y) * num2;
					vector.Z = base.Frames[base.OrderedFrameKeys[index]].Z + (base.Frames[base.OrderedFrameKeys[index2]].Z - base.Frames[base.OrderedFrameKeys[index]].Z) * num2;
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
