using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200004B RID: 75
	[KnownGame(PrototypeGame.Any)]
	[KnownType(102400U)]
	public class Texture : BaseNode
	{
		// Token: 0x170000DA RID: 218
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x00008607 File Offset: 0x00006807
		// (set) Token: 0x060002D5 RID: 725 RVA: 0x0000860F File Offset: 0x0000680F
		public string Name { get; set; }

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x060002D6 RID: 726 RVA: 0x00008618 File Offset: 0x00006818
		// (set) Token: 0x060002D7 RID: 727 RVA: 0x00008620 File Offset: 0x00006820
		public uint Version { get; set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x060002D8 RID: 728 RVA: 0x00008629 File Offset: 0x00006829
		// (set) Token: 0x060002D9 RID: 729 RVA: 0x00008631 File Offset: 0x00006831
		[Category("Image")]
		public uint Width { get; set; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060002DA RID: 730 RVA: 0x0000863A File Offset: 0x0000683A
		// (set) Token: 0x060002DB RID: 731 RVA: 0x00008642 File Offset: 0x00006842
		[Category("Image")]
		public uint Height { get; set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x060002DC RID: 732 RVA: 0x0000864B File Offset: 0x0000684B
		// (set) Token: 0x060002DD RID: 733 RVA: 0x00008653 File Offset: 0x00006853
		[Category("Image")]
		public uint Bpp { get; set; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0000865C File Offset: 0x0000685C
		// (set) Token: 0x060002DF RID: 735 RVA: 0x00008664 File Offset: 0x00006864
		public uint AlphaDepth { get; set; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x0000866D File Offset: 0x0000686D
		// (set) Token: 0x060002E1 RID: 737 RVA: 0x00008675 File Offset: 0x00006875
		public uint NumMipMaps { get; set; }

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x0000867E File Offset: 0x0000687E
		// (set) Token: 0x060002E3 RID: 739 RVA: 0x00008686 File Offset: 0x00006886
		public uint TextureType { get; set; }

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060002E4 RID: 740 RVA: 0x0000868F File Offset: 0x0000688F
		// (set) Token: 0x060002E5 RID: 741 RVA: 0x00008697 File Offset: 0x00006897
		public uint Usage { get; set; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x000086A0 File Offset: 0x000068A0
		// (set) Token: 0x060002E7 RID: 743 RVA: 0x000086A8 File Offset: 0x000068A8
		public uint Priority { get; set; }

		// Token: 0x060002E8 RID: 744 RVA: 0x000086B1 File Offset: 0x000068B1
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x000086F0 File Offset: 0x000068F0
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueU32(this.Width, endian);
			output.WriteValueU32(this.Height, endian);
			output.WriteValueU32(this.Bpp, endian);
			output.WriteValueU32(this.AlphaDepth, endian);
			output.WriteValueU32(this.NumMipMaps, endian);
			output.WriteValueU32(this.TextureType, endian);
			output.WriteValueU32(this.Usage, endian);
			output.WriteValueU32(this.Priority, endian);
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00008780 File Offset: 0x00006980
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.Width = input.ReadValueU32(endian);
			this.Height = input.ReadValueU32(endian);
			this.Bpp = input.ReadValueU32(endian);
			this.AlphaDepth = input.ReadValueU32(endian);
			this.NumMipMaps = input.ReadValueU32(endian);
			this.TextureType = input.ReadValueU32(endian);
			this.Usage = input.ReadValueU32(endian);
			this.Priority = input.ReadValueU32(endian);
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0000880E File Offset: 0x00006A0E
		private BaseNode GetSubImageNode()
		{
			return this.Children.SingleOrDefault((BaseNode candidate) => candidate is TextureDDS || candidate is TexturePNG);
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0000883C File Offset: 0x00006A3C
		public override object Preview()
		{
			BaseNode subImageNode = this.GetSubImageNode();
			if (subImageNode == null)
			{
				return null;
			}
			return subImageNode.Preview();
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060002ED RID: 749 RVA: 0x0000885C File Offset: 0x00006A5C
		public override bool Exportable
		{
			get
			{
				BaseNode subImageNode = this.GetSubImageNode();
				return subImageNode != null && subImageNode.Exportable;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060002EE RID: 750 RVA: 0x0000887B File Offset: 0x00006A7B
		public override string ExportExtension
		{
			get
			{
				return "dds";
			}
		}

		// Token: 0x060002EF RID: 751 RVA: 0x00008882 File Offset: 0x00006A82
		public override void Export(Stream output)
		{
			BaseNode subImageNode = this.GetSubImageNode();
			if (subImageNode == null)
			{
				throw new InvalidOperationException();
			}
			subImageNode.Export(output);
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x0000889C File Offset: 0x00006A9C
		public override bool Importable
		{
			get
			{
				BaseNode subImageNode = this.GetSubImageNode();
				return subImageNode != null && subImageNode.Exportable;
			}
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x000088BB File Offset: 0x00006ABB
		public override void Import(Stream input)
		{
			BaseNode subImageNode = this.GetSubImageNode();
			if (subImageNode == null)
			{
				throw new InvalidOperationException();
			}
			subImageNode.Import(input);
		}
	}
}
