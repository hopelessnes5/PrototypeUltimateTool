using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005D RID: 93
	[KnownGame(PrototypeGame.Any)]
	[KnownType(1183744U)]
	public class Animation : BaseNode
	{
		// Token: 0x17000136 RID: 310
		// (get) Token: 0x060003D4 RID: 980 RVA: 0x00009B8A File Offset: 0x00007D8A
		// (set) Token: 0x060003D5 RID: 981 RVA: 0x00009B92 File Offset: 0x00007D92
		public uint Version { get; set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x00009B9B File Offset: 0x00007D9B
		// (set) Token: 0x060003D7 RID: 983 RVA: 0x00009BA3 File Offset: 0x00007DA3
		public string Name { get; set; }

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060003D8 RID: 984 RVA: 0x00009BAC File Offset: 0x00007DAC
		// (set) Token: 0x060003D9 RID: 985 RVA: 0x00009BB4 File Offset: 0x00007DB4
		public string AnimationType { get; set; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060003DA RID: 986 RVA: 0x00009BBD File Offset: 0x00007DBD
		// (set) Token: 0x060003DB RID: 987 RVA: 0x00009BC5 File Offset: 0x00007DC5
		public float NumFrames { get; set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060003DC RID: 988 RVA: 0x00009BCE File Offset: 0x00007DCE
		// (set) Token: 0x060003DD RID: 989 RVA: 0x00009BD6 File Offset: 0x00007DD6
		public float FrameRate { get; set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060003DE RID: 990 RVA: 0x00009BDF File Offset: 0x00007DDF
		// (set) Token: 0x060003DF RID: 991 RVA: 0x00009BE7 File Offset: 0x00007DE7
		public uint Cyclic { get; set; }

		// Token: 0x060003E0 RID: 992 RVA: 0x00009BF0 File Offset: 0x00007DF0
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00009C2C File Offset: 0x00007E2C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version, endian);
			output.WriteStringAlignedU8(this.Name);
			output.WriteString(this.AnimationType);
			output.WriteValueF32(this.NumFrames, endian);
			output.WriteValueF32(this.FrameRate, endian);
			output.WriteValueU32(this.Cyclic, endian);
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00009C88 File Offset: 0x00007E88
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.Name = input.ReadStringAlignedU8();
			this.AnimationType = input.ReadString(4);
			this.NumFrames = input.ReadValueF32(endian);
			this.FrameRate = input.ReadValueF32(endian);
			this.Cyclic = input.ReadValueU32(endian);
		}
	}
}
