using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000012 RID: 18
	[Tod("proto::ShieldPower", PrototypeGame.P1)]
	public class ShieldPower : MetaObject
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00002D33 File Offset: 0x00000F33
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00002D3B File Offset: 0x00000F3B
		public string ShieldJoint { get; set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000093 RID: 147 RVA: 0x00002D44 File Offset: 0x00000F44
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00002D4C File Offset: 0x00000F4C
		public float DeployedScale { get; set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00002D55 File Offset: 0x00000F55
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00002D5D File Offset: 0x00000F5D
		public float DeploySpeed { get; set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00002D66 File Offset: 0x00000F66
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00002D6E File Offset: 0x00000F6E
		public float RetractSpeed { get; set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000099 RID: 153 RVA: 0x00002D77 File Offset: 0x00000F77
		// (set) Token: 0x0600009A RID: 154 RVA: 0x00002D7F File Offset: 0x00000F7F
		public float MaxHealth { get; set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600009B RID: 155 RVA: 0x00002D88 File Offset: 0x00000F88
		// (set) Token: 0x0600009C RID: 156 RVA: 0x00002D90 File Offset: 0x00000F90
		public float Unknown1 { get; set; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600009D RID: 157 RVA: 0x00002D99 File Offset: 0x00000F99
		// (set) Token: 0x0600009E RID: 158 RVA: 0x00002DA1 File Offset: 0x00000FA1
		public float Unknown2 { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600009F RID: 159 RVA: 0x00002DAA File Offset: 0x00000FAA
		// (set) Token: 0x060000A0 RID: 160 RVA: 0x00002DB2 File Offset: 0x00000FB2
		public float Unknown3 { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00002DBB File Offset: 0x00000FBB
		// (set) Token: 0x060000A2 RID: 162 RVA: 0x00002DC3 File Offset: 0x00000FC3
		public float Unknown4 { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x00002DCC File Offset: 0x00000FCC
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x00002DD4 File Offset: 0x00000FD4
		public string CollisionTemplate { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x00002DDD File Offset: 0x00000FDD
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x00002DE5 File Offset: 0x00000FE5
		public string GameObjectTemplate { get; set; }

		// Token: 0x060000A7 RID: 167 RVA: 0x00002DF0 File Offset: 0x00000FF0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.ShieldJoint, endian);
			output.WriteValueF32(this.DeployedScale, endian);
			output.WriteValueF32(this.DeploySpeed, endian);
			output.WriteValueF32(this.RetractSpeed, endian);
			output.WriteValueF32(this.MaxHealth, endian);
			output.WriteValueF32(this.Unknown1, endian);
			output.WriteValueF32(this.Unknown2, endian);
			output.WriteValueF32(this.Unknown3, endian);
			output.WriteStringU32NoTerminator(this.CollisionTemplate, endian);
			output.WriteStringU32NoTerminator(this.GameObjectTemplate, endian);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00002E80 File Offset: 0x00001080
		public override void Deserialize(Stream input, Endian endian)
		{
			this.ShieldJoint = input.ReadString(input.ReadValueS32(endian));
			this.DeployedScale = input.ReadValueF32(endian);
			this.DeploySpeed = input.ReadValueF32(endian);
			this.RetractSpeed = input.ReadValueF32(endian);
			this.MaxHealth = input.ReadValueF32(endian);
			this.Unknown1 = input.ReadValueF32(endian);
			this.Unknown2 = input.ReadValueF32(endian);
			this.Unknown3 = input.ReadValueF32(endian);
			this.CollisionTemplate = input.ReadString(input.ReadValueS32(endian));
			this.GameObjectTemplate = input.ReadString(input.ReadValueS32(endian));
		}
	}
}
