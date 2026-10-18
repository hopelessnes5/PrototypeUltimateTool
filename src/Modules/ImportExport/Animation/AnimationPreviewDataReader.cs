using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Gibbed.Prototype.FileFormats.Pure3D;
using P3DAnimation = Gibbed.Prototype.FileFormats.Pure3D.Animation;

namespace PrototypeUltimateTool.Modules.ImportExport.Animation
{
    /// <summary>
    /// Decodes P3D animation data blocks for preview only. The parsed node tree is not modified
    /// except for populating the already-supported AnimationChannel key dictionaries.
    /// </summary>
    internal static class AnimationPreviewDataReader
    {
        internal sealed class Result
        {
            public int ChannelsWithFrames;
            public int ChannelsWithoutFrames;
            public List<string> Warnings = new List<string>();
        }

        public static Result Prepare(P3DAnimation animation)
        {
            var result = new Result();
            if (animation == null)
                return result;

            var all = Flatten(new[] { animation }).ToList();
            var channels = all.OfType<AnimationChannel>().ToList();
            var externalChannels = channels
                .Where(c => c != null && !c.ContainsAnimData && c.Children != null && c.Children.OfType<AnimationDataReference>().Any())
                .ToList();
            var dataNodes = all.OfType<AnimationData>().ToList();
            var blocks = dataNodes.Select(Uncompress).Where(b => b != null && b.Length > 0).ToList();
            blocks = SplitConcatenatedBlockIfNeeded(channels, blocks);

            foreach (var channel in channels)
            {
                try
                {
                    if (!channel.ContainsAnimData)
                    {
                        var reference = channel.Children == null ? null : channel.Children.OfType<AnimationDataReference>().FirstOrDefault();
                        if (reference != null && blocks.Count > 0)
                        {
                            int blockIndex = reference.ExtraData != null && reference.ExtraData.Length > 0
                                ? reference.ExtraData[0]
                                : 0;
                            if (blockIndex < 0 || blockIndex >= blocks.Count)
                                blockIndex = 0;
                            channel.ReadFrames(blocks[blockIndex]);
                        }
                    }

                    if (channel.Frames != null && channel.Frames.Count > 0)
                        result.ChannelsWithFrames++;
                    else
                        result.ChannelsWithoutFrames++;
                }
                catch (Exception ex)
                {
                    // ReadFrameKeys creates placeholder values before the compressed sample
                    // data is consumed. On a bad offset/segment, remove those placeholders so
                    // the preview cannot mistake an unread channel for a real animated track.
                    channel.Frames = new Dictionary<ushort, Gibbed.Prototype.FileFormats.Vector4>();
                    channel.FrameOrder = new ushort[0];
                    channel.NumberOfFrames = 0;
                    result.ChannelsWithoutFrames++;
                    result.Warnings.Add(string.Format("{0}/{1}: {2}",
                        channel.GetType().Name, (channel.TranslationType ?? "").TrimEnd('\0', ' '), ex.Message));
                }
            }

            if (externalChannels.Count > 0 && blocks.Count == 0)
                result.Warnings.Add("В анимации есть внешние ссылки на кадры, но блоки AnimationData не найдены или не распакованы.");
            return result;
        }

