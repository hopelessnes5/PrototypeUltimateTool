using System;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200006E RID: 110
	public class VertexAnimBuffer
	{
		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000469 RID: 1129 RVA: 0x0000A9B7 File Offset: 0x00008BB7
		// (set) Token: 0x0600046A RID: 1130 RVA: 0x0000A9BF File Offset: 0x00008BBF
		public uint Index { get; set; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x0600046B RID: 1131 RVA: 0x0000A9C8 File Offset: 0x00008BC8
		// (set) Token: 0x0600046C RID: 1132 RVA: 0x0000A9D0 File Offset: 0x00008BD0
		public byte[] Data { get; set; }
	}
}
