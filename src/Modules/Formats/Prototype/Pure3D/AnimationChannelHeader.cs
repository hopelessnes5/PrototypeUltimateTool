using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000019 RID: 25
	public class AnimationChannelHeader
	{
		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x00004386 File Offset: 0x00002586
		// (set) Token: 0x060000B9 RID: 185 RVA: 0x0000438E File Offset: 0x0000258E
		public uint Type { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000BA RID: 186 RVA: 0x00004397 File Offset: 0x00002597
		// (set) Token: 0x060000BB RID: 187 RVA: 0x0000439F File Offset: 0x0000259F
		public uint Unknown1 { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000BC RID: 188 RVA: 0x000043A8 File Offset: 0x000025A8
		// (set) Token: 0x060000BD RID: 189 RVA: 0x000043B0 File Offset: 0x000025B0
		public uint Unknown2 { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000BE RID: 190 RVA: 0x000043B9 File Offset: 0x000025B9
		// (set) Token: 0x060000BF RID: 191 RVA: 0x000043C1 File Offset: 0x000025C1
		public uint Unknown3 { get; set; }

		// Token: 0x060000C0 RID: 192 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public AnimationChannelHeader()
		{
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000043CA File Offset: 0x000025CA
		public AnimationChannelHeader(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x000043DA File Offset: 0x000025DA
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Type);
			output.WriteValueU32(this.Unknown1);
			output.WriteValueU32(this.Unknown2);
			output.WriteValueU32(this.Unknown3);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000440C File Offset: 0x0000260C
		public void Deserialize(Stream input, Endian endian)
		{
			this.Type = input.ReadValueU32(endian);
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			this.Unknown3 = input.ReadValueU32(endian);
		}
	}
}
