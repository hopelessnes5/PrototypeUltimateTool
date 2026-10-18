using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod
{
	// Token: 0x0200000C RID: 12
	public abstract class MetaObject
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003A RID: 58 RVA: 0x0000244A File Offset: 0x0000064A
		// (set) Token: 0x0600003B RID: 59 RVA: 0x00002452 File Offset: 0x00000652
		[Browsable(false)]
		public uint Length { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003C RID: 60 RVA: 0x0000245B File Offset: 0x0000065B
		// (set) Token: 0x0600003D RID: 61 RVA: 0x00002463 File Offset: 0x00000663
		public List<MetaAttribute> Attributes { get; set; } = new List<MetaAttribute>();

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600003E RID: 62 RVA: 0x0000246C File Offset: 0x0000066C
		[Browsable(false)]
		public virtual bool HasAttributeList
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000246F File Offset: 0x0000066F
		public virtual void Serialize(Stream output, Endian endian)
		{
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002471 File Offset: 0x00000671
		public virtual void Deserialize(Stream input, Endian endian)
		{
		}
	}
}
