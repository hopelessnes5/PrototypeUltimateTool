using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004E RID: 78
	[KnownGame(PrototypeGame.Any)]
	[KnownType(102406U)]
	public class TextureDDS : BaseNode
	{
		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000320 RID: 800 RVA: 0x00008CC7 File Offset: 0x00006EC7
		// (set) Token: 0x06000321 RID: 801 RVA: 0x00008CCF File Offset: 0x00006ECF
		public string Name { get; set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000322 RID: 802 RVA: 0x00008CD8 File Offset: 0x00006ED8
		// (set) Token: 0x06000323 RID: 803 RVA: 0x00008CE0 File Offset: 0x00006EE0
		public uint Version { get; set; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000324 RID: 804 RVA: 0x00008CE9 File Offset: 0x00006EE9
		// (set) Token: 0x06000325 RID: 805 RVA: 0x00008CF1 File Offset: 0x00006EF1
		[Category("Image")]
		public uint Width { get; set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000326 RID: 806 RVA: 0x00008CFA File Offset: 0x00006EFA
		// (set) Token: 0x06000327 RID: 807 RVA: 0x00008D02 File Offset: 0x00006F02
		[Category("Image")]
		public uint Height { get; set; }

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000328 RID: 808 RVA: 0x00008D0B File Offset: 0x00006F0B
		// (set) Token: 0x06000329 RID: 809 RVA: 0x00008D13 File Offset: 0x00006F13
		public uint Unknown4 { get; set; }

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x0600032A RID: 810 RVA: 0x00008D1C File Offset: 0x00006F1C
		// (set) Token: 0x0600032B RID: 811 RVA: 0x00008D24 File Offset: 0x00006F24
		public uint Unknown5 { get; set; }

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x0600032C RID: 812 RVA: 0x00008D2D File Offset: 0x00006F2D
		// (set) Token: 0x0600032D RID: 813 RVA: 0x00008D35 File Offset: 0x00006F35
		[Category("Image")]
		public uint NumMipMaps { get; set; }

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600032E RID: 814 RVA: 0x00008D3E File Offset: 0x00006F3E
		// (set) Token: 0x0600032F RID: 815 RVA: 0x00008D46 File Offset: 0x00006F46
		[Category("Image")]
		public TextureDDS.CompressionAlgorithm Algorithm { get; set; }

		// Token: 0x06000330 RID: 816 RVA: 0x00008D4F File Offset: 0x00006F4F
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name + ")";
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00008D80 File Offset: 0x00006F80
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Width, endian);
			output.WriteValueU32(this.Height, endian);
			output.WriteValueU32(this.Unknown4, endian);
			output.WriteValueU32(this.Unknown5, endian);
			output.WriteValueU32(this.NumMipMaps, endian);
			output.WriteValueU32((uint)this.Algorithm, endian);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00008DF4 File Offset: 0x00006FF4
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.Width = input.ReadValueU32(endian);
			this.Height = input.ReadValueU32(endian);
			this.Unknown4 = input.ReadValueU32(endian);
			this.Unknown5 = input.ReadValueU32(endian);
			this.NumMipMaps = input.ReadValueU32(endian);
			this.Algorithm = (TextureDDS.CompressionAlgorithm)input.ReadValueU32(endian);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00008E68 File Offset: 0x00007068
		public override object Preview()
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null || childNode.Data == null)
			{
				return null;
			}

			// Use the same managed DXT1/DXT3/DXT5 decoder as the main preview UI.
			return DdsDecoder.DecodeDds(childNode.Data);
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000334 RID: 820 RVA: 0x00008ED0 File Offset: 0x000070D0
		public override bool Exportable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<TextureData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0000887B File Offset: 0x00006A7B
		public override string ExportExtension
		{
			get
			{
				return "dds";
			}
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00008AD7 File Offset: 0x00006CD7
		public override void Export(Stream output)
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Export(output);
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000337 RID: 823 RVA: 0x00008EF0 File Offset: 0x000070F0
		public override bool Importable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<TextureData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00008B0F File Offset: 0x00006D0F
		public override void Import(Stream input)
		{
			TextureData childNode = base.GetChildNode<TextureData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Import(input);
		}

		// Token: 0x02000092 RID: 146
		public enum CompressionAlgorithm : uint
		{
			// Token: 0x0400021B RID: 539
			Unknown,
			// Token: 0x0400021C RID: 540
			DXT1 = 827611204U,
			// Token: 0x0400021D RID: 541
			DXT3 = 861165636U,
			// Token: 0x0400021E RID: 542
			DXT5 = 894720068U
		}
	}
}
