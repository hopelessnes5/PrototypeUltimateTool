using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000046 RID: 70
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class TextureGlyph
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x0600029C RID: 668 RVA: 0x000081F0 File Offset: 0x000063F0
		// (set) Token: 0x0600029D RID: 669 RVA: 0x000081F8 File Offset: 0x000063F8
		public uint TextureNum { get; set; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x0600029E RID: 670 RVA: 0x00008201 File Offset: 0x00006401
		// (set) Token: 0x0600029F RID: 671 RVA: 0x00008209 File Offset: 0x00006409
		public Vector2 BottomLeft { get; set; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060002A0 RID: 672 RVA: 0x00008212 File Offset: 0x00006412
		// (set) Token: 0x060002A1 RID: 673 RVA: 0x0000821A File Offset: 0x0000641A
		public Vector2 TopRight { get; set; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060002A2 RID: 674 RVA: 0x00008223 File Offset: 0x00006423
		// (set) Token: 0x060002A3 RID: 675 RVA: 0x0000822B File Offset: 0x0000642B
		public float LeftBearing { get; set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x00008234 File Offset: 0x00006434
		// (set) Token: 0x060002A5 RID: 677 RVA: 0x0000823C File Offset: 0x0000643C
		public float RightBearing { get; set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060002A6 RID: 678 RVA: 0x00008245 File Offset: 0x00006445
		// (set) Token: 0x060002A7 RID: 679 RVA: 0x0000824D File Offset: 0x0000644D
		public float Width { get; set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x00008256 File Offset: 0x00006456
		// (set) Token: 0x060002A9 RID: 681 RVA: 0x0000825E File Offset: 0x0000645E
		public float Advance { get; set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060002AA RID: 682 RVA: 0x00008267 File Offset: 0x00006467
		// (set) Token: 0x060002AB RID: 683 RVA: 0x0000826F File Offset: 0x0000646F
		public uint Code { get; set; }

		// Token: 0x060002AC RID: 684 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public TextureGlyph()
		{
		}

		// Token: 0x060002AD RID: 685 RVA: 0x00008278 File Offset: 0x00006478
		public TextureGlyph(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00008288 File Offset: 0x00006488
		public void Serialize(Stream output)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060002AF RID: 687 RVA: 0x00008290 File Offset: 0x00006490
		public void Deserialize(Stream input, Endian endian)
		{
			this.TextureNum = input.ReadValueU32();
			this.BottomLeft = new Vector2(input, endian);
			this.TopRight = new Vector2(input, endian);
			this.LeftBearing = input.ReadValueF32(endian);
			this.RightBearing = input.ReadValueF32(endian);
			this.Width = input.ReadValueF32(endian);
			this.Advance = input.ReadValueF32(endian);
			this.Code = input.ReadValueU32(endian);
		}
	}
}
