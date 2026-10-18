using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000047 RID: 71
	[KnownGame(PrototypeGame.Any)]
	[KnownType(102403U)]
	public class TextureSource : BaseNode
	{
		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060002B0 RID: 688 RVA: 0x00008304 File Offset: 0x00006504
		// (set) Token: 0x060002B1 RID: 689 RVA: 0x0000830C File Offset: 0x0000650C
		[Category("Image")]
		public string FileName { get; set; }

		// Token: 0x060002B2 RID: 690 RVA: 0x00008315 File Offset: 0x00006515
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.FileName);
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00008323 File Offset: 0x00006523
		public override void Deserialize(Stream input, Endian endian)
		{
			this.FileName = input.ReadStringAlignedU8();
		}
	}
}
