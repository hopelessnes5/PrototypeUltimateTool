using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    /// <summary>
    /// Legacy P3D one-degree-of-freedom channel type 0x00121102. Its stored values use
    /// the same float payload layout as Vector1DOFChannel; the distinct chunk ID is kept
    /// so NodeFactory can parse and round-trip both variants used by EditP3D assets.
    /// </summary>
    [KnownGame(PrototypeGame.Any)]
    [KnownType(1184002U)]
    public class Vector1DOFCompressedChannel : Vector1DOFChannel
    {
    }
}
