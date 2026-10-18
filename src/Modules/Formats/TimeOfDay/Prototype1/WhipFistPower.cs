using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000019 RID: 25
	[Tod("proto::WhipFistPower", PrototypeGame.P1)]
	public class WhipFistPower : MetaObject
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00003607 File Offset: 0x00001807
		// (set) Token: 0x060000F8 RID: 248 RVA: 0x0000360F File Offset: 0x0000180F
		public int UnknownInt1 { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x00003618 File Offset: 0x00001818
		// (set) Token: 0x060000FA RID: 250 RVA: 0x00003620 File Offset: 0x00001820
		public float UnknownFloat1 { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060000FB RID: 251 RVA: 0x00003629 File Offset: 0x00001829
		// (set) Token: 0x060000FC RID: 252 RVA: 0x00003631 File Offset: 0x00001831
		public string WhipfistSegment { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060000FD RID: 253 RVA: 0x0000363A File Offset: 0x0000183A
		// (set) Token: 0x060000FE RID: 254 RVA: 0x00003642 File Offset: 0x00001842
		public string UnknownJoint1 { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060000FF RID: 255 RVA: 0x0000364B File Offset: 0x0000184B
		// (set) Token: 0x06000100 RID: 256 RVA: 0x00003653 File Offset: 0x00001853
		public string UnknownJoint2 { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000101 RID: 257 RVA: 0x0000365C File Offset: 0x0000185C
		// (set) Token: 0x06000102 RID: 258 RVA: 0x00003664 File Offset: 0x00001864
		public string WhipfistEnd { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000103 RID: 259 RVA: 0x0000366D File Offset: 0x0000186D
		// (set) Token: 0x06000104 RID: 260 RVA: 0x00003675 File Offset: 0x00001875
		public float UnknownFloat2 { get; set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000105 RID: 261 RVA: 0x0000367E File Offset: 0x0000187E
		// (set) Token: 0x06000106 RID: 262 RVA: 0x00003686 File Offset: 0x00001886
		public float UnknownFloat3 { get; set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000107 RID: 263 RVA: 0x0000368F File Offset: 0x0000188F
		// (set) Token: 0x06000108 RID: 264 RVA: 0x00003697 File Offset: 0x00001897
		public float UnknownFloat4 { get; set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000109 RID: 265 RVA: 0x000036A0 File Offset: 0x000018A0
		// (set) Token: 0x0600010A RID: 266 RVA: 0x000036A8 File Offset: 0x000018A8
		public int UnknownInt2 { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600010B RID: 267 RVA: 0x000036B1 File Offset: 0x000018B1
		// (set) Token: 0x0600010C RID: 268 RVA: 0x000036B9 File Offset: 0x000018B9
		public int UnknownInt3 { get; set; }

		// Token: 0x0600010D RID: 269 RVA: 0x000036C4 File Offset: 0x000018C4
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32(this.UnknownInt1, endian);
			output.WriteValueF32(this.UnknownFloat1, endian);
			output.WriteStringAlignedU8(this.WhipfistSegment);
			output.WriteStringU32NoTerminator(this.UnknownJoint1, endian);
			output.WriteStringU32NoTerminator(this.UnknownJoint2, endian);
			output.WriteStringU32NoTerminator(this.WhipfistEnd, endian);
			output.WriteValueF32(this.UnknownFloat2, endian);
			output.WriteValueF32(this.UnknownFloat3, endian);
			output.WriteValueF32(this.UnknownFloat4, endian);
			output.WriteValueS32(this.UnknownInt2, endian);
			output.WriteValueS32(this.UnknownInt3, endian);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00003760 File Offset: 0x00001960
		public override void Deserialize(Stream input, Endian endian)
		{
			this.UnknownInt1 = input.ReadValueS32(endian);
			this.UnknownFloat1 = input.ReadValueF32(endian);
			this.WhipfistSegment = input.ReadStringAlignedU8();
			this.UnknownJoint1 = input.ReadString(input.ReadValueS32(endian));
			this.UnknownJoint2 = input.ReadString(input.ReadValueS32(endian));
			this.WhipfistEnd = input.ReadString(input.ReadValueS32(endian));
			this.UnknownFloat2 = input.ReadValueF32(endian);
			this.UnknownFloat3 = input.ReadValueF32(endian);
			this.UnknownFloat4 = input.ReadValueF32(endian);
			this.UnknownInt2 = input.ReadValueS32(endian);
			this.UnknownInt3 = input.ReadValueS32(endian);
		}
	}
}
