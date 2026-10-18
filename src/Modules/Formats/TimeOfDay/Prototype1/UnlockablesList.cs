using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000018 RID: 24
	[Tod("proto::UnlockablesList", PrototypeGame.P1)]
	public class UnlockablesList : MetaObject
	{
		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x00003538 File Offset: 0x00001738
		// (set) Token: 0x060000F1 RID: 241 RVA: 0x00003540 File Offset: 0x00001740
		public UnlockableType UnlockableType { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x00003549 File Offset: 0x00001749
		// (set) Token: 0x060000F3 RID: 243 RVA: 0x00003551 File Offset: 0x00001751
		public List<UnlockableInfo> UnlockableList { get; set; } = new List<UnlockableInfo>();

		// Token: 0x060000F4 RID: 244 RVA: 0x0000355C File Offset: 0x0000175C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32((int)this.UnlockableType, endian);
			output.WriteValueS32(this.UnlockableList.Count, endian);
			for (int i = 0; i < this.UnlockableList.Count; i++)
			{
				this.UnlockableList[i].Serialize(output, endian);
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x000035B4 File Offset: 0x000017B4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.UnlockableType = (UnlockableType)input.ReadValueS32(endian);
			int num = input.ReadValueS32(endian);
			for (int i = 0; i < num; i++)
			{
				this.UnlockableList.Add(new UnlockableInfo(input, endian));
			}
		}
	}
}
