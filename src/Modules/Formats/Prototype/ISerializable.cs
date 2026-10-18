using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000004 RID: 4
	public interface ISerializable
	{
		// Token: 0x0600000B RID: 11
		void Serialize(Stream output, Endian endian);

		// Token: 0x0600000C RID: 12
		void Deserialize(Stream input, Endian endian);
	}
}
