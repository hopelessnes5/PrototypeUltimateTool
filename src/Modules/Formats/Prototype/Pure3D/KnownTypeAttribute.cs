using System;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000048 RID: 72
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class KnownTypeAttribute : Attribute
	{
		// Token: 0x060002B5 RID: 693 RVA: 0x00008331 File Offset: 0x00006531
		public KnownTypeAttribute(uint id)
		{
			this.Id = id;
		}

		// Token: 0x0400011D RID: 285
		public uint Id;
	}
}
