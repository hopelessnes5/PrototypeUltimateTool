using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000072 RID: 114
	public class AudioFile : AudioType
	{
		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600048C RID: 1164 RVA: 0x0000ACE9 File Offset: 0x00008EE9
		// (set) Token: 0x0600048D RID: 1165 RVA: 0x0000ACF1 File Offset: 0x00008EF1
		public string IDName { get; set; }

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x0000ACFA File Offset: 0x00008EFA
		// (set) Token: 0x0600048F RID: 1167 RVA: 0x0000AD02 File Offset: 0x00008F02
		public uint Unknown1 { get; set; }

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x0000AD0B File Offset: 0x00008F0B
		// (set) Token: 0x06000491 RID: 1169 RVA: 0x0000AD13 File Offset: 0x00008F13
		public uint Unknown2 { get; set; }

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x0000AD1C File Offset: 0x00008F1C
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x0000AD24 File Offset: 0x00008F24
		public byte[] Data { get; set; }

		// Token: 0x06000494 RID: 1172 RVA: 0x0000AD2D File Offset: 0x00008F2D
		public AudioFile()
		{
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x0000AD35 File Offset: 0x00008F35
		public AudioFile(Stream input, Endian endian) : base(input, endian)
		{
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x0000AD40 File Offset: 0x00008F40
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32((uint)(this.IDName.Length - 1), endian);
			output.WriteString(this.IDName);
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteBytes(this.Data);
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x0000AD94 File Offset: 0x00008F94
		public override void Deserialize(Stream input, Endian endian)
		{
			uint num = input.ReadValueU32(endian);
			this.IDName = input.ReadString((int)(num + 1U));
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			input.Position += 21L;
			uint num2 = input.ReadValueU32(endian);
			input.Position -= 25L;
			this.Data = input.ReadBytes((int)(25U + num2));
		}
	}
}
