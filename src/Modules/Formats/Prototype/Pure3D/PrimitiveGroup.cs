using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000054 RID: 84
	[KnownGame(PrototypeGame.P1)]
	[KnownType(65568U)]
	public class PrimitiveGroup : BaseNode
	{
		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000377 RID: 887 RVA: 0x00009471 File Offset: 0x00007671
		// (set) Token: 0x06000378 RID: 888 RVA: 0x00009479 File Offset: 0x00007679
		public uint Version { get; set; }

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000379 RID: 889 RVA: 0x00009482 File Offset: 0x00007682
		// (set) Token: 0x0600037A RID: 890 RVA: 0x0000948A File Offset: 0x0000768A
		public string ShaderName { get; set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x0600037B RID: 891 RVA: 0x00009493 File Offset: 0x00007693
		// (set) Token: 0x0600037C RID: 892 RVA: 0x0000949B File Offset: 0x0000769B
		public uint PrimitiveType { get; set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600037D RID: 893 RVA: 0x000094A4 File Offset: 0x000076A4
		// (set) Token: 0x0600037E RID: 894 RVA: 0x000094AC File Offset: 0x000076AC
		public uint VertexType { get; set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600037F RID: 895 RVA: 0x000094B5 File Offset: 0x000076B5
		// (set) Token: 0x06000380 RID: 896 RVA: 0x000094BD File Offset: 0x000076BD
		public uint NumVertices { get; set; }

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000381 RID: 897 RVA: 0x000094C6 File Offset: 0x000076C6
		// (set) Token: 0x06000382 RID: 898 RVA: 0x000094CE File Offset: 0x000076CE
		public uint NumIndices { get; set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000383 RID: 899 RVA: 0x000094D7 File Offset: 0x000076D7
		// (set) Token: 0x06000384 RID: 900 RVA: 0x000094DF File Offset: 0x000076DF
		public uint NumMatrices { get; set; }

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000385 RID: 901 RVA: 0x000094E8 File Offset: 0x000076E8
		// (set) Token: 0x06000386 RID: 902 RVA: 0x000094F0 File Offset: 0x000076F0
		public uint MemoryImaged { get; set; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000387 RID: 903 RVA: 0x000094F9 File Offset: 0x000076F9
		// (set) Token: 0x06000388 RID: 904 RVA: 0x00009501 File Offset: 0x00007701
		public uint Optimized { get; set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x06000389 RID: 905 RVA: 0x0000950A File Offset: 0x0000770A
		// (set) Token: 0x0600038A RID: 906 RVA: 0x00009512 File Offset: 0x00007712
		public uint VertexAnimated { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600038B RID: 907 RVA: 0x0000951B File Offset: 0x0000771B
		// (set) Token: 0x0600038C RID: 908 RVA: 0x00009523 File Offset: 0x00007723
		public uint VertexAnimationMask { get; set; }

		// Token: 0x0600038D RID: 909 RVA: 0x0000952C File Offset: 0x0000772C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.ShaderName);
			output.WriteValueU32(this.PrimitiveType, endian);
			output.WriteValueU32(this.VertexType, endian);
			output.WriteValueU32(this.NumVertices, endian);
			output.WriteValueU32(this.NumIndices, endian);
			output.WriteValueU32(this.NumMatrices, endian);
			output.WriteValueU32(this.MemoryImaged, endian);
			output.WriteValueU32(this.Optimized, endian);
			output.WriteValueU32(this.VertexAnimated, endian);
			output.WriteValueU32(this.VertexAnimationMask, endian);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x000095C8 File Offset: 0x000077C8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.ShaderName = input.ReadStringAlignedU8();
			this.PrimitiveType = input.ReadValueU32(endian);
			this.VertexType = input.ReadValueU32(endian);
			this.NumVertices = input.ReadValueU32(endian);
			this.NumIndices = input.ReadValueU32(endian);
			this.NumMatrices = input.ReadValueU32(endian);
			this.MemoryImaged = input.ReadValueU32(endian);
			this.Optimized = input.ReadValueU32(endian);
			this.VertexAnimated = input.ReadValueU32(endian);
			this.VertexAnimationMask = input.ReadValueU32(endian);
		}
	}
}
