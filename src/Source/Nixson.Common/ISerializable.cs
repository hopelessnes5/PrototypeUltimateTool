using System;
using System.IO;
using Gibbed.IO;

namespace Nixson.Common
{
    public interface ISerializable
    {
        void Serialize(Stream output, Endian endian);
        void Deserialize(Stream input, Endian endian);
    }
}
