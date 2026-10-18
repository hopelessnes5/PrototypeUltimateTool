using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001A RID: 26
	public class SomethingAnimationData
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x00004442 File Offset: 0x00002642
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x0000444A File Offset: 0x0000264A
		public string Type { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000C6 RID: 198 RVA: 0x00004453 File Offset: 0x00002653
		// (set) Token: 0x060000C7 RID: 199 RVA: 0x0000445B File Offset: 0x0000265B
		public AnimationChannelHeader Header { get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00004464 File Offset: 0x00002664
		// (set) Token: 0x060000C9 RID: 201 RVA: 0x0000446C File Offset: 0x0000266C
		public byte[] Data { get; set; }

		// Token: 0x060000CA RID: 202 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public SomethingAnimationData()
		{
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00004475 File Offset: 0x00002675
		public SomethingAnimationData(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00004485 File Offset: 0x00002685
		public void Serialize(Stream output, Endian endian)
		{
			this.Header.Serialize(output, endian);
			output.WriteStringAlignedU8(this.Type);
			output.WriteBytes(this.Data);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x000044AC File Offset: 0x000026AC
		public void Deserialize(Stream input, Endian endian)
		{
			this.Header.Deserialize(input, endian);
			this.Type = input.ReadStringAlignedU8();
			this.Data = input.ReadBytes(16);
		}
	}
}
