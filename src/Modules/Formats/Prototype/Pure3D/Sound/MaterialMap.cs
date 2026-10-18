using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000077 RID: 119
	public class MaterialMap : AudioType
	{
		// Token: 0x1700017F RID: 383
		// (get) Token: 0x060004BA RID: 1210 RVA: 0x0000AFBF File Offset: 0x000091BF
		// (set) Token: 0x060004BB RID: 1211 RVA: 0x0000AFC7 File Offset: 0x000091C7
		public uint NumberOfMaterials { get; set; }

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x060004BC RID: 1212 RVA: 0x0000AFD0 File Offset: 0x000091D0
		// (set) Token: 0x060004BD RID: 1213 RVA: 0x0000AFD8 File Offset: 0x000091D8
		public Material[] Materials { get; set; }

		// Token: 0x060004BE RID: 1214 RVA: 0x0000AD2D File Offset: 0x00008F2D
		public MaterialMap()
		{
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0000AD35 File Offset: 0x00008F35
		public MaterialMap(Stream input, Endian endian) : base(input, endian)
		{
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0000AFE4 File Offset: 0x000091E4
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.NumberOfMaterials, endian);
			int num = 0;
			while ((long)num < (long)((ulong)this.NumberOfMaterials))
			{
				this.Materials[num].Serialize(output, endian);
				num++;
			}
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0000B020 File Offset: 0x00009220
		public override void Deserialize(Stream input, Endian endian)
		{
			this.NumberOfMaterials = input.ReadValueU32(endian);
			if (this.NumberOfMaterials > 100000U || this.NumberOfMaterials > int.MaxValue ||
				(input.CanSeek && (long)this.NumberOfMaterials > (input.Length - input.Position) / 28L))
				throw new FormatException("Sound MaterialMap count exceeds the remaining chunk payload.");
			this.Materials = new Material[(int)this.NumberOfMaterials];
			int num = 0;
			while ((long)num < (long)((ulong)this.NumberOfMaterials))
			{
				this.Materials[num] = new Material(input, endian);
				num++;
			}
		}
	}
}
