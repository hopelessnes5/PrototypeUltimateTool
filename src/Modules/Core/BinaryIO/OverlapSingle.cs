using System;
using System.Runtime.InteropServices;

namespace Gibbed.IO
{
	// Token: 0x02000005 RID: 5
	[StructLayout(LayoutKind.Explicit)]
	internal struct OverlapSingle
	{
		// Token: 0x06000037 RID: 55 RVA: 0x0000247A File Offset: 0x0000067A
		public OverlapSingle(float f)
		{
			this.U = 0U;
			this.F = f;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000248A File Offset: 0x0000068A
		public OverlapSingle(uint u)
		{
			this.F = 0f;
			this.U = u;
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000039 RID: 57 RVA: 0x0000249E File Offset: 0x0000069E
		// (set) Token: 0x0600003A RID: 58 RVA: 0x000024A6 File Offset: 0x000006A6
		public float AsF
		{
			get
			{
				return this.F;
			}
			set
			{
				this.F = value;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600003B RID: 59 RVA: 0x000024AF File Offset: 0x000006AF
		// (set) Token: 0x0600003C RID: 60 RVA: 0x000024B7 File Offset: 0x000006B7
		public uint AsU
		{
			get
			{
				return this.U;
			}
			set
			{
				this.U = value;
			}
		}

		// Token: 0x04000006 RID: 6
		[FieldOffset(0)]
		private float F;

		// Token: 0x04000007 RID: 7
		[FieldOffset(0)]
		private uint U;
	}
}