        private static List<byte[]> SplitConcatenatedBlockIfNeeded(IList<AnimationChannel> channels, List<byte[]> blocks)
        {
            if (blocks == null || blocks.Count != 1 || channels == null || channels.Count == 0)
                return blocks ?? new List<byte[]>();

            var references = channels.Select(c => new
                {
                    Channel = c,
                    Reference = c.Children == null ? null : c.Children.OfType<AnimationDataReference>().FirstOrDefault()
                })
                .Where(x => x.Reference != null)
                .ToList();
            if (references.Count == 0)
                return blocks;

            int maxSegment = references.Max(x => SegmentIndex(x.Reference));
            if (maxSegment <= 0)
                return blocks;

            var sizes = new int[maxSegment + 1];
            foreach (var item in references)
            {
                int segment = SegmentIndex(item.Reference);
                if (segment < 0 || segment >= sizes.Length)
                    continue;
                long end = (long)item.Reference.Offset + ExternalDataSize(item.Channel, item.Reference.Frames);
                if (end > sizes[segment])
                    sizes[segment] = (int)Math.Min(int.MaxValue, end);
            }

            byte[] source = blocks[0];
            var split = new List<byte[]>();
            int sourceOffset = 0;
            for (int i = 0; i < sizes.Length && sourceOffset < source.Length; i++)
            {
                int length = AlignLength(sizes[i], 16);
                length = Math.Max(0, Math.Min(length, source.Length - sourceOffset));
                var segment = new byte[length];
                Buffer.BlockCopy(source, sourceOffset, segment, 0, length);
                split.Add(segment);
                sourceOffset += length;
            }
            return split.Count > 0 ? split : blocks;
        }

        private static int SegmentIndex(AnimationDataReference reference)
        {
            return reference != null && reference.ExtraData != null && reference.ExtraData.Length > 0
                ? reference.ExtraData[0]
                : 0;
        }

        private static int ExternalDataSize(AnimationChannel channel, uint frameCount)
        {
            long frames = Math.Min((long)frameCount, int.MaxValue);
            long frameTable = frames * 2L;
            if ((frames & 1L) != 0L)
                frameTable += 2L;
            long size = frameTable + frames * GetFrameValueSize(channel);
            if (size > int.MaxValue)
                return int.MaxValue;
            return AlignLength((int)size, 4);
        }

        private static int GetFrameValueSize(AnimationChannel channel)
        {
            if (channel is Quaternion6CompressedChannel) return 6;
            if (channel is Quaternion3CompressedChannel) return 3;
            if (channel is Vector3DOFChannel) return 12;
            if (channel is Vector3DOFCompressedChannel) return 6;
            if (channel is Vector2DOFChannel) return 8;
            if (channel is Vector2DOFCompressedChannel) return 4;
            if (channel is Vector1DOFChannel || channel is Vector1DOFCompressedChannel) return 4;
            return 0;
        }

        private static int AlignLength(int value, int alignment)
        {
            if (value <= 0) return 0;
            long aligned = ((long)value + alignment - 1L) & ~((long)alignment - 1L);
            return (int)Math.Min(int.MaxValue, aligned);
        }

        private static byte[] Uncompress(AnimationData data)
        {
            if (data == null || data.CompressedData == null || data.CompressedData.Length == 0)
                return new byte[0];

            string compression = (data.Compression ?? string.Empty).TrimEnd('\0', ' ').Trim();
            if (!string.Equals(compression, "ZLIB", StringComparison.OrdinalIgnoreCase))
                return data.CompressedData;

            using (var input = new MemoryStream(data.CompressedData, false))
            {
                // P3D's ZLIB payload has a two-byte zlib header followed by a raw DEFLATE stream.
                if (input.Length >= 2) input.Position = 2;
                using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                using (var output = new MemoryStream(data.UncompressedSize > 0 && data.UncompressedSize < int.MaxValue
                    ? (int)data.UncompressedSize : 0))
                {
                    deflate.CopyTo(output);
                    byte[] bytes = output.ToArray();
                    if (data.UncompressedSize > 0 && data.UncompressedSize < int.MaxValue && bytes.Length > data.UncompressedSize)
                        Array.Resize(ref bytes, (int)data.UncompressedSize);
                    return bytes;
                }
            }
        }

        private static IEnumerable<BaseNode> Flatten(IEnumerable<BaseNode> nodes)
        {
            if (nodes == null) yield break;
            foreach (var node in nodes)
            {
                if (node == null) continue;
                yield return node;
                if (node.Children == null) continue;
                foreach (var child in Flatten(node.Children)) yield return child;
            }
        }
    }
}
