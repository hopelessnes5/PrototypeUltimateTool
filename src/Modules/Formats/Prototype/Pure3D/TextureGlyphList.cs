using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200005B RID: 91
	[KnownType(139265U)]
	public class TextureGlyphList : BaseNode
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x000099FF File Offset: 0x00007BFF
		// (set) Token: 0x060003C3 RID: 963 RVA: 0x00009A07 File Offset: 0x00007C07
		public uint NumGlyphs { get; set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x060003C4 RID: 964 RVA: 0x00009A10 File Offset: 0x00007C10
		// (set) Token: 0x060003C5 RID: 965 RVA: 0x00009A18 File Offset: 0x00007C18
		public TextureGlyph[] Glyphs { get; set; }

		// Token: 0x060003C6 RID: 966 RVA: 0x00008288 File Offset: 0x00006488
		public override void Serialize(Stream output, Endian endian)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x00009A24 File Offset: 0x00007C24
		public override void Deserialize(Stream input, Endian endian)
		{
			this.NumGlyphs = input.ReadValueU32(endian);
			if (this.NumGlyphs > 500000U || this.NumGlyphs > int.MaxValue ||
				(input.CanSeek && (long)this.NumGlyphs * 40L > input.Length - input.Position))
				throw new FormatException("TextureGlyphList count exceeds the remaining chunk payload.");
			this.Glyphs = new TextureGlyph[(int)this.NumGlyphs];
			int num = 0;
			while ((long)num < (long)((ulong)this.NumGlyphs))
			{
				this.Glyphs[num] = new TextureGlyph(input, endian);
				num++;
			}
		}
	}
}
