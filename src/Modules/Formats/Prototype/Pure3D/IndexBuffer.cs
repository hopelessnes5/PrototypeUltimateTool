using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000034 RID: 52
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65555U)]
	public class IndexBuffer : BaseNode
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x00006DF8 File Offset: 0x00004FF8
		// (set) Token: 0x060001C7 RID: 455 RVA: 0x00006E00 File Offset: 0x00005000
		public uint Version { get; set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x00006E09 File Offset: 0x00005009
		// (set) Token: 0x060001C9 RID: 457 RVA: 0x00006E11 File Offset: 0x00005011
		public uint Param { get; set; }

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x060001CA RID: 458 RVA: 0x00006E1A File Offset: 0x0000501A
		// (set) Token: 0x060001CB RID: 459 RVA: 0x00006E22 File Offset: 0x00005022
		public uint BufferSize { get; set; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060001CC RID: 460 RVA: 0x00006E2B File Offset: 0x0000502B
		// (set) Token: 0x060001CD RID: 461 RVA: 0x00006E33 File Offset: 0x00005033
		public Face[] Faces { get; set; }

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060001CE RID: 462 RVA: 0x00003219 File Offset: 0x00001419
		public override bool Importable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00006E3C File Offset: 0x0000503C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Param, endian);
			output.WriteValueU32(this.BufferSize, endian);
			for (int i = 0; i < this.Faces.Length; i++)
			{
				this.Faces[i].Serialize(output, endian);
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00006E94 File Offset: 0x00005094
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Param = input.ReadValueU32(endian);
			this.BufferSize = input.ReadValueU32(endian);
			if (this.BufferSize % 6U != 0U || this.BufferSize / 6U > 2000000U || this.BufferSize > int.MaxValue ||
				(input.CanSeek && (long)this.BufferSize > input.Length - input.Position))
				throw new FormatException("IndexBuffer byte count exceeds the remaining chunk payload.");
			uint num = this.BufferSize / 6U;
			this.Faces = new Face[(int)num];
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				this.Faces[num2] = new Face(input, endian);
				num2++;
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00006EFC File Offset: 0x000050FC
		public override void Import(Stream input)
		{
			using (StreamReader streamReader = new StreamReader(input))
			{
				for (int i = 0; i < 1666; i++)
				{
					string[] array = streamReader.ReadLine().Split(new char[]
					{
						','
					});
					this.Faces[i].Point1 = ushort.Parse(array[0]);
					this.Faces[i].Point2 = ushort.Parse(array[1]);
					this.Faces[i].Point3 = ushort.Parse(array[2]);
				}
			}
		}
	}
}
