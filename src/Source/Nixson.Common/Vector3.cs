using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Gibbed.IO;

namespace Nixson.Common
{
    [TypeConverter(typeof(MyTypeConverter))]
    [DataContract(Namespace = "http://datacontract.gib.me/prototype")]
    public class Vector3 : ISerializable
    {
        [DataMember(Name = "x", Order = 1)]
        [Descriptable]
        public float X { get; set; }
        [DataMember(Name = "y", Order = 1)]
        [Descriptable]
        public float Y { get; set; }
        [DataMember(Name = "z", Order = 1)]
        [Descriptable]
        public float Z { get; set; }
        public override string ToString()
        {
            return string.Format("Vector3: X = {0} | Y = {1} | Z = {2}", this.X, this.Y, this.Z);
        }
        public Vector3()
        {
        }
        public Vector3(Stream input, Endian endian)
        {
            this.Deserialize(input, endian);
        }
        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueF32(this.X, endian);
            output.WriteValueF32(this.Y, endian);
            output.WriteValueF32(this.Z, endian);
        }
        public void Deserialize(Stream input, Endian endian)
        {
            this.X = input.ReadValueF32(endian);
            this.Y = input.ReadValueF32(endian);
            this.Z = input.ReadValueF32(endian);
        }
    }
}
