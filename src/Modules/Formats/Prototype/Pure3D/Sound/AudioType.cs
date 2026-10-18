using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D.Sound
{
	// Token: 0x02000073 RID: 115
	public abstract class AudioType : ISerializable
	{
		// Token: 0x06000498 RID: 1176 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public AudioType()
		{
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x0000AE0A File Offset: 0x0000900A
		public AudioType(Stream input, Endian endian)
		{
			this.Deserialize(input, endian);
		}

		// Token: 0x0600049A RID: 1178
		public abstract void Serialize(Stream output, Endian endian);

		// Token: 0x0600049B RID: 1179
		public abstract void Deserialize(Stream input, Endian endian);
	}
}
