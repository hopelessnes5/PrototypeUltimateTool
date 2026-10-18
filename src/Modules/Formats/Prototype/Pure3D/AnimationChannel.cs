using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
	// Token: 0x0200001C RID: 28
	public abstract class AnimationChannel : BaseNode
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000DA RID: 218 RVA: 0x000045D6 File Offset: 0x000027D6
		// (set) Token: 0x060000DB RID: 219 RVA: 0x000045DE File Offset: 0x000027DE
		public uint Version { get; set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000DC RID: 220 RVA: 0x000045E7 File Offset: 0x000027E7
		// (set) Token: 0x060000DD RID: 221 RVA: 0x000045EF File Offset: 0x000027EF
		public string TranslationType { get; set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000DE RID: 222 RVA: 0x000045F8 File Offset: 0x000027F8
		// (set) Token: 0x060000DF RID: 223 RVA: 0x00004600 File Offset: 0x00002800
		public uint NumberOfFrames { get; set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00004609 File Offset: 0x00002809
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x00004611 File Offset: 0x00002811
		public ushort Mapping { get; set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x0000461A File Offset: 0x0000281A
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x00004622 File Offset: 0x00002822
        private Dictionary<ushort, Vector4> _frames;
        public Dictionary<ushort, Vector4> Frames
        {
            get { return this._frames; }
            set { this._frames = value; this._orderedFrameKeysCache = null; }
        }
        private ushort[] _orderedFrameKeysCache;

        // The source format stores a frame-key table followed by values in exactly the
        // same order. Keep that table separate from Frames: a few real assets repeat a
        // frame index, and Dictionary.Add must not make the whole P3D unreadable.
        public ushort[] FrameOrder { get; set; }

        /// <summary>Frame keys sorted by frame number; dictionary enumeration is not a timeline.</summary>
        public ushort[] OrderedFrameKeys
        {
            get
            {
                if (this.Frames == null || this.Frames.Count == 0) return new ushort[0];
                if (this._orderedFrameKeysCache == null || this._orderedFrameKeysCache.Length != this.Frames.Count)
                    this._orderedFrameKeysCache = this.Frames.Keys.OrderBy(k => k).ToArray();
                return this._orderedFrameKeysCache;
            }
        }


		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x0000462B File Offset: 0x0000282B
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x00004633 File Offset: 0x00002833
		public Vector3 BaseValues { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x0000463C File Offset: 0x0000283C
		public bool ContainsAnimData
		{
			get
			{
				return this.NumberOfFrames > 0U;
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00004647 File Offset: 0x00002847
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.TranslationType))
			{
				return base.ToString();
			}
			return base.ToString() + " (" + this.TranslationType.Trim(new char[1]) + ")";
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00004683 File Offset: 0x00002883
		public override void Serialize(Stream output, Endian endian)
		{
			output.WriteValueU32(this.Version);
			output.WriteString(this.TranslationType);
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000469D File Offset: 0x0000289D
		public override void Deserialize(Stream input, Endian endian)
		{
			this.Version = input.ReadValueU32(endian);
			this.TranslationType = input.ReadString(4);
		}

		// Token: 0x060000EA RID: 234 RVA: 0x000046BC File Offset: 0x000028BC
        public virtual void ReadFrames(byte[] animationData)
        {
            if (this.ContainsAnimData || animationData == null || animationData.Length == 0)
                return;
            var reference = this.Children == null ? null : this.Children.OfType<AnimationDataReference>().FirstOrDefault();
            if (reference == null || (long)reference.Offset >= animationData.Length || reference.Frames == 0 || reference.Frames > int.MaxValue)
                return;
            long bytesAfterOffset = animationData.Length - (long)reference.Offset;
            long minimumTableBytes = (long)reference.Frames * 2L + ((reference.Frames & 1U) != 0U ? 2L : 0L);
            if (minimumTableBytes > bytesAfterOffset || minimumTableBytes > int.MaxValue)
                return;

            using (var input = new MemoryStream(animationData, false))
            {
                input.Position = reference.Offset;
                this.NumberOfFrames = reference.Frames;
                this.ReadFrameKeys(input, Endian.Little, false);
                if (this.NumberOfFrames % 2U != 0U)
                    input.ReadBytes(2);
                this.ReadChannel(input, Endian.Little);
            }
        }

        protected void ReadFrameKeys(Stream input, Endian endian, bool enforceChunkBounds = true)
        {
            if (this.NumberOfFrames > int.MaxValue)
                throw new FormatException("Animation channel contains too many frames.");

            int count = (int)this.NumberOfFrames;
            long keyTableBytes = (long)count * 2L;
            if (input.CanSeek)
            {
                long available = input.Length - input.Position;
                if (enforceChunkBounds && this.HeaderSize >= 12U)
                {
                    long chunkEnd = (long)this.StartPosition + this.HeaderSize;
                    available = Math.Min(available, chunkEnd - input.Position);
                }
                if (keyTableBytes > available)
                    throw new FormatException("Animation channel frame-key table exceeds its chunk size.");
            }

            this.Frames = new Dictionary<ushort, Vector4>();
            this.FrameOrder = new ushort[count];
            this._orderedFrameKeysCache = null;
            for (int i = 0; i < count; i++)
            {
                ushort key = input.ReadValueU16(endian);
                this.FrameOrder[i] = key;
                // Preserve each key-table entry in FrameOrder, but let the last sample for
                // a repeated frame key win in the dictionary. ReadChannel still consumes
                // one value per FrameOrder entry, so subsequent chunks stay aligned.
                if (!this.Frames.ContainsKey(key))
                    this.Frames.Add(key, new Vector4());
            }
        }

        protected ushort[] GetRawFrameKeys()
        {
            if (this.FrameOrder != null && this.FrameOrder.Length > 0)
                return this.FrameOrder;
            return this.Frames == null ? new ushort[0] : this.Frames.Keys.ToArray();
        }

        protected virtual bool GetKey(float frame, out int start, out int end)
        {
            var keys = this.OrderedFrameKeys;
            if (keys.Length == 0)
            {
                start = end = 0;
                return false;
            }
            if (frame < keys[0])
            {
                start = end = 0;
                return false;
            }
            if (frame >= keys[keys.Length - 1])
            {
                start = end = keys.Length - 1;
                return false;
            }
            int low = 0;
            int high = keys.Length - 2;
            while (low <= high)
            {
                int middle = low + ((high - low) / 2);
                if (frame < keys[middle]) high = middle - 1;
                else if (frame >= keys[middle + 1]) low = middle + 1;
                else
                {
                    start = middle;
                    end = middle + 1;
                    return true;
                }
            }
            start = Math.Max(0, Math.Min(keys.Length - 1, low));
            end = start;
            return false;
        }

		// Token: 0x060000EC RID: 236
		protected abstract void ReadChannel(Stream input, Endian endian);

		// Token: 0x060000ED RID: 237
		public abstract Vector4 CalculateValue(float frame);
	}
}
