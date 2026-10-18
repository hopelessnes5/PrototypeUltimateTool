using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats.Cement;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats
{
	// Token: 0x02000005 RID: 5
	public class CementFile
	{
		// Token: 0x0600000D RID: 13 RVA: 0x0000227C File Offset: 0x0000047C
		public CementFile()
		{
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000022A5 File Offset: 0x000004A5
		public CementFile(Stream input)
		{
			this.Deserialize(input);
		}

        /// <summary>
        /// Open a CEMENT archive with optional lazy payload loading. Lazy mode is intended for
        /// the editor UI: index tables are parsed immediately, but entry payloads stay backed by
        /// their original file ranges until requested.
        /// </summary>
        public CementFile(Stream input, bool lazyEntryData)
        {
            this.Deserialize(input, lazyEntryData);
        }

		// Token: 0x0600000F RID: 15 RVA: 0x000022D8 File Offset: 0x000004D8
		public Metadata GetMetadata(uint hash)
		{
			// A hash may match more than one metadata path. Preserve the legacy "first match"
			// behavior instead of throwing for a valid collision. The UI uses all metadata
			// rows directly when it needs to keep duplicates distinguishable.
			return this.Metadatas.FirstOrDefault((Metadata candidate) => candidate.Name.HashFileName() == hash);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002309 File Offset: 0x00000509
		public int EstimateHeaderSize()
		{
			return ((0 + 24 + 8 + 28 + this.EstimateEntryTableSize()).Align(2048) + this.EstimateMetadataTableSize()).Align(2048);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002336 File Offset: 0x00000536
		public int EstimateEntryTableSize()
		{
			return this.Entries.Sum((Entry e) => Entry.ByteSize);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002362 File Offset: 0x00000562
		public int EstimateMetadataTableSize()
		{
			return 8 + this.Metadatas.Sum((Metadata e) => e.ByteSize);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002390 File Offset: 0x00000590
		public void Serialize(Stream output)
		{
            // Keep table sizes in sync without forcing lazy entries to allocate their entire payload.
            // Duplicate hashes are preserved in a stable order.
            this.EntryCount = (uint)this.Entries.Count;
            foreach (Entry entry in this.Entries)
            {
                long length = entry.DataLength;
                if (length > uint.MaxValue) throw new InvalidDataException("CEMENT entry exceeds the format's 32-bit size limit.");
                entry.Size = (uint)length;
            }
            List<Entry> sortedEntries = this.Entries.OrderBy(entry => entry.Hash).ToList();
			output.WriteString("ATG CORE CEMENT LIBRARY", 24, Encoding.ASCII);
			output.Seek(8L, SeekOrigin.Current);
			output.WriteValueU8(this.MajorVersion);
			output.WriteValueU8(this.MinorVersion);
			output.WriteValueB8(this.Endian != Endian.Little);
			output.WriteValueU8(1);
			int num = 60;
			int num2 = this.EstimateEntryTableSize();
			output.WriteValueS32(60, this.Endian);
			output.WriteValueS32(num2, this.Endian);
			num += num2;
			num = num.Align(2048);
			int value = this.EstimateMetadataTableSize();
			output.WriteValueS32(num, this.Endian);
			output.WriteValueS32(value, this.Endian);
			output.WriteValueU32(0U, this.Endian);
			output.WriteValueS32(this.Entries.Count, this.Endian);
			foreach (Entry entry in sortedEntries)
			{
				entry.Serialize(output, this.Endian);
			}
			output.Seek(output.Position.Align(2048L), SeekOrigin.Begin);
			output.WriteValueU32(2048U, Endian.Little);
			output.WriteValueU32(0U, Endian.Little);
			foreach (Metadata metadata in this.Metadatas)
			{
				metadata.Serialize(output, this.Endian);
			}
			output.MoveToAlign(2048);
			for (int i = 0; i < sortedEntries.Count; i++)
			{
                long dataOffset = output.Position;
                if (dataOffset > uint.MaxValue) throw new InvalidDataException("CEMENT archive exceeds the format's 32-bit offset limit.");
				output.Position = (long)(60 + i * Entry.ByteSize + 4);
				output.WriteValueS32((int)dataOffset, this.Endian);
				output.Position = dataOffset;
                sortedEntries[i].CopyDataTo(output);
				output.MoveToAlign(2048);
                sortedEntries[i].Offset = (uint)dataOffset;
			}

            // Only bind the entries to the newly serialized file after the entire write succeeds.
            // The MainForm saves archives to a temporary path first, so the original source remains
            // readable even when the user overwrites the currently opened RCF.
            FileStream outputFile = output as FileStream;
            if (outputFile != null)
                this.RebindEntriesToSource(outputFile.Name);
		}

        public void RebindEntriesToSource(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return;
            foreach (Entry entry in this.Entries)
                entry.BindSource(sourcePath, entry.Offset, entry.Size, true);
        }

		// Token: 0x06000014 RID: 20 RVA: 0x000025C8 File Offset: 0x000007C8
		public void Deserialize(Stream input)
        {
            this.Deserialize(input, false);
        }

        public void Deserialize(Stream input, bool lazyEntryData)
		{
            // Permit reusing a CementFile instance and archives containing duplicate hashes.
            this.Entries.Clear();
            this.EntriesDict.Clear();
            this.Metadatas.Clear();
			if (input.ReadString(24, true, Encoding.ASCII) != "ATG CORE CEMENT LIBRARY")
			{
				throw new FormatException("Not a Cement file");
			}
			input.ReadBytes(8);
			this.MajorVersion = input.ReadValueU8();
			this.MinorVersion = input.ReadValueU8();
			this.Endian = ((!input.ReadValueB8()) ? Endian.Little : Endian.Big);
			input.ReadValueU8();
			uint num = input.ReadValueU32(this.Endian);
			uint size = input.ReadValueU32(this.Endian);
			uint num2 = input.ReadValueU32(this.Endian);
			uint size2 = input.ReadValueU32(this.Endian);
			input.ReadValueU32();
			this.EntryCount = input.ReadValueU32(this.Endian);
			input.Seek((long)((ulong)num), SeekOrigin.Begin);
			using (MemoryStream memoryStream = input.ReadToMemoryStream((int)size))
			{
				this.Entries.Clear();
				int num3 = 0;
				while ((long)num3 < (long)((ulong)this.EntryCount))
				{
					Entry entry = new Entry();
					entry.Deserialize(memoryStream, this.Endian);
					this.Entries.Add(entry);
                    // Hashes are not unique identifiers: collisions (and duplicated files)
                    // exist in shipped CEMENT libraries. Keep the first for legacy callers,
                    // while preserving every entry in Entries for unpacking.
                    if (!this.EntriesDict.ContainsKey(entry.Hash))
                        this.EntriesDict.Add(entry.Hash, entry);
					num3++;
				}
				if (memoryStream.Position != memoryStream.Length)
				{
					throw new FormatException();
				}
			}
			input.Seek((long)((ulong)num2), SeekOrigin.Begin);
			using (MemoryStream memoryStream2 = input.ReadToMemoryStream((int)size2))
			{
				memoryStream2.ReadValueU32(Endian.Little);
				memoryStream2.ReadValueU32(Endian.Little);
				this.Metadatas.Clear();
				int num4 = 0;
				while ((long)num4 < (long)((ulong)this.EntryCount))
				{
					Metadata metadata = new Metadata();
					metadata.Deserialize(memoryStream2, this.Endian);
					this.Metadatas.Add(metadata);
					num4++;
				}
			}
            string sourcePath = null;
            FileStream sourceFile = input as FileStream;
            if (lazyEntryData && sourceFile != null && !string.IsNullOrWhiteSpace(sourceFile.Name))
                sourcePath = sourceFile.Name;

			foreach (Entry entry2 in this.Entries)
			{
                if (!string.IsNullOrEmpty(sourcePath))
                {
                    // Keep only the file range in memory. Entry.Data is materialized only for
                    // operations that truly need a byte array; scanners use OpenDataRead().
                    entry2.BindSource(sourcePath, entry2.Offset, entry2.Size, true);
                }
                else
                {
                    if (entry2.Size > int.MaxValue)
                        throw new InvalidDataException("CEMENT entry exceeds the maximum byte-array size.");
				    input.Seek((long)((ulong)entry2.Offset), SeekOrigin.Begin);
				    entry2.Data = input.ReadBytes((int)entry2.Size);
                }
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002808 File Offset: 0x00000A08
		public void Pack(string path)
		{
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = files[i].Substring(path.Length + 1, files[i].Length - path.Length - 1);
			}
			this.EntryCount = (uint)files.Length;
			int num = 0;
			while ((long)num < (long)((ulong)this.EntryCount))
			{
				Entry entry = new Entry
				{
					Hash = Utils.RCFStringHash(files[num])
				};
				string sourceFilePath = Path.GetFullPath(Path.Combine(path, files[num]));
                using (FileStream fileStream = File.OpenRead(sourceFilePath))
                {
                    if (fileStream.Length > uint.MaxValue)
                        throw new InvalidDataException("CEMENT entries are limited to 32-bit sizes.");
                    // Keep source files as stream-backed entries until Serialize writes the new RCF.
                    // Packing a large directory no longer holds every file payload in RAM at once.
                    entry.BindSource(sourceFilePath, 0, fileStream.Length, true);
                }
				this.Entries.Add(entry);
				Metadata item = new Metadata
				{
					Date = DateTime.Now.GetUnixEpoch(),
					Name = files[num]
				};
				this.Metadatas.Add(item);
				num++;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002934 File Offset: 0x00000B34
		public void Unpack(string path)
		{
            // A CEMENT file indexes entries by a 32-bit path hash, not by a unique ID.
            // Build buckets so duplicate hashes do not throw and every duplicate entry can
            // be consumed in order when metadata contains the corresponding paths.
            var entriesByHash = this.Entries.GroupBy(e => e.Hash)
                .ToDictionary(g => g.Key, g => g.ToList());
            var usedByHash = new Dictionary<uint, int>();
			int num = 0;
			while ((long)num < (long)((ulong)this.EntryCount))
			{
				uint key = Utils.RCFStringHash(this.Metadatas[num].Name.Trim(new char[1]));
                List<Entry> matches;
                if (!entriesByHash.TryGetValue(key, out matches) || matches.Count == 0)
                    throw new InvalidDataException("RCF metadata has no matching entry hash for: " + this.Metadatas[num].Name);
                int used;
                usedByHash.TryGetValue(key, out used);
                Entry entry = matches[Math.Min(used, matches.Count - 1)];
                usedByHash[key] = used + 1;
				string path2 = this.Metadatas[num].Name.Trim(new char[1]);
				string directoryName = Path.GetDirectoryName(path2);
				using (FileStream fileStream = File.Create(Path.Combine(Directory.CreateDirectory(Path.Combine(path, directoryName)).FullName, Path.GetFileName(path2))))
				{
                    entry.CopyDataTo(fileStream);
				}
				num++;
			}
		}

		// Token: 0x04000004 RID: 4
		public Endian Endian;

		// Token: 0x04000005 RID: 5
		public byte MajorVersion;

		// Token: 0x04000006 RID: 6
		public byte MinorVersion;

		// Token: 0x04000007 RID: 7
		public uint EntryCount;

		// Token: 0x04000008 RID: 8
		public readonly List<Entry> Entries = new List<Entry>();

		// Token: 0x04000009 RID: 9
		public readonly Dictionary<uint, Entry> EntriesDict = new Dictionary<uint, Entry>();

		// Token: 0x0400000A RID: 10
		public readonly List<Metadata> Metadatas = new List<Metadata>();
	}
}
