using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000074 RID: 116
	public class AudioTypeUnknown : AudioType
	{
		// Token: 0x17000176 RID: 374
		// (get) Token: 0x0600049C RID: 1180 RVA: 0x0000AE1A File Offset: 0x0000901A
		// (set) Token: 0x0600049D RID: 1181 RVA: 0x0000AE22 File Offset: 0x00009022
		public byte[] Data { get; set; }

		// Token: 0x0600049E RID: 1182 RVA: 0x0000AD2D File Offset: 0x00008F2D
		public AudioTypeUnknown()
		{
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0000AE2B File Offset: 0x0000902B
		public AudioTypeUnknown(Stream input, Endian endian, int length) : base(input, endian)
		{
			this.Length = length;
			this.Deserialize(input, endian);
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x0000AE44 File Offset: 0x00009044
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Data);
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x0000AE52 File Offset: 0x00009052
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Data = input.ReadBytes(this.Length);
		}

		// Token: 0x040001B1 RID: 433
		public int Length;
	}
}
