using System;
using System.Runtime.InteropServices;

namespace Gibbed.IO
{
	// Token: 0x02000004 RID: 4
	[StructLayout(LayoutKind.Explicit)]
	internal struct OverlapDouble
	{
		// Token: 0x06000031 RID: 49 RVA: 0x0000242F File Offset: 0x0000062F
		public OverlapDouble(double d)
		{
			this.U = 0UL;
			this.D = d;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002440 File Offset: 0x00000640
		public OverlapDouble(ulong u)
		{
			this.D = 0.0;
			this.U = u;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002458 File Offset: 0x00000658
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00002460 File Offset: 0x00000660
		public double AsD
		{
			get
			{
				return this.D;
			}
			set
			{
				this.D = value;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00002469 File Offset: 0x00000669
		// (set) Token: 0x06000036 RID: 54 RVA: 0x00002471 File Offset: 0x00000671
		public ulong AsU
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

		// Token: 0x04000004 RID: 4
		[FieldOffset(0)]
		private double D;

		// Token: 0x04000005 RID: 5
		[FieldOffset(0)]
		private ulong U;
	}
}
