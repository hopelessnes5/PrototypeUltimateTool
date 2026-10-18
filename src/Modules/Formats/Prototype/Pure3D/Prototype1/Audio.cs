using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats.Pure3D.Sound;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype1
{
	// Token: 0x02000083 RID: 131
	[KnownGame(PrototypeGame.P1)]
	[KnownType(4261412864U)]
	public class Audio : BaseNode
	{
		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x0000BF66 File Offset: 0x0000A166
		// (set) Token: 0x06000554 RID: 1364 RVA: 0x0000BF6E File Offset: 0x0000A16E
		public uint Unknown1 { get; set; }

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000555 RID: 1365 RVA: 0x0000BF77 File Offset: 0x0000A177
		// (set) Token: 0x06000556 RID: 1366 RVA: 0x0000BF7F File Offset: 0x0000A17F
		[ReadOnly(true)]
		public string FileType { get; set; }

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000557 RID: 1367 RVA: 0x0000BF88 File Offset: 0x0000A188
		// (set) Token: 0x06000558 RID: 1368 RVA: 0x0000BF90 File Offset: 0x0000A190
		public uint Unknown2 { get; set; }

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000559 RID: 1369 RVA: 0x0000BF99 File Offset: 0x0000A199
		// (set) Token: 0x0600055A RID: 1370 RVA: 0x0000BFA1 File Offset: 0x0000A1A1
		public string Name { get; set; }

		// Token: 0x0600055B RID: 1371 RVA: 0x0000BFAC File Offset: 0x0000A1AC
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return string.Concat(new string[]
			{
				base.ToString(),
				" (",
				this.Name.Trim(new char[1]),
				") (",
				this.FileType.Trim(new char[1]),
				")"
			});
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x0600055C RID: 1372 RVA: 0x0000C021 File Offset: 0x0000A221
		public override bool Exportable
		{
			get
			{
				return this.FileType == "AudioFile\0";
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x0600055D RID: 1373 RVA: 0x0000C033 File Offset: 0x0000A233
		public override string ExportExtension
		{
			get
			{
				return "radp";
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0000C03C File Offset: 0x0000A23C
		public override void Export(Stream output)
		{
			if (!this.Exportable)
			{
				return;
			}
			AudioFile audioFile = (AudioFile)this.AudioType;
			output.Write(audioFile.Data, 0, audioFile.Data.Length);
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0000C074 File Offset: 0x0000A274
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32((uint)(this.FileType.Length - 1), endian);
			output.WriteString(this.FileType);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteValueU32((uint)(this.Name.Length - 1), endian);
			output.WriteString(this.Name);
			this.AudioType.Serialize(output, endian);
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0000C0E8 File Offset: 0x0000A2E8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			uint num = input.ReadValueU32(endian);
			this.FileType = input.ReadString((int)(num + 1U));
			this.Unknown2 = input.ReadValueU32(endian);
			num = input.ReadValueU32(endian);
			this.Name = input.ReadString((int)(num + 1U));
			if (this.FileType == "AudioFile\0")
			{
				this.AudioType = new AudioFile(input, endian);
				return;
			}
			int length = (int)((ulong)(base.StartPosition + base.TotalSize) - (ulong)input.Position);
			this.AudioType = new AudioTypeUnknown(input, endian, length);
		}

		// Token: 0x040001F0 RID: 496
		public AudioType AudioType;
	}
}
