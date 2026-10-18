using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000017 RID: 23
	public class UnlockableInfo : ISerializable
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060000DE RID: 222 RVA: 0x000033C2 File Offset: 0x000015C2
		// (set) Token: 0x060000DF RID: 223 RVA: 0x000033CA File Offset: 0x000015CA
		public Unlockable Unlockable { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x000033D3 File Offset: 0x000015D3
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x000033DB File Offset: 0x000015DB
		public string HudCode { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x000033E4 File Offset: 0x000015E4
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x000033EC File Offset: 0x000015EC
		public int Unknown1 { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x000033F5 File Offset: 0x000015F5
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x000033FD File Offset: 0x000015FD
		public int Cost { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00003406 File Offset: 0x00001606
		// (set) Token: 0x060000E7 RID: 231 RVA: 0x0000340E File Offset: 0x0000160E
		public string Mission { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x00003417 File Offset: 0x00001617
		// (set) Token: 0x060000E9 RID: 233 RVA: 0x0000341F File Offset: 0x0000161F
		public Unlockable Requirement { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00003428 File Offset: 0x00001628
		// (set) Token: 0x060000EB RID: 235 RVA: 0x00003430 File Offset: 0x00001630
		public bool IsCore { get; set; }

		// Token: 0x060000EC RID: 236 RVA: 0x00003439 File Offset: 0x00001639
		public UnlockableInfo()
		{
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00003441 File Offset: 0x00001641
		public UnlockableInfo(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00003454 File Offset: 0x00001654
		public void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32((int)(this.Unlockable + 1), endian);
			output.WriteStringU32NoTerminator(this.HudCode, endian);
			output.WriteValueS32(this.Unknown1, endian);
			output.WriteValueS32(this.Cost, endian);
			output.WriteStringU32NoTerminator(this.Mission, endian);
			output.WriteValueS32((int)(this.Requirement + 1), endian);
			output.WriteValueB32(this.IsCore, endian);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x000034C0 File Offset: 0x000016C0
		public void Deserialize(Stream input, Endian endian)
		{
			this.Unlockable = (Unlockable)(input.ReadValueS32(endian) - 1);
			this.HudCode = input.ReadString(input.ReadValueS32(endian));
			this.Unknown1 = input.ReadValueS32(endian);
			this.Cost = input.ReadValueS32(endian);
			this.Mission = input.ReadString(input.ReadValueS32(endian));
			this.Requirement = (Unlockable)(input.ReadValueS32(endian) - 1);
			this.IsCore = input.ReadValueB32(endian);
		}
	}
}
