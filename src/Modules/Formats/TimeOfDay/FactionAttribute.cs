using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x02000003 RID: 3
	public class FactionAttribute : MetaAttribute
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000006 RID: 6 RVA: 0x0000208F File Offset: 0x0000028F
		// (set) Token: 0x06000007 RID: 7 RVA: 0x00002097 File Offset: 0x00000297
		public string Faction { get; set; }

		// Token: 0x06000008 RID: 8 RVA: 0x000020A0 File Offset: 0x000002A0
		public override void Serialize(Stream output, Endian endian)
		{
			base.Serialize(output, endian);
			output.WriteStringU32NoTerminator(this.Faction, endian);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020B7 File Offset: 0x000002B7
		public override void Deserialize(Stream input, Endian endian)
		{
			base.Deserialize(input, endian);
			this.Faction = input.ReadString(input.ReadValueS32(endian), false);
		}
	}
}
