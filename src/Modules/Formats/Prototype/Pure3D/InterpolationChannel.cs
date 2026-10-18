using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000027 RID: 39
	[KnownType(1184016U)]
	public class InterpolationChannel : BaseNode
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00006253 File Offset: 0x00004453
		// (set) Token: 0x06000131 RID: 305 RVA: 0x0000625B File Offset: 0x0000445B
		public uint Version { get; set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00006264 File Offset: 0x00004464
		// (set) Token: 0x06000133 RID: 307 RVA: 0x0000626C File Offset: 0x0000446C
		public float Interpolate { get; set; }

		// Token: 0x06000134 RID: 308 RVA: 0x00006275 File Offset: 0x00004475
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version);
			output.WriteValueF32(this.Interpolate);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x0000628F File Offset: 0x0000448F
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Interpolate = input.ReadValueF32(endian);
		}
	}
}
