using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000006 RID: 6
	public class MatrixAttribute : MetaAttribute
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000015 RID: 21 RVA: 0x0000216B File Offset: 0x0000036B
		// (set) Token: 0x06000016 RID: 22 RVA: 0x00002173 File Offset: 0x00000373
		public Vector3 Position { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000017 RID: 23 RVA: 0x0000217C File Offset: 0x0000037C
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002184 File Offset: 0x00000384
		public Vector3 Rotation { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000019 RID: 25 RVA: 0x0000218D File Offset: 0x0000038D
		// (set) Token: 0x0600001A RID: 26 RVA: 0x00002195 File Offset: 0x00000395
		public float Scale { get; set; }

		// Token: 0x0600001B RID: 27 RVA: 0x0000219E File Offset: 0x0000039E
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			this.Position.Serialize(output, endian);
			this.Rotation.Serialize(output, endian);
			output.WriteValueF32(this.Scale, endian);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000021CF File Offset: 0x000003CF
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Position = new Vector3(input, endian);
			this.Rotation = new Vector3(input, endian);
			this.Scale = input.ReadValueF32(endian);
		}
	}
}
