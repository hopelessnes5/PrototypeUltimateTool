using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000052 RID: 82
	[KnownType(69644U)]
	public class ShaderCode : BaseNode
	{
		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000361 RID: 865 RVA: 0x00009266 File Offset: 0x00007466
		// (set) Token: 0x06000362 RID: 866 RVA: 0x0000926E File Offset: 0x0000746E
		[DisplayName("Global Variable Count")]
		public uint GlobalVariableCount { get; set; }

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000363 RID: 867 RVA: 0x00009277 File Offset: 0x00007477
		// (set) Token: 0x06000364 RID: 868 RVA: 0x0000927F File Offset: 0x0000747F
		public ShaderCode.ICode Code { get; set; }

		// Token: 0x06000365 RID: 869 RVA: 0x00009288 File Offset: 0x00007488
		public override string ToString()
		{
			if (this.Code == null)
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.Code.GetType().Name + ")";
		}

		// Token: 0x06000366 RID: 870 RVA: 0x000092C0 File Offset: 0x000074C0
		public override void Serialize(Stream output, Endian endian)
		{
			if (this.Code is ShaderCode.SourceCode)
			{
				output.WriteValueU32(1U, endian);
			}
			else
			{
				if (!(this.Code is ShaderCode.CompiledCode))
				{
					throw new InvalidOperationException();
				}
				output.WriteValueU32(5U, endian);
			}
			Stream stream = new MemoryStream();
			this.Code.Serialize(stream);
			stream.Seek(0L, SeekOrigin.Begin);
			output.WriteValueS32((int)stream.Length);
			output.WriteFromStream(stream, stream.Length);
		}

		// Token: 0x06000367 RID: 871 RVA: 0x00009338 File Offset: 0x00007538
		public override void Deserialize(Stream input, Endian endian)
		{
			uint num = input.ReadValueU32(endian);
			int size = input.ReadValueS32(endian);
			if (size < 0) throw new FormatException("ShaderCode declares a negative source length.");
			if (input.CanSeek)
			{
				long available = input.Length - input.Position;
				if (available < 4L || size > available - 4L)
					throw new FormatException("ShaderCode source length exceeds the declared chunk payload.");
			}
			this.GlobalVariableCount = input.ReadValueU32(endian);
			MemoryStream input2 = input.ReadToMemoryStream(size);
			ShaderCode.ICode code;
			if (num == 1U)
			{
				code = new ShaderCode.SourceCode();
			}
			else
			{
				if (num != 5U)
				{
					throw new InvalidOperationException();
				}
				code = new ShaderCode.CompiledCode();
			}
			code.Deserialize(input2);
			this.Code = code;
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000368 RID: 872 RVA: 0x00009396 File Offset: 0x00007596
		public override bool Exportable
		{
			get
			{
				return this.Code != null;
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x000093A1 File Offset: 0x000075A1
		public override void Export(Stream output)
		{
			this.Code.Serialize(output);
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x0600036A RID: 874 RVA: 0x00009396 File Offset: 0x00007596
		public override bool Importable
		{
			get
			{
				return this.Code != null;
			}
		}

		// Token: 0x0600036B RID: 875 RVA: 0x000093AF File Offset: 0x000075AF
		public override void Import(Stream input)
		{
			this.Code.Deserialize(input);
		}

		// Token: 0x02000093 RID: 147
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public interface ICode
		{
			// Token: 0x060005AF RID: 1455
			void Serialize(Stream output);

			// Token: 0x060005B0 RID: 1456
			void Deserialize(Stream input);
		}

		// Token: 0x02000094 RID: 148
		public class CompiledCode : ShaderCode.ICode
		{
			// Token: 0x170001D2 RID: 466
			// (get) Token: 0x060005B1 RID: 1457 RVA: 0x0000CCA8 File Offset: 0x0000AEA8
			// (set) Token: 0x060005B2 RID: 1458 RVA: 0x0000CCB0 File Offset: 0x0000AEB0
			public byte[] Data { get; set; }

			// Token: 0x060005B3 RID: 1459 RVA: 0x0000CCB9 File Offset: 0x0000AEB9
			public void Serialize(Stream output)
			{
				output.Write(this.Data, 0, this.Data.Length);
			}

			// Token: 0x060005B4 RID: 1460 RVA: 0x0000CCD0 File Offset: 0x0000AED0
			public void Deserialize(Stream input)
			{
				this.Data = new byte[input.Length];
				input.ReadExactly(this.Data);
			}
		}

		// Token: 0x02000095 RID: 149
		public class SourceCode : ShaderCode.ICode
		{
			// Token: 0x170001D3 RID: 467
			// (get) Token: 0x060005B6 RID: 1462 RVA: 0x0000CCFA File Offset: 0x0000AEFA
			// (set) Token: 0x060005B7 RID: 1463 RVA: 0x0000CD02 File Offset: 0x0000AF02
			public string Text { get; set; }

			// Token: 0x060005B8 RID: 1464 RVA: 0x0000CD0B File Offset: 0x0000AF0B
			public void Serialize(Stream output)
			{
				output.WriteStringZ(this.Text, Encoding.ASCII);
			}

			// Token: 0x060005B9 RID: 1465 RVA: 0x0000CD1E File Offset: 0x0000AF1E
			public void Deserialize(Stream input)
			{
				this.Text = input.ReadString((int)input.Length, true, Encoding.ASCII);
			}
		}
	}
}
