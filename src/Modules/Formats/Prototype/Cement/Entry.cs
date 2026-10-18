using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Prototype.FileFormats.Cement
{
    /// <summary>
    /// CEMENT archive entry. Entries read from an existing archive can keep only a file-range
    /// reference until the payload is actually requested; this avoids loading every archive
    /// payload into a managed byte array when an RCF is opened for indexing.
    /// </summary>
    public class Entry
    {
        private byte[] _data;
        private string _sourcePath;
        private long _sourceOffset;
        private long _sourceLength;
        private bool _sourceBacked;
        private long _dataRevision;

        public static int ByteSize { get { return 12; } }

        public uint Hash;
        public uint Offset;
        public uint Size;

        /// <summary>Revision changes whenever payload bytes are replaced, not when merely loaded.</summary>
        public long DataRevision { get { return _dataRevision; } }

        /// <summary>True if payload bytes exist either in memory or in the original archive file.</summary>
        public bool HasData { get { return _data != null || _sourceBacked; } }

        /// <summary>True when a source-backed payload may be re-read without retaining its array.</summary>
        public bool IsSourceBacked { get { return _sourceBacked; } }

        /// <summary>Payload length without forcing a lazy entry to load.</summary>
        public long DataLength
        {
            get
            {
                if (_data != null) return _data.LongLength;
                if (_sourceBacked) return _sourceLength;
                return 0;
            }
        }

        /// <summary>
        /// Compatibility API for existing editor code. Reading Data loads only this entry;
        /// archive-wide scanners should use OpenDataRead instead.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (_data == null && _sourceBacked) _data = ReadSourceData();
                return _data;
            }
            set
            {
                _data = value;
                Size = (uint)(value == null ? 0 : value.LongLength);
                _sourcePath = null;
                _sourceOffset = 0;
                _sourceLength = 0;
                _sourceBacked = false;
                unchecked { _dataRevision++; }
            }
        }

        public Stream OpenDataRead()
        {
            if (_data != null) return new MemoryStream(_data, false);
            if (_sourceBacked)
            {
                var file = new FileStream(_sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                    1024 * 1024, FileOptions.SequentialScan);
                try { return new BoundedReadStream(file, _sourceOffset, _sourceLength); }
                catch { file.Dispose(); throw; }
            }
            return new MemoryStream(new byte[0], false);
        }

        public void CopyDataTo(Stream output)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (_data != null)
            {
                output.Write(_data, 0, _data.Length);
                return;
            }
            if (!_sourceBacked) return;
            using (Stream input = OpenDataRead()) input.CopyTo(output);
        }

        /// <summary>Drop an unmodified, source-backed byte array after a temporary operation.</summary>
        public void ReleaseLoadedData()
        {
            if (_sourceBacked) _data = null;
        }

        /// <summary>Bind the entry to a range in an archive file.</summary>
        public void BindSource(string path, long offset, long length, bool releaseLoadedData = false)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A source path is required.", "path");
            if (offset < 0 || length < 0) throw new ArgumentOutOfRangeException(offset < 0 ? "offset" : "length");
            _sourcePath = Path.GetFullPath(path);
            _sourceOffset = offset;
            _sourceLength = length;
            _sourceBacked = true;
            Size = checked((uint)length);
            if (releaseLoadedData) _data = null;
        }

        private byte[] ReadSourceData()
        {
            if (_sourceLength > int.MaxValue)
                throw new InvalidDataException("This archive entry is too large to materialize in a single byte array.");
            byte[] result = new byte[(int)_sourceLength];
            using (Stream input = OpenDataRead())
            {
                int offset = 0;
                while (offset < result.Length)
                {
                    int read = input.Read(result, offset, result.Length - offset);
                    if (read <= 0) throw new EndOfStreamException("The source archive entry is truncated.");
                    offset += read;
                }
            }
            return result;
        }

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32(this.Hash, endian);
            output.WriteValueU32(this.Offset, endian);
            output.WriteValueU32(this.Size, endian);
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.Hash = input.ReadValueU32(endian);
            this.Offset = input.ReadValueU32(endian);
            this.Size = input.ReadValueU32(endian);
            _data = null;
            _sourcePath = null;
            _sourceOffset = 0;
            _sourceLength = 0;
            _sourceBacked = false;
            _dataRevision = 0;
        }

        private sealed class BoundedReadStream : Stream
        {
            private readonly Stream _inner;
            private readonly long _start;
            private readonly long _length;
            private long _position;

            public BoundedReadStream(Stream inner, long start, long length)
            {
                if (inner == null || !inner.CanSeek || !inner.CanRead) throw new ArgumentException("A seekable readable stream is required.");
                if (start < 0 || length < 0 || start > inner.Length || length > inner.Length - start)
                    throw new EndOfStreamException("The archive entry range is outside the source file.");
                _inner = inner;
                _start = start;
                _length = length;
                _inner.Position = _start;
                _position = 0;
            }

            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return true; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { return _length; } }
            public override long Position
            {
                get { return _position; }
                set { Seek(value, SeekOrigin.Begin); }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (offset < 0 || count < 0 || offset > buffer.Length - count) throw new ArgumentOutOfRangeException();
                if (_position >= _length || count == 0) return 0;
                int allowed = (int)Math.Min((long)count, _length - _position);
                _inner.Position = _start + _position;
                int read = _inner.Read(buffer, offset, allowed);
                _position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long next;
                switch (origin)
                {
                    case SeekOrigin.Begin: next = offset; break;
                    case SeekOrigin.Current: next = _position + offset; break;
                    case SeekOrigin.End: next = _length + offset; break;
                    default: throw new ArgumentOutOfRangeException("origin");
                }
                if (next < 0 || next > _length) throw new IOException("Seek lies outside the archive entry.");
                _position = next;
                _inner.Position = _start + _position;
                return _position;
            }

            public override void Flush() { }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
            protected override void Dispose(bool disposing)
            {
                if (disposing) _inner.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
