using System;
using System.IO;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000050 RID: 80
	[KnownGame(PrototypeGame.Any)]
	[KnownType(8704U)]
	public class Camera : BaseNode
	{
		// Token: 0x17000104 RID: 260
		// (get) Token: 0x0600033F RID: 831 RVA: 0x00008FAC File Offset: 0x000071AC
		// (set) Token: 0x06000340 RID: 832 RVA: 0x00008FB4 File Offset: 0x000071B4
		public string Name { get; set; }

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000341 RID: 833 RVA: 0x00008FBD File Offset: 0x000071BD
		// (set) Token: 0x06000342 RID: 834 RVA: 0x00008FC5 File Offset: 0x000071C5
		public uint Version { get; set; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000343 RID: 835 RVA: 0x00008FCE File Offset: 0x000071CE
		// (set) Token: 0x06000344 RID: 836 RVA: 0x00008FD6 File Offset: 0x000071D6
		public float FOV { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000345 RID: 837 RVA: 0x00008FDF File Offset: 0x000071DF
		// (set) Token: 0x06000346 RID: 838 RVA: 0x00008FE7 File Offset: 0x000071E7
		public float AspectRatio { get; set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000347 RID: 839 RVA: 0x00008FF0 File Offset: 0x000071F0
		// (set) Token: 0x06000348 RID: 840 RVA: 0x00008FF8 File Offset: 0x000071F8
		public float NearClip { get; set; }

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x06000349 RID: 841 RVA: 0x00009001 File Offset: 0x00007201
		// (set) Token: 0x0600034A RID: 842 RVA: 0x00009009 File Offset: 0x00007209
		public float FarClip { get; set; }

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x0600034B RID: 843 RVA: 0x00009012 File Offset: 0x00007212
		// (set) Token: 0x0600034C RID: 844 RVA: 0x0000901A File Offset: 0x0000721A
		public Vector3 Position { get; set; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x0600034D RID: 845 RVA: 0x00009023 File Offset: 0x00007223
		// (set) Token: 0x0600034E RID: 846 RVA: 0x0000902B File Offset: 0x0000722B
		public Vector3 Look { get; set; }

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600034F RID: 847 RVA: 0x00009034 File Offset: 0x00007234
		// (set) Token: 0x06000350 RID: 848 RVA: 0x0000903C File Offset: 0x0000723C
		public Vector3 Up { get; set; }

		// Token: 0x06000351 RID: 849 RVA: 0x00009045 File Offset: 0x00007245
		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.Name))
			{
				return base.ToString() + " (" + this.Name.Trim(new char[1]) + ")";
			}
			return base.ToString();
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00009084 File Offset: 0x00007284
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteStringAlignedU8(this.Name);
			output.WriteValueU32(this.Version, endian);
			output.WriteValueF32(this.FOV, endian);
			output.WriteValueF32(this.AspectRatio, endian);
			output.WriteValueF32(this.NearClip, endian);
			output.WriteValueF32(this.FarClip, endian);
			this.Position.Serialize(output, endian);
			this.Look.Serialize(output, endian);
			this.Up.Serialize(output, endian);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00009108 File Offset: 0x00007308
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Name = input.ReadStringAlignedU8();
			this.Version = input.ReadValueU32(endian);
			this.FOV = input.ReadValueF32(endian);
			this.AspectRatio = input.ReadValueF32(endian);
			this.NearClip = input.ReadValueF32(endian);
			this.FarClip = input.ReadValueF32(endian);
			this.Position = new Vector3(input, endian);
			this.Look = new Vector3(input, endian);
			this.Up = new Vector3(input, endian);
		}
	}
}
