using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002D RID: 45
	[KnownGame(PrototypeGame.Any)]
	[KnownType(65540U)]
	public class BoundingSphere : BaseNode
	{
		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000173 RID: 371 RVA: 0x0000673B File Offset: 0x0000493B
		// (set) Token: 0x06000174 RID: 372 RVA: 0x00006743 File Offset: 0x00004943
		public float CenterX { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000175 RID: 373 RVA: 0x0000674C File Offset: 0x0000494C
		// (set) Token: 0x06000176 RID: 374 RVA: 0x00006754 File Offset: 0x00004954
		public float CenterY { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000177 RID: 375 RVA: 0x0000675D File Offset: 0x0000495D
		// (set) Token: 0x06000178 RID: 376 RVA: 0x00006765 File Offset: 0x00004965
		public float CenterZ { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000179 RID: 377 RVA: 0x0000676E File Offset: 0x0000496E
		// (set) Token: 0x0600017A RID: 378 RVA: 0x00006776 File Offset: 0x00004976
		public float Radius { get; set; }

		// Token: 0x0600017B RID: 379 RVA: 0x0000677F File Offset: 0x0000497F
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.CenterX, endian);
			output.WriteValueF32(this.CenterY, endian);
			output.WriteValueF32(this.CenterZ, endian);
			output.WriteValueF32(this.Radius, endian);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x000067B5 File Offset: 0x000049B5
		public override void Deserialize(Stream input, Endian endian)
		{
			this.CenterX = input.ReadValueF32(endian);
			this.CenterY = input.ReadValueF32(endian);
			this.CenterZ = input.ReadValueF32(endian);
			this.Radius = input.ReadValueF32(endian);
		}
	}
}
