using System;
using System.IO;
using System.Threading;

namespace Gibbed.IO
{
	// Token: 0x02000007 RID: 7
	internal static class SmallWorkBuffer
	{
		// Token: 0x0600003E RID: 62 RVA: 0x000025DF File Offset: 0x000007DF
		public static byte[] Get(int count)
		{
			if (count < 0 || count > 8)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			return SmallWorkBuffer._SmallWorkBuffer.Value;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002600 File Offset: 0x00000800
		public static byte[] ReadBytes(Stream stream, int count)
		{
			if (count < 0 || count > 8)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			byte[] value = SmallWorkBuffer._SmallWorkBuffer.Value;
			stream.ReadExactly(value, 0, count);
			return value;
		}

		// Token: 0x04000008 RID: 8
		public const int BufferSize = 8;

		// Token: 0x04000009 RID: 9
		private static readonly ThreadLocal<byte[]> _SmallWorkBuffer = new ThreadLocal<byte[]>(() => new byte[8]);
	}
}
