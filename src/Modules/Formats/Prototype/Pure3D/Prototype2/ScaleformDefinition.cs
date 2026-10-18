using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D.Prototype2
{
	// Token: 0x0200007C RID: 124
	[KnownGame(PrototypeGame.P2)]
	[KnownType(98819U)]
	public class ScaleformDefinition : BaseNode
	{
		// Token: 0x17000192 RID: 402
		// (get) Token: 0x060004F2 RID: 1266 RVA: 0x0000B681 File Offset: 0x00009881
		// (set) Token: 0x060004F3 RID: 1267 RVA: 0x0000B689 File Offset: 0x00009889
		public string Name { get; set; }

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x060004F4 RID: 1268 RVA: 0x0000B692 File Offset: 0x00009892
		// (set) Token: 0x060004F5 RID: 1269 RVA: 0x0000B69A File Offset: 0x0000989A
		public int Unknown1 { get; set; }

		// Token: 0x060004F6 RID: 1270 RVA: 0x0000B6A3 File Offset: 0x000098A3
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0000B6DF File Offset: 0x000098DF
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueS32(this.Unknown1, endian);
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000B6FA File Offset: 0x000098FA
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Unknown1 = input.ReadValueS32(endian);
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x060004F9 RID: 1273 RVA: 0x0000B639 File Offset: 0x00009839
		public override string ExportExtension
		{
			get
			{
				return "gfx";
			}
		}

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x060004FA RID: 1274 RVA: 0x0000B718 File Offset: 0x00009918
		public override bool Exportable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<ScaleformData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x0000B737 File Offset: 0x00009937
		public override void Export(Stream output)
		{
			ScaleformData childNode = base.GetChildNode<ScaleformData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Export(output);
		}

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x060004FC RID: 1276 RVA: 0x0000B750 File Offset: 0x00009950
		public override bool Importable
		{
			get
			{
				BaseNode childNode = base.GetChildNode<ScaleformData>();
				return childNode != null && childNode.Exportable;
			}
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0000B76F File Offset: 0x0000996F
		public override void Import(Stream input)
		{
			ScaleformData childNode = base.GetChildNode<ScaleformData>();
			if (childNode == null)
			{
				throw new InvalidOperationException();
			}
			childNode.Import(input);
		}
	}
}
