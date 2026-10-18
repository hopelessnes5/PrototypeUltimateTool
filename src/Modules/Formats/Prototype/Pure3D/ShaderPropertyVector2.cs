using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000057 RID: 87
	[KnownGame(PrototypeGame.Any)]
	[KnownType(69656U)]
	public class ShaderPropertyVector2 : BaseNode
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060003A0 RID: 928 RVA: 0x00009789 File Offset: 0x00007989
		// (set) Token: 0x060003A1 RID: 929 RVA: 0x00009791 File Offset: 0x00007991
		public string Name { get; set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x060003A2 RID: 930 RVA: 0x0000979A File Offset: 0x0000799A
		// (set) Token: 0x060003A3 RID: 931 RVA: 0x000097A2 File Offset: 0x000079A2
		public Vector2 Value { get; set; }

		// Token: 0x060003A4 RID: 932 RVA: 0x000097AB File Offset: 0x000079AB
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x000097E7 File Offset: 0x000079E7
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			this.Value.Serialize(output, endian);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00009802 File Offset: 0x00007A02
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Value = new Vector2(input, endian);
		}
	}
}
