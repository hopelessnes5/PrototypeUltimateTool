using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000071 RID: 113
	public class AudioDialogueSubtitle : AudioType
	{
		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000483 RID: 1155 RVA: 0x0000AC09 File Offset: 0x00008E09
		// (set) Token: 0x06000484 RID: 1156 RVA: 0x0000AC11 File Offset: 0x00008E11
		public uint Unknown1 { get; set; }

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x0000AC1A File Offset: 0x00008E1A
		// (set) Token: 0x06000486 RID: 1158 RVA: 0x0000AC22 File Offset: 0x00008E22
		public AudioSubtitle[] AudioSubtitles { get; set; }

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000487 RID: 1159 RVA: 0x0000AC2B File Offset: 0x00008E2B
		// (set) Token: 0x06000488 RID: 1160 RVA: 0x0000AC33 File Offset: 0x00008E33
		public byte[] Unknown2 { get; set; }

		// Token: 0x06000489 RID: 1161 RVA: 0x0000AC3C File Offset: 0x00008E3C
		public AudioDialogueSubtitle(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x0000AC4C File Offset: 0x00008E4C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			for (int i = 0; i < this.AudioSubtitles.Length; i++)
			{
				this.AudioSubtitles[i].Serialize(output, endian);
			}
			output.WriteBytes(this.Unknown2);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x0000AC94 File Offset: 0x00008E94
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32();
			this.AudioSubtitles = new AudioSubtitle[5];
			for (int i = 0; i < this.AudioSubtitles.Length; i++)
			{
				this.AudioSubtitles[i] = new AudioSubtitle(input, endian);
			}
			this.Unknown2 = input.ReadBytes(14);
		}
	}
}
