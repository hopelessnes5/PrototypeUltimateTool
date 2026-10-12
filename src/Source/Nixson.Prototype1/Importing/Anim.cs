using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;

namespace Nixson.Prototype1.Importing
{
    public static class Anim
    {
        public static byte[] UncompressAnimationData(AnimationData animationData)
        {
            if (animationData == null)
            {
                return new byte[0];
            }
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream(animationData.CompressedData))
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    memoryStream.Position = 2L;
                    byte[] array = new byte[animationData.UncompressedSize];
                    deflateStream.Read(array, 0, (int)animationData.UncompressedSize);
                    result = array;
                }
            }
            return result;
        }
        public static Dictionary<ushort, FrameData> GetFrameData(AnimationBone bone, byte[] animationData)
        {
            Dictionary<ushort, FrameData> dictionary = new Dictionary<ushort, FrameData>();
            foreach (AnimationChannel animationChannel in bone.GetChildNodes<AnimationChannel>())
            {
                if (!animationChannel.ContainsAnimData)
                {
                    animationChannel.ReadFrames(animationData);
                }
                foreach (KeyValuePair<ushort, Vector4> keyValuePair in animationChannel.Frames)
                {
                    FrameData frameData = dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : new FrameData();
                    if (!dictionary.ContainsKey(keyValuePair.Key))
                    {
                        dictionary.Add(keyValuePair.Key, frameData);
                    }
                    Vector4 vector = animationChannel.CalculateValue((float)keyValuePair.Key);
                    string translationType = animationChannel.TranslationType;
                    if (!(translationType == "SCL\0"))
                    {
                        if (!(translationType == "TRAN"))
                        {
                            if (translationType == "ROT\0")
                            {
                                frameData.rotation = vector;
                            }
                        }
                        else
                        {
                            frameData.location = vector;
                        }
                    }
                    else
                    {
                        frameData.scale = vector;
                    }
                }
            }
            return dictionary;
        }
    }
}
