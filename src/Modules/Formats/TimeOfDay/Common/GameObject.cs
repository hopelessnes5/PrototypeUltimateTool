using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Nixson.Prototype.Tod.Common
{
	// Token: 0x0200001B RID: 27
	public class GameObject : MetaObject
	{
		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000117 RID: 279 RVA: 0x000039BC File Offset: 0x00001BBC
		// (set) Token: 0x06000118 RID: 280 RVA: 0x000039C4 File Offset: 0x00001BC4
		[Browsable(false)]
		public byte[] Data { get; set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000119 RID: 281 RVA: 0x000039CD File Offset: 0x00001BCD
		[Browsable(false)]
		public override bool HasAttributeList
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x000039D0 File Offset: 0x00001BD0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueS32(base.Attributes.Count, endian);
			foreach (MetaAttribute metaAttribute in base.Attributes)
			{
				metaAttribute.Serialize(output, endian);
			}
			output.WriteBytes(this.Data);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00003A40 File Offset: 0x00001C40
		public override void Deserialize(Stream input, Endian endian)
		{
			long position = input.Position;
			int num = input.ReadValueS32(endian);
			for (int i = 0; i < num; i++)
			{
				string attributeName = input.ReadString(input.ReadValueS32(endian), false);
				string text = input.ReadString(input.ReadValueS32(endian), false);
				MetaAttribute metaAttribute = MetaObjectFactory.CreateAttribute(text);
				if (metaAttribute == null)
				{
					throw new FormatException("Unknown attribute: " + text);
				}
				metaAttribute.AttributeName = attributeName;
				metaAttribute.TypeName = text;
				metaAttribute.Deserialize(input, endian);
				base.Attributes.Add(metaAttribute);
			}
			long num2 = input.Position - position;
			long num3 = (long)((ulong)base.Length - (ulong)num2);
			this.Data = input.ReadBytes((int)num3);
		}
	}
}
