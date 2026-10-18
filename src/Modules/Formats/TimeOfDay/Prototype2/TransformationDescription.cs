using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod.Prototype2
{
	// Token: 0x02000011 RID: 17
	public class TransformationDescription : MetaObject
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000062 RID: 98 RVA: 0x00002978 File Offset: 0x00000B78
		// (set) Token: 0x06000063 RID: 99 RVA: 0x00002980 File Offset: 0x00000B80
		public string Name { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000064 RID: 100 RVA: 0x00002989 File Offset: 0x00000B89
		// (set) Token: 0x06000065 RID: 101 RVA: 0x00002991 File Offset: 0x00000B91
		public int TransformationIndex { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000066 RID: 102 RVA: 0x0000299A File Offset: 0x00000B9A
		// (set) Token: 0x06000067 RID: 103 RVA: 0x000029A2 File Offset: 0x00000BA2
		public int UnknownInt1 { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000068 RID: 104 RVA: 0x000029AB File Offset: 0x00000BAB
		// (set) Token: 0x06000069 RID: 105 RVA: 0x000029B3 File Offset: 0x00000BB3
		public string PackageName { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600006A RID: 106 RVA: 0x000029BC File Offset: 0x00000BBC
		// (set) Token: 0x0600006B RID: 107 RVA: 0x000029C4 File Offset: 0x00000BC4
		public string ArmsCompositeDrawableName { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600006C RID: 108 RVA: 0x000029CD File Offset: 0x00000BCD
		// (set) Token: 0x0600006D RID: 109 RVA: 0x000029D5 File Offset: 0x00000BD5
		public int UnknownInt2 { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000029DE File Offset: 0x00000BDE
		// (set) Token: 0x0600006F RID: 111 RVA: 0x000029E6 File Offset: 0x00000BE6
		public string BodyTemplateName { get; set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000070 RID: 112 RVA: 0x000029EF File Offset: 0x00000BEF
		// (set) Token: 0x06000071 RID: 113 RVA: 0x000029F7 File Offset: 0x00000BF7
		public string UnknownName1 { get; set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000072 RID: 114 RVA: 0x00002A00 File Offset: 0x00000C00
		// (set) Token: 0x06000073 RID: 115 RVA: 0x00002A08 File Offset: 0x00000C08
		public string PowerClassName { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00002A11 File Offset: 0x00000C11
		// (set) Token: 0x06000075 RID: 117 RVA: 0x00002A19 File Offset: 0x00000C19
		public string Faction { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00002A22 File Offset: 0x00000C22
		// (set) Token: 0x06000077 RID: 119 RVA: 0x00002A2A File Offset: 0x00000C2A
		public float DamageMultiplier { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000078 RID: 120 RVA: 0x00002A33 File Offset: 0x00000C33
		// (set) Token: 0x06000079 RID: 121 RVA: 0x00002A3B File Offset: 0x00000C3B
		public float DamageReceived { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00002A44 File Offset: 0x00000C44
		// (set) Token: 0x0600007B RID: 123 RVA: 0x00002A4C File Offset: 0x00000C4C
		public string UnknownName2 { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600007C RID: 124 RVA: 0x00002A55 File Offset: 0x00000C55
		// (set) Token: 0x0600007D RID: 125 RVA: 0x00002A5D File Offset: 0x00000C5D
		public string UnknownName3 { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600007E RID: 126 RVA: 0x00002A66 File Offset: 0x00000C66
		// (set) Token: 0x0600007F RID: 127 RVA: 0x00002A6E File Offset: 0x00000C6E
		public string LeftArmTemplateName { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000080 RID: 128 RVA: 0x00002A77 File Offset: 0x00000C77
		// (set) Token: 0x06000081 RID: 129 RVA: 0x00002A7F File Offset: 0x00000C7F
		public string UnknownName4 { get; set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000082 RID: 130 RVA: 0x00002A88 File Offset: 0x00000C88
		// (set) Token: 0x06000083 RID: 131 RVA: 0x00002A90 File Offset: 0x00000C90
		public string UnknownName5 { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000084 RID: 132 RVA: 0x00002A99 File Offset: 0x00000C99
		// (set) Token: 0x06000085 RID: 133 RVA: 0x00002AA1 File Offset: 0x00000CA1
		public bool IsOneHanded { get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000086 RID: 134 RVA: 0x00002AAA File Offset: 0x00000CAA
		// (set) Token: 0x06000087 RID: 135 RVA: 0x00002AB2 File Offset: 0x00000CB2
		public int UnknownInt3 { get; set; }

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000088 RID: 136 RVA: 0x00002ABB File Offset: 0x00000CBB
		// (set) Token: 0x06000089 RID: 137 RVA: 0x00002AC3 File Offset: 0x00000CC3
		public int UnknownInt4 { get; set; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600008A RID: 138 RVA: 0x00002ACC File Offset: 0x00000CCC
		// (set) Token: 0x0600008B RID: 139 RVA: 0x00002AD4 File Offset: 0x00000CD4
		public string ShoulderPowerCapLeft { get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00002ADD File Offset: 0x00000CDD
		// (set) Token: 0x0600008D RID: 141 RVA: 0x00002AE5 File Offset: 0x00000CE5
		public string ShoulderPowerCapRight { get; set; }

		// Token: 0x0600008E RID: 142 RVA: 0x00002AF0 File Offset: 0x00000CF0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringU32NoTerminator(this.Name, endian);
			output.WriteValueS32(this.TransformationIndex, endian);
			output.WriteValueS32(this.UnknownInt1, endian);
			output.WriteStringAlignedU8(this.PackageName);
			output.WriteStringU32NoTerminator(this.ArmsCompositeDrawableName, endian);
			output.WriteValueS32(this.UnknownInt2, endian);
			output.WriteStringU32NoTerminator(this.BodyTemplateName, endian);
			output.WriteStringU32NoTerminator(this.UnknownName1, endian);
			output.WriteStringAlignedU8(this.PowerClassName);
			output.WriteStringU32NoTerminator(this.Faction, endian);
			output.WriteValueF32(this.DamageMultiplier, endian);
			output.WriteValueF32(this.DamageReceived, endian);
			output.WriteValueB32(this.IsOneHanded, endian);
			output.WriteStringAlignedU8(this.UnknownName2);
			output.WriteStringAlignedU8(this.UnknownName3);
			output.WriteStringU32NoTerminator(this.UnknownName2, endian);
			output.WriteValueS32(this.UnknownInt3, endian);
			output.WriteValueS32(this.UnknownInt4, endian);
			output.WriteStringU32NoTerminator(this.ShoulderPowerCapLeft, endian);
			output.WriteStringU32NoTerminator(this.ShoulderPowerCapRight, endian);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00002C00 File Offset: 0x00000E00
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadString(input.ReadValueS32(endian), false);
			this.TransformationIndex = input.ReadValueS32(endian);
			this.UnknownInt1 = input.ReadValueS32(endian);
			this.PackageName = input.ReadStringAlignedU8();
			this.ArmsCompositeDrawableName = input.ReadString(input.ReadValueS32(endian), false);
			this.UnknownInt2 = input.ReadValueS32(endian);
			this.BodyTemplateName = input.ReadString(input.ReadValueS32(endian), false);
			this.PowerClassName = input.ReadStringAlignedU8();
			this.Faction = input.ReadString(input.ReadValueS32(endian), false);
			this.DamageMultiplier = input.ReadValueF32(endian);
			this.DamageReceived = input.ReadValueF32(endian);
			this.UnknownName2 = input.ReadStringAlignedU8();
			this.UnknownName3 = input.ReadStringAlignedU8();
			this.LeftArmTemplateName = input.ReadString(input.ReadValueS32(endian), false);
			this.IsOneHanded = input.ReadValueB32(endian);
			this.UnknownName2 = input.ReadString(input.ReadValueS32(endian), false);
			this.ShoulderPowerCapLeft = input.ReadString(input.ReadValueS32(endian), false);
			this.ShoulderPowerCapRight = input.ReadString(input.ReadValueS32(endian), false);
		}
	}
}
