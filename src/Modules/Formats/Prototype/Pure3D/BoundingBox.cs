using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002C RID: 44
	[KnownGame(PrototypeGame.Any)]
	[KnownType(65539U)]
	public class BoundingBox : BaseNode
	{
		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000164 RID: 356 RVA: 0x0000661D File Offset: 0x0000481D
		// (set) Token: 0x06000165 RID: 357 RVA: 0x00006625 File Offset: 0x00004825
		public float LowerX { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000166 RID: 358 RVA: 0x0000662E File Offset: 0x0000482E
		// (set) Token: 0x06000167 RID: 359 RVA: 0x00006636 File Offset: 0x00004836
		public float LowerY { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0000663F File Offset: 0x0000483F
		// (set) Token: 0x06000169 RID: 361 RVA: 0x00006647 File Offset: 0x00004847
		public float LowerZ { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600016A RID: 362 RVA: 0x00006650 File Offset: 0x00004850
		// (set) Token: 0x0600016B RID: 363 RVA: 0x00006658 File Offset: 0x00004858
		public float UpperX { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600016C RID: 364 RVA: 0x00006661 File Offset: 0x00004861
		// (set) Token: 0x0600016D RID: 365 RVA: 0x00006669 File Offset: 0x00004869
		public float UpperY { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x0600016E RID: 366 RVA: 0x00006672 File Offset: 0x00004872
		// (set) Token: 0x0600016F RID: 367 RVA: 0x0000667A File Offset: 0x0000487A
		public float UpperZ { get; set; }

		// Token: 0x06000170 RID: 368 RVA: 0x00006684 File Offset: 0x00004884
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueF32(this.LowerX, endian);
			output.WriteValueF32(this.LowerY, endian);
			output.WriteValueF32(this.LowerZ, endian);
			output.WriteValueF32(this.UpperX, endian);
			output.WriteValueF32(this.UpperY, endian);
			output.WriteValueF32(this.UpperZ, endian);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x000066E0 File Offset: 0x000048E0
		public override void Deserialize(Stream input, Endian endian)
		{
			this.LowerX = input.ReadValueF32(endian);
			this.LowerY = input.ReadValueF32(endian);
			this.LowerZ = input.ReadValueF32(endian);
			this.UpperX = input.ReadValueF32(endian);
			this.UpperY = input.ReadValueF32(endian);
			this.UpperZ = input.ReadValueF32(endian);
		}
	}
}
