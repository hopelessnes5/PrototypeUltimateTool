using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200002A RID: 42
	[KnownType(94214U)]
	public class Billboard : BaseNode
	{
		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000149 RID: 329 RVA: 0x00006424 File Offset: 0x00004624
		// (set) Token: 0x0600014A RID: 330 RVA: 0x0000642C File Offset: 0x0000462C
		public uint Version { get; set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600014B RID: 331 RVA: 0x00006435 File Offset: 0x00004635
		// (set) Token: 0x0600014C RID: 332 RVA: 0x0000643D File Offset: 0x0000463D
		public string Name { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600014D RID: 333 RVA: 0x00006446 File Offset: 0x00004646
		// (set) Token: 0x0600014E RID: 334 RVA: 0x0000644E File Offset: 0x0000464E
		public string NewShaderName { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600014F RID: 335 RVA: 0x00006457 File Offset: 0x00004657
		// (set) Token: 0x06000150 RID: 336 RVA: 0x0000645F File Offset: 0x0000465F
		public uint CutOffEnabled { get; set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000151 RID: 337 RVA: 0x00006468 File Offset: 0x00004668
		// (set) Token: 0x06000152 RID: 338 RVA: 0x00006470 File Offset: 0x00004670
		public uint ZTest { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000153 RID: 339 RVA: 0x00006479 File Offset: 0x00004679
		// (set) Token: 0x06000154 RID: 340 RVA: 0x00006481 File Offset: 0x00004681
		public uint ZWrite { get; set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000155 RID: 341 RVA: 0x0000648A File Offset: 0x0000468A
		// (set) Token: 0x06000156 RID: 342 RVA: 0x00006492 File Offset: 0x00004692
		public uint OcclusionCulling { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000157 RID: 343 RVA: 0x0000649B File Offset: 0x0000469B
		// (set) Token: 0x06000158 RID: 344 RVA: 0x000064A3 File Offset: 0x000046A3
		public uint NumQuads { get; set; }

		// Token: 0x06000159 RID: 345 RVA: 0x000064AC File Offset: 0x000046AC
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000064E0 File Offset: 0x000046E0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteStringAlignedU8(this.NewShaderName);
			output.WriteValueU32(this.CutOffEnabled, endian);
			output.WriteValueU32(this.ZTest, endian);
			output.WriteValueU32(this.ZWrite, endian);
			output.WriteValueU32(this.OcclusionCulling, endian);
			output.WriteValueU32(this.NumQuads, endian);
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00006554 File Offset: 0x00004754
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.NewShaderName = input.ReadStringAlignedU8();
			this.CutOffEnabled = input.ReadValueU32(endian);
			this.ZTest = input.ReadValueU32(endian);
			this.ZWrite = input.ReadValueU32(endian);
			this.OcclusionCulling = input.ReadValueU32(endian);
			this.NumQuads = input.ReadValueU32(endian);
		}
	}
}
