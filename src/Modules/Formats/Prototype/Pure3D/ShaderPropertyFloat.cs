using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000056 RID: 86
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69655U)]
	public class ShaderPropertyFloat : BaseNode
	{
		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000398 RID: 920 RVA: 0x000096F5 File Offset: 0x000078F5
		// (set) Token: 0x06000399 RID: 921 RVA: 0x000096FD File Offset: 0x000078FD
		public string Name { get; set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600039A RID: 922 RVA: 0x00009706 File Offset: 0x00007906
		// (set) Token: 0x0600039B RID: 923 RVA: 0x0000970E File Offset: 0x0000790E
		public float Value { get; set; }

		// Token: 0x0600039C RID: 924 RVA: 0x00009717 File Offset: 0x00007917
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00009753 File Offset: 0x00007953
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueF32(this.Value, endian);
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0000976E File Offset: 0x0000796E
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Value = input.ReadValueF32(endian);
		}
	}
}
