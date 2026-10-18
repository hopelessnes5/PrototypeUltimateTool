using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Nixson.Common;
using Nixson.Prototype.Tod;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000061 RID: 97
	[KnownGame(PrototypeGame.Any)]
	[KnownType(133169153U)]
	[TypeConverter(typeof(TodTypeConverter))]
	public class MetaObjectData : BaseNode
	{
		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060003FA RID: 1018 RVA: 0x0000A068 File Offset: 0x00008268
		// (set) Token: 0x060003FB RID: 1019 RVA: 0x0000A070 File Offset: 0x00008270
		[Browsable(false)]
		public MetaObjectDefinition ObjectDefinition { get; set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060003FC RID: 1020 RVA: 0x0000A079 File Offset: 0x00008279
		// (set) Token: 0x060003FD RID: 1021 RVA: 0x0000A081 File Offset: 0x00008281
		[Browsable(false)]
		public bool BigEndian { get; set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060003FE RID: 1022 RVA: 0x0000A08A File Offset: 0x0000828A
		[Browsable(false)]
		public string TypeName
		{
			get
			{
				return this.ObjectDefinition.TypeName;
			}
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x0000A097 File Offset: 0x00008297
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.TypeName))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.TypeName.Trim(new char[1]) + ")";
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x0000A0D4 File Offset: 0x000082D4
		public override void Serialize(Stream output, Endian endian)
		{
			Stream stream = new MemoryStream();
			if (this.BigEndian)
			{
				stream.WriteString("ATEM");
			}
			else
			{
				stream.WriteString("META");
			}
			Endian endian2 = this.BigEndian ? Endian.Big : Endian.Little;
			this.metaObject.Serialize(stream, endian2);
			output.WriteValueU32((uint)stream.Length, endian);
			stream.Seek(0L, SeekOrigin.Begin);
			output.WriteFromStream(stream, stream.Length);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0000A148 File Offset: 0x00008348
		public override void Deserialize(Stream input, Endian endian)
		{
			this.ObjectDefinition = (MetaObjectDefinition)this.ParentNode;
			uint num = input.ReadValueU32(endian);
			num -= 4U;
			string a = input.ReadString(4);
			if (a == "META")
			{
				this.BigEndian = false;
			}
			else
			{
				if (!(a == "ATEM"))
				{
					throw new FormatException("Invalid tod signature");
				}
				this.BigEndian = true;
			}
			Endian endian2 = this.BigEndian ? Endian.Big : Endian.Little;
			MetaObject metaObject = MetaObjectFactory.CreateTod(this.TypeName.Trim(new char[1]), base.Game);
			if (metaObject == null)
			{
				this.metaObject = new UnknownMeta(input, endian2, num);
				return;
			}
			this.metaObject = metaObject;
			this.metaObject.Length = num;
			this.metaObject.Deserialize(input, endian2);
		}

		// Token: 0x04000181 RID: 385
		[Browsable(false)]
		public MetaObject metaObject;
	}
}
