using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Nixson.Prototype.Tod.Prototype1
{
	// Token: 0x02000014 RID: 20
	[Tod("proto::TransformationDescription", PrototypeGame.P1)]
	public class TransformationDescription : MetaObject
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000BB RID: 187 RVA: 0x000030CD File Offset: 0x000012CD
		// (set) Token: 0x060000BC RID: 188 RVA: 0x000030D5 File Offset: 0x000012D5
		public string Name { get; set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000BD RID: 189 RVA: 0x000030DE File Offset: 0x000012DE
		// (set) Token: 0x060000BE RID: 190 RVA: 0x000030E6 File Offset: 0x000012E6
		public string PackageName { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000BF RID: 191 RVA: 0x000030EF File Offset: 0x000012EF
		// (set) Token: 0x060000C0 RID: 192 RVA: 0x000030F7 File Offset: 0x000012F7
		public string ArmsCompositeDrawableName { get; set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000C1 RID: 193 RVA: 0x00003100 File Offset: 0x00001300
		// (set) Token: 0x060000C2 RID: 194 RVA: 0x00003108 File Offset: 0x00001308
		public string BodyTemplateName { get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000C3 RID: 195 RVA: 0x00003111 File Offset: 0x00001311
		// (set) Token: 0x060000C4 RID: 196 RVA: 0x00003119 File Offset: 0x00001319
		public string UnknownName { get; set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000C5 RID: 197 RVA: 0x00003122 File Offset: 0x00001322
		// (set) Token: 0x060000C6 RID: 198 RVA: 0x0000312A File Offset: 0x0000132A
		public string UnknownName2 { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00003133 File Offset: 0x00001333
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x0000313B File Offset: 0x0000133B
		public string PhysicsFactoryName { get; set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x00003144 File Offset: 0x00001344
		// (set) Token: 0x060000CA RID: 202 RVA: 0x0000314C File Offset: 0x0000134C
		public string SupportingLimbName { get; set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000CB RID: 203 RVA: 0x00003155 File Offset: 0x00001355
		// (set) Token: 0x060000CC RID: 204 RVA: 0x0000315D File Offset: 0x0000135D
		public string PowerName { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000CD RID: 205 RVA: 0x00003166 File Offset: 0x00001366
		// (set) Token: 0x060000CE RID: 206 RVA: 0x0000316E File Offset: 0x0000136E
		public string Faction { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000CF RID: 207 RVA: 0x00003177 File Offset: 0x00001377
		// (set) Token: 0x060000D0 RID: 208 RVA: 0x0000317F File Offset: 0x0000137F
		public float DamageMultiplier { get; set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x00003188 File Offset: 0x00001388
		// (set) Token: 0x060000D2 RID: 210 RVA: 0x00003190 File Offset: 0x00001390
		public float DamageReceived { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x00003199 File Offset: 0x00001399
		// (set) Token: 0x060000D4 RID: 212 RVA: 0x000031A1 File Offset: 0x000013A1
		public float DamageToAttacker { get; set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x000031AA File Offset: 0x000013AA
		// (set) Token: 0x060000D6 RID: 214 RVA: 0x000031B2 File Offset: 0x000013B2
		public int TransformationIndex { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060000D7 RID: 215 RVA: 0x000031BB File Offset: 0x000013BB
		// (set) Token: 0x060000D8 RID: 216 RVA: 0x000031C3 File Offset: 0x000013C3
		public string LeftArmTemplateName { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x000031CC File Offset: 0x000013CC
		// (set) Token: 0x060000DA RID: 218 RVA: 0x000031D4 File Offset: 0x000013D4
		public bool IsOneHanded { get; set; }

		// Token: 0x060000DB RID: 219 RVA: 0x000031E0 File Offset: 0x000013E0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.Name, endian);
			output.WriteStringAlignedU8(this.PackageName);
			output.WriteStringU32NoTerminator(this.ArmsCompositeDrawableName, endian);
			output.WriteStringU32NoTerminator(this.BodyTemplateName, endian);
			output.WriteStringU32NoTerminator(this.UnknownName, endian);
			output.WriteStringAlignedU8(this.UnknownName2);
			output.WriteStringAlignedU8(this.PhysicsFactoryName);
			output.WriteStringAlignedU8(this.SupportingLimbName);
			output.WriteStringAlignedU8(this.PowerName);
			output.WriteStringU32NoTerminator(this.Faction, endian);
			output.WriteValueF32(this.DamageMultiplier, endian);
			output.WriteValueF32(this.DamageReceived, endian);
			output.WriteValueF32(this.DamageToAttacker, endian);
			output.WriteValueS32(this.TransformationIndex, endian);
			output.WriteStringU32NoTerminator(this.LeftArmTemplateName, endian);
			output.WriteValueB32(this.IsOneHanded, endian);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x000032B8 File Offset: 0x000014B8
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadString(input.ReadValueS32(endian), false);
			this.PackageName = input.ReadStringAlignedU8();
			this.ArmsCompositeDrawableName = input.ReadString(input.ReadValueS32(endian), false);
			this.BodyTemplateName = input.ReadString(input.ReadValueS32(endian), false);
			this.UnknownName = input.ReadString(input.ReadValueS32(endian), false);
			this.UnknownName2 = input.ReadStringAlignedU8();
			this.PhysicsFactoryName = input.ReadStringAlignedU8();
			this.SupportingLimbName = input.ReadStringAlignedU8();
			this.PowerName = input.ReadStringAlignedU8();
			this.Faction = input.ReadString(input.ReadValueS32(endian), false);
			this.DamageMultiplier = input.ReadValueF32(endian);
			this.DamageReceived = input.ReadValueF32(endian);
			this.DamageToAttacker = input.ReadValueF32(endian);
			this.TransformationIndex = input.ReadValueS32(endian);
			this.LeftArmTemplateName = input.ReadString(input.ReadValueS32(endian), false);
			this.IsOneHanded = input.ReadValueB32(endian);
		}
	}
}
