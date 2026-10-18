using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000076 RID: 118
	public class Material
	{
		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x0000AEAA File Offset: 0x000090AA
		// (set) Token: 0x060004AF RID: 1199 RVA: 0x0000AEB2 File Offset: 0x000090B2
		public byte[] Unknown1 { get; set; }

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060004B0 RID: 1200 RVA: 0x0000AEBB File Offset: 0x000090BB
		// (set) Token: 0x060004B1 RID: 1201 RVA: 0x0000AEC3 File Offset: 0x000090C3
		public string MaterialName { get; set; }

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060004B2 RID: 1202 RVA: 0x0000AECC File Offset: 0x000090CC
		// (set) Token: 0x060004B3 RID: 1203 RVA: 0x0000AED4 File Offset: 0x000090D4
		public byte[] Unknown2 { get; set; }

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060004B4 RID: 1204 RVA: 0x0000AEDD File Offset: 0x000090DD
		// (set) Token: 0x060004B5 RID: 1205 RVA: 0x0000AEE5 File Offset: 0x000090E5
		public string AudioClipName { get; set; }

		// Token: 0x060004B6 RID: 1206 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public Material()
		{
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0000AEEE File Offset: 0x000090EE
		public Material(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0000AF00 File Offset: 0x00009100
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Unknown1);
			output.WriteValueU32((uint)(this.MaterialName.Length - 1), endian);
			output.WriteString(this.MaterialName);
			output.WriteBytes(this.Unknown2);
			output.WriteValueU32((uint)(this.AudioClipName.Length - 1), endian);
			output.WriteString(this.AudioClipName);
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0000AF68 File Offset: 0x00009168
		public void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadBytes(9);
			uint num = input.ReadValueU32(endian);
			this.MaterialName = input.ReadString((int)(num + 1U));
			this.Unknown2 = input.ReadBytes(9);
			num = input.ReadValueU32(endian);
			this.AudioClipName = input.ReadString((int)(num + 1U));
		}
	}
}
