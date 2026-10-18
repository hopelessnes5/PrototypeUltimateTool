using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gibbed.IO;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    // IDs cross-referenced with the original EditP3D format catalogue. These chunks are
    // recognized by ID and preserved as editable raw payloads until a dedicated decoder is
    // available. Unlike Unknown, the node name identifies the known format family.
    [KnownGame(PrototypeGame.Any)]
    [KnownType(0x00010010U)]
    [KnownType(0x00010011U)]
    [KnownType(0x00010019U)]
    [KnownType(0x0001001BU)]
    [KnownType(0x00010021U)]
    [KnownType(0x00011000U)]
    [KnownType(0x00011002U)]
    [KnownType(0x00011003U)]
    [KnownType(0x00011004U)]
    [KnownType(0x00011005U)]
    // IDs below have placeholder readers in the Elan-S/Gibbed sources, but their
    // KnownType attributes are commented out there. Register only as opaque chunks:
    // this surfaces them by ID without trusting the incomplete typed layouts.
    [KnownType(0x00019008U)]
    [KnownType(0x00022004U)]
    [KnownType(0x00022006U)]
    [KnownType(0x00121008U)]
    [KnownType(0x00121010U)]
    [KnownType(0x00122000U)]
    [KnownType(0x02F00001U)]
    [KnownType(0x07020001U)]
    [KnownType(0x07021000U)]
    [KnownType(0x07021004U)]
    [KnownType(0x07021101U)]
    [KnownType(0x07021801U)]
    [KnownType(0x07021802U)]
    [KnownType(0x07022000U)]
    [KnownType(0x07022100U)]
    [KnownType(0x07022101U)]
    [KnownType(0x07022102U)]
    [KnownType(0x07022107U)]
    [KnownType(0x07022200U)]
    [KnownType(0x20000702U)]
    public class RecognizedRawChunk : BaseNode
    {
        private static readonly Dictionary<uint, string> Names = new Dictionary<uint, string>
        {
            { 0x00010010U, "Legacy byte stream" },
            { 0x00010011U, "Legacy primitive group flags" },
            { 0x00010019U, "Legacy primitive group" },
            { 0x0001001BU, "Legacy index record stream" },
            { 0x00010021U, "Legacy primitive data" },
            { 0x00011000U, "Legacy shader / render data" },
            { 0x00011002U, "Legacy shader data 11002" },
            { 0x00011003U, "Legacy shader data 11003" },
            { 0x00011004U, "Legacy shader data 11004" },
            { 0x00011005U, "Legacy shader data 11005" },
            { 0x00019008U, "Unverified legacy texture payload 19008" },
            { 0x00022004U, "Unverified legacy font payload 22004" },
            { 0x00022006U, "Unverified legacy font data 22006" },
            { 0x00121008U, "Animation payload 121008" },
            { 0x00121010U, "Animation payload 121010" },
            { 0x00122000U, "Legacy animation group data" },
            { 0x02F00001U, "Spider-Man 4 payload" },
            { 0x07020001U, "Prototype 2 payload 07020001" },
            { 0x07021000U, "Prototype 2 physics payload 07021000" },
            { 0x07021004U, "Prototype 2 physics payload 07021004" },
            { 0x07021101U, "Prototype 2 physics payload 07021101" },
            { 0x07021801U, "Prototype 2 physics payload 07021801" },
            { 0x07021802U, "Prototype 2 physics payload 07021802" },
            { 0x07022000U, "Prototype 2 physics payload 07022000" },
            { 0x07022100U, "Prototype 2 physics payload 07022100" },
            { 0x07022101U, "Prototype 2 physics payload 07022101" },
            { 0x07022102U, "Prototype 2 physics payload 07022102" },
            { 0x07022107U, "Prototype 2 physics payload 07022107" },
            { 0x07022200U, "Prototype 2 physics payload 07022200" },
            { 0x20000702U, "Fight data payload" }
        };

        [Category("Raw payload")]
        public byte[] Data { get; set; } = new byte[0];

        [Category("Raw payload")]
        [ReadOnly(true)]
        public int PayloadLength => Data == null ? 0 : Data.Length;

        public override string ToString()
        {
            string name;
            if (!Names.TryGetValue(TypeId, out name)) name = "Recognized chunk";
            return name + " (0x" + TypeId.ToString("X8") + ", " + PayloadLength + " bytes)";
        }

        public override void Serialize(Stream output, Endian endian)
        {
            if (Data != null && Data.Length > 0) output.Write(Data, 0, Data.Length);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            long end = (long)StartPosition + HeaderSize;
            long remaining = end - input.Position;
            if (remaining < 0 || remaining > int.MaxValue) throw new FormatException("Invalid known raw chunk payload size.");
            if (remaining > int.MaxValue || (input.CanSeek && remaining > input.Length - input.Position))
                throw new FormatException("Known raw chunk payload exceeds the available bytes.");
            Data = remaining == 0 ? new byte[0] : input.ReadBytes((int)remaining);
            if (Data == null || Data.Length != remaining) throw new EndOfStreamException("Truncated known raw chunk payload.");
        }

        public override bool Exportable => Data != null && Data.Length > 0;
        public override string ExportExtension => "bin";
        public override void Export(Stream output)
        { if (Data != null) output.Write(Data, 0, Data.Length); }
        public override bool Importable => true;
        public override void Import(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            using (var memory = new MemoryStream()) { input.CopyTo(memory); Data = memory.ToArray(); }
        }
    }
}
