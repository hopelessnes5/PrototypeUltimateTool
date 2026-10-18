using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000075 RID: 117
	public class Cue : AudioType
	{
		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x0000AE66 File Offset: 0x00009066
		// (set) Token: 0x060004A3 RID: 1187 RVA: 0x0000AE6E File Offset: 0x0000906E
		public uint Unknown1 { get; set; }

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x0000AE77 File Offset: 0x00009077
		// (set) Token: 0x060004A5 RID: 1189 RVA: 0x0000AE7F File Offset: 0x0000907F
		public uint NumberOfEntries { get; set; }

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x0000AE88 File Offset: 0x00009088
		// (set) Token: 0x060004A7 RID: 1191 RVA: 0x0000AE90 File Offset: 0x00009090
		public uint Unknown2 { get; set; }

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060004A8 RID: 1192 RVA: 0x0000AE99 File Offset: 0x00009099
		// (set) Token: 0x060004A9 RID: 1193 RVA: 0x0000AEA1 File Offset: 0x000090A1
		public byte[] Unknown3 { get; set; }

		// Token: 0x060004AA RID: 1194 RVA: 0x0000AD2D File Offset: 0x00008F2D
		public Cue()
		{
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0000AD35 File Offset: 0x00008F35
		public Cue(Stream input, Endian endian) : base(input, endian)
		{
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x0000A320 File Offset: 0x00008520
		public override void Serialize(Stream output, Endian endian)
		{
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x0000A320 File Offset: 0x00008520
		public override void Deserialize(Stream input, Endian endian)
		{
		}
	}
}
