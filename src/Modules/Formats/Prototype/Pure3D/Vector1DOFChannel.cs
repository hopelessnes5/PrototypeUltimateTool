using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000022 RID: 34
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1184000U)]
	public class Vector1DOFChannel : AnimationChannel
	{
		// Token: 0x06000117 RID: 279 RVA: 0x000050B0 File Offset: 0x000032B0
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
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000518C File Offset: 0x0000338C
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			base.Mapping = input.ReadValueU16(endian);
			base.BaseValues = new Vector3(input, endian);
			base.NumberOfFrames = input.ReadValueU32(endian);
			base.ReadFrameKeys(input, endian);
			this.ReadChannel(input, endian);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005208 File Offset: 0x00003408
		protected override void ReadChannel(Stream input, Endian endian)
		{
			foreach (ushort frameKey in base.GetRawFrameKeys())
			{
                Vector4 vector = base.Frames[frameKey];
				vector.X = input.ReadValueF32(endian);
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005264 File Offset: 0x00003464
		public override Vector4 CalculateValue(float frame)
		{
			Vector4 value = new Vector4
			{
				X = base.BaseValues.X,
				Y = base.BaseValues.Y,
				Z = base.BaseValues.Z
			};
			var keys = base.OrderedFrameKeys;
			if (keys.Length == 0 || base.Frames == null || base.Frames.Count == 0)
				return value;

			int start;
			int end;
			if (!this.GetKey(frame, out start, out end))
			{
				start = Math.Max(0, Math.Min(keys.Length - 1, start));
				int mapping = base.Mapping <= 2 ? base.Mapping : 0;
				value[mapping] = base.Frames[keys[start]].X;
				return value;
			}

			int component = base.Mapping <= 2 ? base.Mapping : 0;
			float startValue = base.Frames[keys[start]].X;
			if (frame <= keys[start] || end == start)
			{
				value[component] = startValue;
				return value;
			}

			float endValue = base.Frames[keys[end]].X;
			float span = keys[end] - keys[start];
			float t = span <= 0.0f ? 0.0f : (frame - keys[start]) / span;
			value[component] = startValue + (endValue - startValue) * t;
			return value;
		}
	}
}
