using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002F RID: 47
	[KnownGame(PrototypeGame.P1)]
	[KnownType(1191937U)]
	public class CompositeDrawablePolySkinReference : BaseNode
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600018A RID: 394 RVA: 0x000068D3 File Offset: 0x00004AD3
		// (set) Token: 0x0600018B RID: 395 RVA: 0x000068DB File Offset: 0x00004ADB
		public uint Unknown1 { get; set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600018C RID: 396 RVA: 0x000068E4 File Offset: 0x00004AE4
		// (set) Token: 0x0600018D RID: 397 RVA: 0x000068EC File Offset: 0x00004AEC
		public uint Unknown2 { get; set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600018E RID: 398 RVA: 0x000068F5 File Offset: 0x00004AF5
		// (set) Token: 0x0600018F RID: 399 RVA: 0x000068FD File Offset: 0x00004AFD
		public string PolySkinName { get; set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000190 RID: 400 RVA: 0x00006906 File Offset: 0x00004B06
		// (set) Token: 0x06000191 RID: 401 RVA: 0x0000690E File Offset: 0x00004B0E
		public uint Unknown4 { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000192 RID: 402 RVA: 0x00006917 File Offset: 0x00004B17
		// (set) Token: 0x06000193 RID: 403 RVA: 0x0000691F File Offset: 0x00004B1F
		public uint Unknown5 { get; set; }

		// Token: 0x06000194 RID: 404 RVA: 0x00006928 File Offset: 0x00004B28
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Unknown2, endian);
			output.WriteStringAlignedU8(this.PolySkinName);
			output.WriteValueU32(this.Unknown4, endian);
			output.WriteValueU32(this.Unknown5, endian);
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00006978 File Offset: 0x00004B78
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Unknown1 = input.ReadValueU32(endian);
			this.Unknown2 = input.ReadValueU32(endian);
			this.PolySkinName = input.ReadStringAlignedU8();
			this.Unknown4 = input.ReadValueU32(endian);
			this.Unknown5 = input.ReadValueU32(endian);
		}
	}
}
