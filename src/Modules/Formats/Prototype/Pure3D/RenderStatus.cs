using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200003A RID: 58
	[KnownType(65559U)]
	public class RenderStatus : BaseNode
	{
		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000203 RID: 515 RVA: 0x0000752F File Offset: 0x0000572F
		// (set) Token: 0x06000204 RID: 516 RVA: 0x00007537 File Offset: 0x00005737
		public uint CastShadow { get; set; }

		// Token: 0x06000205 RID: 517 RVA: 0x00007540 File Offset: 0x00005740
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.CastShadow, endian);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000754F File Offset: 0x0000574F
		public override void Deserialize(Stream input, Endian endian)
		{
			this.CastShadow = input.ReadValueU32(endian);
		}
	}
}
