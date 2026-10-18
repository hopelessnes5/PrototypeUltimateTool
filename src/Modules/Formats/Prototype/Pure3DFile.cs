using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats.Pure3D;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
    internal sealed class Pure3DParallelParseMetrics
    {
        internal int RootChunkCount { get; private set; }
        internal double AverageRootMilliseconds { get; private set; }
        internal double P95RootMilliseconds { get; private set; }
        internal double MaxRootMilliseconds { get; private set; }
        internal uint SlowestRootTypeId { get; private set; }

        internal Pure3DParallelParseMetrics(int rootChunkCount, double averageRootMilliseconds,
            double p95RootMilliseconds, double maxRootMilliseconds, uint slowestRootTypeId)
        {
            RootChunkCount = rootChunkCount;
            AverageRootMilliseconds = averageRootMilliseconds;
            P95RootMilliseconds = p95RootMilliseconds;
            MaxRootMilliseconds = maxRootMilliseconds;
            SlowestRootTypeId = slowestRootTypeId;
        }
    }

	// Token: 0x0200000E RID: 14
	public class Pure3DFile
	{
        internal Pure3DParallelParseMetrics ParallelParseMetrics { get; private set; }

        private struct RootChunkDescriptor
        {
            internal readonly long Offset;
            internal readonly uint TotalSize;
            internal readonly uint TypeId;

            internal RootChunkDescriptor(long offset, uint totalSize, uint typeId)
            {
                Offset = offset;
                TotalSize = totalSize;
                TypeId = typeId;
            }
        }
		// Token: 0x0600004F RID: 79 RVA: 0x0000321C File Offset: 0x0000141C
		private void SerializeNode(Stream output, BaseNode node, Endian endianess)
		{
			Stream stream = new MemoryStream();
			foreach (BaseNode node2 in node.Children)
			{
				this.SerializeNode(stream, node2, endianess);
			}
			Stream stream2 = new MemoryStream();
			node.Serialize(stream2, endianess);
			output.WriteValueU32(node.TypeId, endianess);
			output.WriteValueU32((uint)(12L + stream2.Length), endianess);
			output.WriteValueU32((uint)(12L + stream2.Length + stream.Length), endianess);
			stream2.Seek(0L, SeekOrigin.Begin);
			output.WriteFromStream(stream2, stream2.Length);
			stream.Seek(0L, SeekOrigin.Begin);
			output.WriteFromStream(stream, stream.Length);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000032EC File Offset: 0x000014EC
		public void Serialize(Stream output, Endian endianess)
		{
			Stream stream = new MemoryStream();
			foreach (BaseNode node in this.Nodes)
			{
				this.SerializeNode(stream, node, endianess);
			}
			output.WriteValueU32(4282659664U, endianess);
			output.WriteValueU32(12U, endianess);
			output.WriteValueU32((uint)(12L + stream.Length), endianess);
			stream.Seek(0L, SeekOrigin.Begin);
			output.WriteFromStream(stream, stream.Length);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003388 File Offset: 0x00001588
		public static BaseNode DeserializeNode(Stream input, Endian endian, BaseNode parent, PrototypeGame game)
		{
			long position = input.Position;
			input.ReadValueU32(endian);
			uint num = input.ReadValueU32(endian);
			uint num2 = input.ReadValueU32(endian);
			input.Position = position;
			BaseNode baseNode = Pure3DFile.CreateP3DNode(input, endian, parent, game);
			if (input.Position != position + (long)((ulong)num))
			{
				throw new FormatException();
			}
			long num3 = position + (long)((ulong)num2);
			while (input.Position < num3)
			{
				BaseNode item = Pure3DFile.DeserializeNode(input, endian, baseNode, game);
				baseNode.Children.Add(item);
			}
			if (input.Position != num3)
			{
				throw new FormatException();
			}
			return baseNode;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003414 File Offset: 0x00001614
        private static BaseNode CreateP3DNode(Stream input, Endian endian, BaseNode parent, PrototypeGame game = PrototypeGame.Any)
        {
            long position = input.Position;
            uint typeId = input.ReadValueU32(endian);
            uint headerSize = input.ReadValueU32(endian);
            uint totalSize = input.ReadValueU32(endian);

            if (headerSize < 12U || totalSize < headerSize || position + (long)headerSize > input.Length ||
                position + (long)totalSize > input.Length)
            {
                throw new FormatException("Invalid Pure3D chunk size for type 0x" + typeId.ToString("X8") + ".");
            }

            List<BaseNode> candidates = NodeFactory.CreateNodeCandidates(typeId, game);
            Exception parseError = null;
            string candidateName = null;

            foreach (BaseNode candidate in candidates)
            {
                candidate.Game = game;
                candidate.StartPosition = (uint)position;
                candidate.HeaderSize = headerSize;
                candidate.TotalSize = totalSize;
                candidate.ParentNode = parent;
                candidate.ID = typeId;
                candidateName = candidate.GetType().Name;

                if (candidate is AnimationChannel && HasInvalidInlineAnimationFrameKeyTable(input, position, headerSize, endian, (AnimationChannel)candidate))
                {
                    parseError = new FormatException("Animation channel frame-key table exceeds its chunk size.");
                    continue;
                }

                try
                {
                    input.Position = position + 12L;
                    candidate.Deserialize(input, endian);
                    if (input.Position != position + (long)headerSize)
                    {
                        throw new FormatException("The typed reader consumed " + (input.Position - (position + 12L)) +
                            " bytes, but the chunk payload contains " + (headerSize - 12U) + " bytes.");
                    }
                    return candidate;
                }
                catch (Exception ex) when (IsRecoverableChunkParseError(ex))
                {
                    parseError = ex;
                }
            }

            // Unknown or version-mismatched chunks are preserved byte-for-byte instead of
            // aborting the containing file. All candidate typed readers were bounded by the
            // declared header size, and we now resume at the start of the child-chunk area.
            input.Position = position + 12L;
            int payloadLength = checked((int)(headerSize - 12U));
            byte[] rawPayload = input.ReadBytes(payloadLength);
            if (rawPayload == null || rawPayload.Length != payloadLength)
                throw new EndOfStreamException("Unable to preserve Pure3D chunk payload.");

            var unknown = new Unknown(typeId)
            {
                Game = game,
                StartPosition = (uint)position,
                HeaderSize = headerSize,
                TotalSize = totalSize,
                ParentNode = parent,
                Data = rawPayload,
                HasError = candidates.Count > 0,
                DeserializationError = parseError == null
                    ? null
                    : (candidateName ?? "Known chunk") + ": " + parseError.Message
            };
            return unknown;
        }

        private static bool IsRecoverableChunkParseError(Exception ex)
        {
            return ex is FormatException || ex is IOException || ex is OverflowException ||
                ex is ArgumentException || ex is IndexOutOfRangeException ||
                ex is NotSupportedException || ex is NullReferenceException;
        }

		private static bool HasInvalidInlineAnimationFrameKeyTable(Stream input, long chunkPosition, uint headerSize, Endian endian, AnimationChannel channel)
		{
			// These variants place Mapping + BaseValues before NumberOfFrames; all other
			// currently registered channel variants place NumberOfFrames after Version + FourCC.
			int bytesBeforeFrameCount = (channel is Vector1DOFChannel || channel is Vector2DOFChannel ||
                channel is Vector2DOFCompressedChannel) ? 22 : (channel is VisibilityChannel ? 10 : 8);
			long frameCountPosition = chunkPosition + 12L + bytesBeforeFrameCount;
			if (headerSize < 12U || (long)headerSize < 12L + bytesBeforeFrameCount + 4L)
				return true;
			if (input == null || !input.CanSeek || chunkPosition + headerSize > input.Length)
				return false;

			long savedPosition = input.Position;
			try
			{
				input.Position = frameCountPosition;
				uint frames = input.ReadValueU32(endian);
				long available = chunkPosition + headerSize - input.Position;
				if (frames > int.MaxValue)
					return true;
				long frameCount = frames;
				// Inline channel chunks store the key table immediately followed by samples;
				// the two-byte odd-frame padding is specific to external AnimationData blocks.
				long required = frameCount * 2L + frameCount * GetInlineAnimationFrameValueSize(channel);
				return required > available;
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				input.Position = savedPosition;
			}
		}

		private static int GetInlineAnimationFrameValueSize(AnimationChannel channel)
		{
			if (channel is Quaternion6CompressedChannel) return 6;
			if (channel is Quaternion3CompressedChannel) return 3;
			if (channel is Vector3DOFChannel) return 12;
			if (channel is Vector3DOFCompressedChannel) return 6;
			if (channel is Vector2DOFChannel) return 8;
			if (channel is Vector2DOFCompressedChannel) return 4;
            if (channel is VertexMorphChannel) return 4;
			if (channel is Vector1DOFChannel) return 4;
			return 0;
		}

		private static BaseNode PreserveAnimationChannelAsUnknown(Stream input, long position, uint typeId, uint headerSize, uint totalSize, PrototypeGame game, BaseNode parent, string error)
		{
			if (headerSize < 12U || headerSize - 12U > int.MaxValue)
				throw new FormatException("Invalid animation channel chunk header size.");
			input.Position = position + 12L;
			int payloadLength = (int)(headerSize - 12U);
			byte[] rawPayload = input.ReadBytes(payloadLength);
			if (rawPayload == null || rawPayload.Length != payloadLength)
				throw new EndOfStreamException("Unable to preserve malformed animation channel payload.");
			return new Unknown(typeId)
			{
				Game = game,
				StartPosition = (uint)position,
				HeaderSize = headerSize,
				TotalSize = totalSize,
				ParentNode = parent,
				Data = rawPayload,
				HasError = true,
				DeserializationError = error
			};
		}

        /// <summary>
        /// Attempts to deserialize a Pure3D stream by processing independent root chunks in
        /// parallel. Child chunks remain parsed recursively by their owning root, preserving
        /// parent references and exact source offsets. Returns false for small files where the
        /// serial path is cheaper; in that case the input position is restored and this instance
        /// is left unchanged.
        /// </summary>
        internal bool TryDeserializeParallel(MemoryStream input, Endian endianess,
            int maxDegreeOfParallelism, CancellationToken cancellationToken, out int rootChunkCount)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanSeek) throw new ArgumentException("The Pure3D input must be seekable.", nameof(input));

            rootChunkCount = 0;
            this.ParallelParseMetrics = null;
            long start = input.Position;
            long end;
            var rootChunks = new List<RootChunkDescriptor>();
            try
            {
                uint signature = input.ReadValueU32(endianess);
                if (signature != 1345537279U && signature.Swap() != 1345537279U)
                    throw new FormatException("not a Pure3D file");
                if (input.ReadValueU32(endianess) != 12U)
                    throw new FormatException("invalid header size");
                uint fileSize = input.ReadValueU32(endianess);
                end = checked(start + (long)fileSize);
                if (fileSize < 12U || end > input.Length)
                    throw new FormatException("Invalid Pure3D file size.");

                input.Position = start + 12L;
                while (input.Position < end)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    long chunkStart = input.Position;
                    if (end - chunkStart < 12L)
                        throw new FormatException("Incomplete root Pure3D chunk header.");
                    uint typeId = input.ReadValueU32(endianess);
                    uint headerSize = input.ReadValueU32(endianess);
                    uint totalSize = input.ReadValueU32(endianess);
                    if (headerSize < 12U || totalSize < headerSize ||
                        checked(chunkStart + (long)totalSize) > end)
                        throw new FormatException("Invalid root Pure3D chunk size at offset " + chunkStart + ".");
                    rootChunks.Add(new RootChunkDescriptor(chunkStart, totalSize, typeId));
                    input.Position = chunkStart + totalSize;
                }
                if (input.Position != end)
                    throw new FormatException("The Pure3D root chunks do not match the file size.");
            }
            catch
            {
                input.Position = start;
                throw;
            }

            rootChunkCount = rootChunks.Count;
            int degree = Math.Min(maxDegreeOfParallelism, Environment.ProcessorCount);
            // For a small number of roots, thread scheduling costs more than it saves.
            if (rootChunks.Count < 8 || degree < 2)
            {
                input.Position = start;
                return false;
            }

            ArraySegment<byte> buffer;
            if (!input.TryGetBuffer(out buffer) || buffer.Array == null)
            {
                input.Position = start;
                return false;
            }

            // Build the immutable node-type registry before fan-out. Otherwise the first worker
            // would perform reflection over the full assembly while all other workers wait on its lock.
            NodeFactory.CreateNodeCandidates(0U, this.GamePicked);

            var nodes = new BaseNode[rootChunks.Count];
            var rootElapsedTicks = new long[rootChunks.Count];
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = degree,
                CancellationToken = cancellationToken
            };
            Parallel.For(0, rootChunks.Count, options, i =>
            {
                RootChunkDescriptor chunk = rootChunks[i];
                long parseStarted = Stopwatch.GetTimestamp();
                using (var reader = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false, true))
                {
                    reader.Position = chunk.Offset;
                    BaseNode node = Pure3DFile.DeserializeNode(reader, endianess, null, this.GamePicked);
                    long expectedEnd = chunk.Offset + chunk.TotalSize;
                    if (reader.Position != expectedEnd)
                        throw new FormatException("Root chunk deserialization ended at an unexpected offset: " + chunk.Offset + ".");
                    nodes[i] = node;
                }
                rootElapsedTicks[i] = Stopwatch.GetTimestamp() - parseStarted;
            });

            // Capture per-root timings to distinguish parser overhead from a few exceptionally
            // expensive roots. This also exposes load imbalance in the parallel stage.
            long totalTicks = 0L;
            long maxTicks = 0L;
            int slowestIndex = 0;
            for (int i = 0; i < rootElapsedTicks.Length; i++)
            {
                long ticks = rootElapsedTicks[i];
                totalTicks += ticks;
                if (ticks > maxTicks)
                {
                    maxTicks = ticks;
                    slowestIndex = i;
                }
            }
            long[] sortedTicks = (long[])rootElapsedTicks.Clone();
            Array.Sort(sortedTicks);
            int p95Index = Math.Max(0, (int)Math.Ceiling(sortedTicks.Length * 0.95) - 1);
            double ticksToMilliseconds = 1000.0 / Stopwatch.Frequency;
            this.ParallelParseMetrics = new Pure3DParallelParseMetrics(rootChunks.Count,
                totalTicks * ticksToMilliseconds / rootElapsedTicks.Length,
                sortedTicks[p95Index] * ticksToMilliseconds,
                maxTicks * ticksToMilliseconds,
                rootChunks[slowestIndex].TypeId);

            this.Nodes.Clear();
            this.Nodes.AddRange(nodes);
            this.Endian = endianess;
            input.Position = end;
            return true;
        }

		// Token: 0x06000053 RID: 83 RVA: 0x000034C8 File Offset: 0x000016C8
		public void Deserialize(Stream input, Endian endianess)
		{
			long position = input.Position;
			uint num = input.ReadValueU32(endianess);
			if (num != 1345537279U && num.Swap() != 1345537279U)
			{
				throw new FormatException("not a Pure3D file");
			}
			if (input.ReadValueU32(endianess) != 12U)
			{
				throw new FormatException("invalid header size");
			}
			uint num2 = input.ReadValueU32(endianess);
			if (position + (long)((ulong)num2) > input.Length)
			{
				throw new FormatException();
			}
			long num3 = position + (long)((ulong)num2);
			this.Nodes.Clear();
			while (input.Position < num3)
			{
				this.Nodes.Add(Pure3DFile.DeserializeNode(input, endianess, null, this.GamePicked));
			}
			if (input.Position != num3)
			{
				throw new FormatException();
			}
			this.Endian = endianess;
		}

		// Token: 0x0400004F RID: 79
		public Endian Endian;

		// Token: 0x04000050 RID: 80
		public List<BaseNode> Nodes = new List<BaseNode>();

		// Token: 0x04000051 RID: 81
		public PrototypeGame GamePicked;

		// Token: 0x04000052 RID: 82
		public const uint Signature = 1345537279U;
	}
}
