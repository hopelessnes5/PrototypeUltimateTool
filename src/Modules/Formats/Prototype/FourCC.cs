using System;
using System.IO;
using System.Text;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x0200000B RID: 11
	public class FourCC
	{
		// Token: 0x06000037 RID: 55 RVA: 0x00002EBC File Offset: 0x000010BC
		public FourCC()
		{
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002ED0 File Offset: 0x000010D0
		public FourCC(byte[] chars)
		{
			if (chars == null)
			{
				throw new ArgumentNullException("chars");
			}
			if (chars.Length != 4)
			{
				throw new ArgumentException("chars must have a length of 4");
			}
			Array.Copy(chars, this._Chars, 4);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002F10 File Offset: 0x00001110
		public FourCC(Stream input)
		{
			this.Deserialize(input);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00002F2B File Offset: 0x0000112B
		public override string ToString()
		{
			return Encoding.ASCII.GetString(this._Chars).TrimEnd(new char[1]);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002F48 File Offset: 0x00001148
		public static implicit operator FourCC(string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			if (bytes.Length != 4)
			{
				throw new InvalidCastException();
			}
			return new FourCC(bytes);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002F66 File Offset: 0x00001166
		public override int GetHashCode()
		{
			return this._Chars.GetHashCode();
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002F74 File Offset: 0x00001174
		public override bool Equals(object obj)
		{
			FourCC fourCC = obj as FourCC;
			return fourCC != null && (fourCC._Chars[0] == this._Chars[0] && fourCC._Chars[1] == this._Chars[1] && fourCC._Chars[2] == this._Chars[2]) && fourCC._Chars[3] == this._Chars[3];
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00002FD7 File Offset: 0x000011D7
		public void Serialize(Stream output)
		{
			output.Write(this._Chars, 0, 4);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002FE7 File Offset: 0x000011E7
		public void Deserialize(Stream input)
		{
			input.ReadExactly(this._Chars, 0, 4);
		}

		// Token: 0x0400004B RID: 75
		private readonly byte[] _Chars = new byte[4];
	}
}
