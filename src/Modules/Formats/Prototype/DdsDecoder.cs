using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Prototype.FileFormats.Pure3D;

namespace Gibbed.Prototype.FileFormats
{
    public static class DdsDecoder
    {
        public static Bitmap ExtractBitmap(BaseNode node, bool flipY)
        {
            if (node == null) return null;
            Bitmap bmp = null;

            // 1. Пробуем декодировать сырой DDS напрямую из ноды
            if (node.Exportable)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        node.Export(ms);
                        bmp = DecodeDds(ms.ToArray());
                    }
                }
                catch { }
            }

            // 2. Если узел Texture — проверяем дочерние элементы
            if (bmp == null && node.Children != null)
            {
                foreach (BaseNode child in node.Children)
                {
                    if (child.Exportable)
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                child.Export(ms);
                                bmp = DecodeDds(ms.ToArray());
                                if (bmp != null) break;
                            }
                        }
                        catch { }
                    }
                }
            }

            // 3. Резервный вариант только для PNG. DDS уже обрабатывается выше
            // управляемым декодером DecodeDds; нативные библиотеки не требуются.
            if (bmp == null)
            {
                try
                {
                    using (Image img = TryPreviewPng(node))
                    {
                        if (img != null) bmp = new Bitmap(img);
                    }
                }
                catch { }
            }

            if (bmp != null && flipY)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return bmp;
        }

        private static Image TryPreviewPng(BaseNode node)
        {
            if (node == null) return null;

            if (string.Equals(node.GetType().Name, "TexturePNG", StringComparison.Ordinal))
            {
                return node.Preview() as Image;
            }

            if (node.Children != null)
            {
                foreach (BaseNode child in node.Children)
                {
                    Image preview = TryPreviewPng(child);
                    if (preview != null) return preview;
                }
            }

            return null;
        }

        public static Bitmap DecodeDds(byte[] dds)
        {
            if (dds == null || dds.Length < 128) return null;
            if (dds[0] != 0x44 || dds[1] != 0x44 || dds[2] != 0x53 || dds[3] != 0x20) return null; // "DDS "

            int height = BitConverter.ToInt32(dds, 12);
            int width = BitConverter.ToInt32(dds, 16);
            if (width <= 0 || height <= 0) return null;

            uint fourCC = BitConverter.ToUInt32(dds, 84);
            uint rgbBitCount = BitConverter.ToUInt32(dds, 88);

            byte[] bgra = new byte[width * height * 4];

            if (fourCC == 0x31545844) // DXT1
                DecodeDxt1(dds, 128, width, height, bgra);
            else if (fourCC == 0x33545844) // DXT3
                DecodeDxt3(dds, 128, width, height, bgra);
            else if (fourCC == 0x35545844) // DXT5
                DecodeDxt5(dds, 128, width, height, bgra);
            else if (fourCC == 0 && rgbBitCount == 32 && dds.Length >= 128 + width * height * 4)
                Buffer.BlockCopy(dds, 128, bgra, 0, width * height * 4);
            else
                return null;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bgra, 0, bd.Scan0, bgra.Length);
            bmp.UnlockBits(bd);
            return bmp;
        }

        public static Bitmap ProcessChannels(Bitmap source, bool showR, bool showG, bool showB, bool showA)
        {
            if (source == null) return null;

            int width = source.Width;
            int height = source.Height;

            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(srcData.Stride) * height;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            source.UnlockBits(srcData);

            int activeColors = (showR ? 1 : 0) + (showG ? 1 : 0) + (showB ? 1 : 0);

            if (!showR && !showG && !showB && showA) // Только Alpha -> Градации серого
            {
                for (int i = 0; i < bytes; i += 4)
                {
                    byte a = srcBuffer[i + 3];
                    dstBuffer[i + 0] = a;
                    dstBuffer[i + 1] = a;
                    dstBuffer[i + 2] = a;
                    dstBuffer[i + 3] = 255;
                }
            }
            else if (activeColors == 1 && !showA) // Ровно один цвет -> Градации серого
            {
                int offset = showB ? 0 : (showG ? 1 : 2);
                for (int i = 0; i < bytes; i += 4)
                {
                    byte val = srcBuffer[i + offset];
                    dstBuffer[i + 0] = val;
                    dstBuffer[i + 1] = val;
                    dstBuffer[i + 2] = val;
                    dstBuffer[i + 3] = 255;
                }
            }
            else // Мультивыбор
            {
                for (int i = 0; i < bytes; i += 4)
                {
                    dstBuffer[i + 0] = showB ? srcBuffer[i + 0] : (byte)0;
                    dstBuffer[i + 1] = showG ? srcBuffer[i + 1] : (byte)0;
                    dstBuffer[i + 2] = showR ? srcBuffer[i + 2] : (byte)0;
                    dstBuffer[i + 3] = showA ? srcBuffer[i + 3] : (byte)255;
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            result.UnlockBits(dstData);

            return result;
        }

        private static void DecodeRgb565(ushort c, out byte r, out byte g, out byte b)
        {
            r = (byte)(((c >> 11) & 0x1F) * 255 / 31);
            g = (byte)(((c >> 5) & 0x3F) * 255 / 63);
            b = (byte)((c & 0x1F) * 255 / 31);
        }

        private static void DecodeDxt1(byte[] data, int offset, int width, int height, byte[] dst)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int blockOffset = offset;
            byte[] r = new byte[4], g = new byte[4], b = new byte[4];

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (blockOffset + 8 > data.Length) return;
                    ushort c0 = BitConverter.ToUInt16(data, blockOffset);
                    ushort c1 = BitConverter.ToUInt16(data, blockOffset + 2);
                    uint code = BitConverter.ToUInt32(data, blockOffset + 4);

                    DecodeRgb565(c0, out r[0], out g[0], out b[0]);
                    DecodeRgb565(c1, out r[1], out g[1], out b[1]);

                    if (c0 > c1)
                    {
                        r[2] = (byte)((2 * r[0] + r[1]) / 3); g[2] = (byte)((2 * g[0] + g[1]) / 3); b[2] = (byte)((2 * b[0] + b[1]) / 3);
                        r[3] = (byte)((r[0] + 2 * r[1]) / 3); g[3] = (byte)((g[0] + 2 * g[1]) / 3); b[3] = (byte)((b[0] + 2 * b[1]) / 3);
                    }
                    else
                    {
                        r[2] = (byte)((r[0] + r[1]) / 2); g[2] = (byte)((g[0] + g[1]) / 2); b[2] = (byte)((b[0] + b[1]) / 2);
                        r[3] = 0; g[3] = 0; b[3] = 0;
                    }

                    for (int p = 0; p < 16; p++)
                    {
                        int px = bx * 4 + (p % 4), py = by * 4 + (p / 4);
                        if (px < width && py < height)
                        {
                            int idx = (int)((code >> (p * 2)) & 0x03);
                            int dstIdx = (py * width + px) * 4;
                            dst[dstIdx + 0] = b[idx];
                            dst[dstIdx + 1] = g[idx];
                            dst[dstIdx + 2] = r[idx];
                            dst[dstIdx + 3] = (c0 <= c1 && idx == 3) ? (byte)0 : (byte)255;
                        }
                    }
                    blockOffset += 8;
                }
            }
        }

        private static void DecodeDxt3(byte[] data, int offset, int width, int height, byte[] dst)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int blockOffset = offset;
            byte[] r = new byte[4], g = new byte[4], b = new byte[4];

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (blockOffset + 16 > data.Length) return;
                    ushort c0 = BitConverter.ToUInt16(data, blockOffset + 8);
                    ushort c1 = BitConverter.ToUInt16(data, blockOffset + 10);
                    uint code = BitConverter.ToUInt32(data, blockOffset + 12);

                    DecodeRgb565(c0, out r[0], out g[0], out b[0]);
                    DecodeRgb565(c1, out r[1], out g[1], out b[1]);
                    r[2] = (byte)((2 * r[0] + r[1]) / 3); g[2] = (byte)((2 * g[0] + g[1]) / 3); b[2] = (byte)((2 * b[0] + b[1]) / 3);
                    r[3] = (byte)((r[0] + 2 * r[1]) / 3); g[3] = (byte)((g[0] + 2 * g[1]) / 3); b[3] = (byte)((b[0] + 2 * b[1]) / 3);

                    for (int p = 0; p < 16; p++)
                    {
                        int px = bx * 4 + (p % 4), py = by * 4 + (p / 4);
                        if (px < width && py < height)
                        {
                            int alphaByte = data[blockOffset + (p / 2)];
                            byte alpha = (byte)(((p % 2 == 0) ? (alphaByte & 0x0F) : ((alphaByte >> 4) & 0x0F)) * 255 / 15);
                            int idx = (int)((code >> (p * 2)) & 0x03);
                            int dstIdx = (py * width + px) * 4;
                            dst[dstIdx + 0] = b[idx];
                            dst[dstIdx + 1] = g[idx];
                            dst[dstIdx + 2] = r[idx];
                            dst[dstIdx + 3] = alpha;
                        }
                    }
                    blockOffset += 16;
                }
            }
        }

        private static void DecodeDxt5(byte[] data, int offset, int width, int height, byte[] dst)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int blockOffset = offset;
            byte[] alphas = new byte[8];
            byte[] r = new byte[4], g = new byte[4], b = new byte[4];

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (blockOffset + 16 > data.Length) return;

                    byte a0 = data[blockOffset + 0], a1 = data[blockOffset + 1];
                    alphas[0] = a0; alphas[1] = a1;

                    if (a0 > a1)
                    {
                        alphas[2] = (byte)((6 * a0 + 1 * a1) / 7); alphas[3] = (byte)((5 * a0 + 2 * a1) / 7);
                        alphas[4] = (byte)((4 * a0 + 3 * a1) / 7); alphas[5] = (byte)((3 * a0 + 4 * a1) / 7);
                        alphas[6] = (byte)((2 * a0 + 5 * a1) / 7); alphas[7] = (byte)((1 * a0 + 6 * a1) / 7);
                    }
                    else
                    {
                        alphas[2] = (byte)((4 * a0 + 1 * a1) / 5); alphas[3] = (byte)((3 * a0 + 2 * a1) / 5);
                        alphas[4] = (byte)((2 * a0 + 3 * a1) / 5); alphas[5] = (byte)((1 * a0 + 4 * a1) / 5);
                        alphas[6] = 0; alphas[7] = 255;
                    }

                    ulong alphaBits = (ulong)data[blockOffset + 2] |
                                     ((ulong)data[blockOffset + 3] << 8) |
                                     ((ulong)data[blockOffset + 4] << 16) |
                                     ((ulong)data[blockOffset + 5] << 24) |
                                     ((ulong)data[blockOffset + 6] << 32) |
                                     ((ulong)data[blockOffset + 7] << 40);

                    ushort c0 = BitConverter.ToUInt16(data, blockOffset + 8);
                    ushort c1 = BitConverter.ToUInt16(data, blockOffset + 10);
                    uint code = BitConverter.ToUInt32(data, blockOffset + 12);

                    DecodeRgb565(c0, out r[0], out g[0], out b[0]);
                    DecodeRgb565(c1, out r[1], out g[1], out b[1]);
                    r[2] = (byte)((2 * r[0] + r[1]) / 3); g[2] = (byte)((2 * g[0] + g[1]) / 3); b[2] = (byte)((2 * b[0] + b[1]) / 3);
                    r[3] = (byte)((r[0] + 2 * r[1]) / 3); g[3] = (byte)((g[0] + 2 * g[1]) / 3); b[3] = (byte)((b[0] + 2 * b[1]) / 3);

                    for (int p = 0; p < 16; p++)
                    {
                        int px = bx * 4 + (p % 4), py = by * 4 + (p / 4);
                        if (px < width && py < height)
                        {
                            int alphaIdx = (int)((alphaBits >> (p * 3)) & 0x07);
                            int colorIdx = (int)((code >> (p * 2)) & 0x03);
                            int dstIdx = (py * width + px) * 4;

                            dst[dstIdx + 0] = b[colorIdx];
                            dst[dstIdx + 1] = g[colorIdx];
                            dst[dstIdx + 2] = r[colorIdx];
                            dst[dstIdx + 3] = alphas[alphaIdx];
                        }
                    }
                    blockOffset += 16;
                }
            }
        }
    }
}