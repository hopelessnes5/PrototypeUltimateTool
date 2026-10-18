using System;
using System.ComponentModel;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x02000066 RID: 102
	public class Unknown : BaseNode
	{
		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x0000A3D8 File Offset: 0x000085D8
		public override uint TypeId
		{
			get
			{
				return this._TypeId;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x0000A3E0 File Offset: 0x000085E0
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x0000A3E8 File Offset: 0x000085E8
		public bool HasError { get; set; }

        /// <summary>Reason the parser preserved this chunk as raw bytes instead of a typed node.</summary>
        [ReadOnly(true)]
        [Category("Pure3D")]
        public string DeserializationError { get; set; }

		// Token: 0x06000424 RID: 1060 RVA: 0x0000A3F1 File Offset: 0x000085F1
		public Unknown(uint typeId)
		{
			this._TypeId = typeId;
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x0000A400 File Offset: 0x00008600
		// (set) Token: 0x06000426 RID: 1062 RVA: 0x0000A408 File Offset: 0x00008608
		[ReadOnly(true)]
		[Category("Pure3D")]
		public byte[] Data { get; set; }

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0000A411 File Offset: 0x00008611
		[ReadOnly(true)]
		[Category("Pure3D")]
		public int Length
		{
			get
			{
				if (this.Data != null)
				{
					return this.Data.Length;
				}
				return -1;
			}
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x0000A428 File Offset: 0x00008628
		public override string ToString()
		{
            return this.HasError
                ? "Unknown (" + this.TypeId.ToString("X8") + ") [parse warning]"
                : "Unknown (" + this.TypeId.ToString("X8") + ")";
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x0000A457 File Offset: 0x00008657
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteBytes(this.Data);
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0000A465 File Offset: 0x00008665
		public override void Deserialize(Stream input, Endian endian)
		{
			throw new NotSupportedException();
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x0000A46C File Offset: 0x0000866C
		public override bool Exportable
		{
			get
			{
				return this.Data != null && this.Data.Length != 0;
			}
		}

        /// <summary>Unknown chunks are editable as opaque header payloads.</summary>
        public override bool Importable => true;

        public override void Import(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            using (var memory = new MemoryStream())
            {
                input.CopyTo(memory);
                this.Data = memory.ToArray();
            }
            this.HasError = false;
            this.DeserializationError = null;
        }

		// Token: 0x0600042C RID: 1068 RVA: 0x0000A482 File Offset: 0x00008682
		public override void Export(Stream output)
		{
			output.Write(this.Data, 0, this.Data.Length);
		}

		// Token: 0x0400018B RID: 395
		private readonly uint _TypeId;
	}
}
