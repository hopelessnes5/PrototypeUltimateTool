using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Gibbed.IO;

namespace Nixson.Common
{
    public static class Utils
    {
        public static List<string> StringToList(Stream input)
        {
            List<string> list = new List<string>();
            int num = input.ReadValueS32();
            string text = "";
            for (int i = 0; i < num; i++)
            {
                char c = (char)input.ReadByte();
                if (c == ';')
                {
                    list.Add(text);
                    text = "";
                }
                else
                {
                    text += c.ToString();
                }
            }
            if (!string.IsNullOrEmpty(text))
            {
                list.Add(text);
            }
            return list;
        }
        public static List<string> StringToList(string str)
        {
            return str.Split(new char[]
            {
                ';'
            }).ToList<string>();
        }
        public static string ListToString(List<string> list)
        {
            string text = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                text += list[i];
                if (i != list.Count - 1)
                {
                    text += ";";
                }
            }
            return text;
        }
        public static T StringToEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }
            T result;
            if (!Enum.TryParse<T>(value, out result))
            {
                throw new FormatException("Invalid enum value");
            }
            return result;
        }
        public static T IntToEnum<T>(int index) where T : struct
        {
            return (T)((object)Enum.ToObject(typeof(T), index));
        }
        public static Stream CompressRZ(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (!input.CanRead || !input.CanSeek)
            {
                throw new ArgumentException("The RZ input stream must be readable and seekable.", nameof(input));
            }

            input.Position = 0L;
            var output = new MemoryStream();
            using (var compressed = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(compressed, CompressionMode.Compress, true))
                {
                    input.CopyTo(deflateStream);
                }

                byte[] data = compressed.ToArray();
                output.WriteValueU32(23122U);
                output.WriteValueU32(0U);
                output.WriteValueS32((int)input.Length);
                output.WriteValueU32(0U);
                output.WriteValueU16(40056);
                output.WriteBytes(data);
                output.WriteValueU32(Utils.Adler32(input), Endian.Big);
            }

            output.Position = 0L;
            return output;
        }

        public static void DecompressRZ(Stream input, Stream output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (!input.CanSeek)
            {
                throw new ArgumentException("The RZ input stream must support seeking.", nameof(input));
            }
            if (input.Length < 18L)
            {
                throw new InvalidDataException("The RZ header is incomplete.");
            }

            // The RZ header is 18 bytes long; the raw DEFLATE payload follows it.
            input.Position = 18L;
            using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress, true))
            {
                deflateStream.CopyTo(output);
            }
        }

        public static bool IsRZFile(string path)
        {
            return !string.IsNullOrEmpty(path) && path.EndsWith(".p3d.rz", StringComparison.OrdinalIgnoreCase);
        }
        private static uint Adler32(Stream input)
        {
            uint num = 1U;
            uint num2 = 0U;
            MemoryStream memoryStream = new MemoryStream();
            input.Position = 0L;
            input.CopyTo(memoryStream);
            byte[] array = memoryStream.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                num = (num + (uint)array[i]) % 65521U;
                num2 = (num2 + num) % 65521U;
            }
            return num2 << 16 | num;
        }
        public static uint RCFStringHash(string str)
        {
            uint num = 0U;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (i != 0 || c != '\\')
                {
                    uint num2 = (uint)((c < 'a') ? (c + 'a' - 'A') : c);
                    num = (num << 5) - num + num2;
                }
            }
            return num;
        }
        public static uint GetUnixEpoch(this DateTime dateTime)
        {
            return (uint)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
