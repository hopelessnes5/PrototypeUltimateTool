using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000070 RID: 112
	public class AudioSubtitle
	{
		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x0000AB1E File Offset: 0x00008D1E
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x0000AB26 File Offset: 0x00008D26
		public byte[] Unknown1 { get; set; }

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x0000AB2F File Offset: 0x00008D2F
		// (set) Token: 0x0600047C RID: 1148 RVA: 0x0000AB37 File Offset: 0x00008D37
		public string Language { get; set; }

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x0000AB40 File Offset: 0x00008D40
		// (set) Token: 0x0600047E RID: 1150 RVA: 0x0000AB48 File Offset: 0x00008D48
		public string Text { get; set; }

		// Token: 0x0600047F RID: 1151 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public AudioSubtitle()
		{
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0000AB51 File Offset: 0x00008D51
		public AudioSubtitle(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x0000AB64 File Offset: 0x00008D64
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Unknown1);
			output.WriteValueU32((uint)(this.Language.Length - 1), endian);
			output.WriteString(this.Language);
			output.WriteValueU32((uint)(this.Text.Length - 1), endian);
			output.WriteString(this.Text);
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x0000ABC0 File Offset: 0x00008DC0
		public void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadBytes(9);
			uint num = input.ReadValueU32(endian);
			this.Language = input.ReadString((int)(num + 1U));
			num = input.ReadValueU32(endian);
			this.Text = input.ReadString((int)(num + 1U));
		}
	}
}
