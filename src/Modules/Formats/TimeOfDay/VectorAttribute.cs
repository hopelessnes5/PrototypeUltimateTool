using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000009 RID: 9
	public class VectorAttribute : MetaAttribute
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000028 RID: 40 RVA: 0x0000229B File Offset: 0x0000049B
		// (set) Token: 0x06000029 RID: 41 RVA: 0x000022A3 File Offset: 0x000004A3
		public Vector3 Vector { get; set; }

		// Token: 0x0600002A RID: 42 RVA: 0x000022AC File Offset: 0x000004AC
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			this.Vector.Serialize(output, endian);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000022C3 File Offset: 0x000004C3
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Vector = new Vector3(input, endian);
		}
	}
}
