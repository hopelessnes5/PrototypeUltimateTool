using System;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200003B RID: 59
	public enum ShaderVariableType : uint
	{
		// Token: 0x040000D3 RID: 211
		@float = 1U,
		// Token: 0x040000D4 RID: 212
		float2,
		// Token: 0x040000D5 RID: 213
		float3,
		// Token: 0x040000D6 RID: 214
		float4,
		// Token: 0x040000D7 RID: 215
		float4x4,
		// Token: 0x040000D8 RID: 216
		sampler2D,
		// Token: 0x040000D9 RID: 217
		sampler3D,
		// Token: 0x040000DA RID: 218
		samplerCUBE,
		// Token: 0x040000DB RID: 219
		sampler2D_ShadowMap,
		// Token: 0x040000DC RID: 220
		@bool
	}
}
