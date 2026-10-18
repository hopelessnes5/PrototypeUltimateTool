using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000065 RID: 101
	[KnownGame(PrototypeGame.Any)]
	[KnownType(150994954U)]
	public class ShaderHash : BaseNode
	{
		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600041B RID: 1051 RVA: 0x0000A37A File Offset: 0x0000857A
		// (set) Token: 0x0600041C RID: 1052 RVA: 0x0000A382 File Offset: 0x00008582
		public string Hash { get; set; }

		// Token: 0x0600041D RID: 1053 RVA: 0x0000A38B File Offset: 0x0000858B
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Hash))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Hash + ")";
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x0000A3BC File Offset: 0x000085BC
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Hash);
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x0000A3CA File Offset: 0x000085CA
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Hash = input.ReadStringAlignedU8();
		}
	}
}
