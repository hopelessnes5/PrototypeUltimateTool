using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004C RID: 76
	[KnownType(102401U)]
	public class TexturePNG : BaseNode
	{
		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x000088D2 File Offset: 0x00006AD2
		// (set) Token: 0x060002F4 RID: 756 RVA: 0x000088DA File Offset: 0x00006ADA
		public string Name { get; set; }

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x000088E3 File Offset: 0x00006AE3
		// (set) Token: 0x060002F6 RID: 758 RVA: 0x000088EB File Offset: 0x00006AEB
		public uint Unknown1 { get; set; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x000088F4 File Offset: 0x00006AF4
		// (set) Token: 0x060002F8 RID: 760 RVA: 0x000088FC File Offset: 0x00006AFC
		[Category("Image")]
		public uint Width { get; set; }

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x00008905 File Offset: 0x00006B05
		// (set) Token: 0x060002FA RID: 762 RVA: 0x0000890D File Offset: 0x00006B0D
		[Category("Image")]
		public uint Height { get; set; }

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060002FB RID: 763 RVA: 0x00008916 File Offset: 0x00006B16
		// (set) Token: 0x060002FC RID: 764 RVA: 0x0000891E File Offset: 0x00006B1E
		public uint Bpp { get; set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060002FD RID: 765 RVA: 0x00008927 File Offset: 0x00006B27
		// (set) Token: 0x060002FE RID: 766 RVA: 0x0000892F File Offset: 0x00006B2F
		public uint Palettized { get; set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060002FF RID: 767 RVA: 0x00008938 File Offset: 0x00006B38
		// (set) Token: 0x06000300 RID: 768 RVA: 0x00008940 File Offset: 0x00006B40
		public uint HasAlpha { get; set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000301 RID: 769 RVA: 0x00008949 File Offset: 0x00006B49
		// (set) Token: 0x06000302 RID: 770 RVA: 0x00008951 File Offset: 0x00006B51
		public uint Format { get; set; }

		// Token: 0x06000303 RID: 771 RVA: 0x0000895A File Offset: 0x00006B5A
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000898C File Offset: 0x00006B8C
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Unknown1, endian);
			output.WriteValueU32(this.Width, endian);
			output.WriteValueU32(this.Height, endian);
			output.WriteValueU32(this.Bpp, endian);
			output.WriteValueU32(this.Palettized, endian);
			output.WriteValueU32(this.HasAlpha, endian);
			output.WriteValueU32(this.Format, endian);
		}

		// Token: 0x06000305 RID: 773 RVA: 0x00008A00 File Offset: 0x00006C00
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown1 = input.ReadValueU32(endian);
			this.Width = input.ReadValueU32(endian);
			this.Height = input.ReadValueU32(endian);
			this.Bpp = input.ReadValueU32(endian);
			this.Palettized = input.ReadValueU32(endian);
			this.HasAlpha = input.ReadValueU32(endian);
			this.Format = input.ReadValueU32(endian);
		}

		// Token: 0x06000306 RID: 774 RVA: 0x00008A74 File Offset: 0x00006C74
		public override object Preview()
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(childNode.Data, 0, childNode.Data.Length);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return Image.FromStream(memoryStream);
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000307 RID: 775 RVA: 0x00008AB8 File Offset: 0x00006CB8
		public override bool Exportable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<TextureData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x06000308 RID: 776 RVA: 0x00008AD7 File Offset: 0x00006CD7
		public override void Export(Stream output)
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Export(output);
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000309 RID: 777 RVA: 0x00008AF0 File Offset: 0x00006CF0
		public override bool Importable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<TextureData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x0600030A RID: 778 RVA: 0x00008B0F File Offset: 0x00006D0F
		public override void Import(Stream input)
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Import(input);
		}
	}
}
