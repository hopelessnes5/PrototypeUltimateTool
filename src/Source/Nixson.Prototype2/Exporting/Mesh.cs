using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;
using Nixson.Common;

namespace Nixson.Prototype2.Exporting
{
    public static class Mesh
    {
        public static P2Primitive CreatePrimitiveIndices(Endian endianess, string name, List<Vector3Int> faces)
        {
            P2Primitive p2Primitive = new P2Primitive();
            p2Primitive.ID = 65601U;
            p2Primitive.Version = 0U;
            p2Primitive.Name = name + "_indices";
            p2Primitive.Unknown1 = 0U;
            p2Primitive.Unknown2 = 1U;
            p2Primitive.NumberOfVertices = (uint)(faces.Count * 3);
            p2Primitive.Unknown3 = 0U;
            p2Primitive.Unknown4 = (uint)faces.Count;
            p2Primitive.Children.Add(Mesh.CreateFaceDescriptor(faces.Count, MeshExportFlags.Group));
            List<BufferItem> list = new List<BufferItem>();
            for (int i = 0; i < faces.Count; i++)
            {
                list.Add(new BufferItem(endianess)
                {
                    Data = BitConverter.GetBytes((ushort)faces[i].X)
                });
                list.Add(new BufferItem(endianess)
                {
                    Data = BitConverter.GetBytes((ushort)faces[i].Y)
                });
                list.Add(new BufferItem(endianess)
                {
                    Data = BitConverter.GetBytes((ushort)faces[i].Z)
                });
            }
            P2Buffer p2Buffer = new P2Buffer();
            p2Buffer.BufferSize = p2Primitive.NumberOfVertices * 2U;
            p2Buffer.BufferItems = list.ToArray();
            p2Primitive.Children.Add(p2Buffer);
            return p2Primitive;
        }
        public static P2Primitive CreatePrimitiveSkin(Endian endianess, string name, List<Vector3> position, List<Vector3> normal, List<float> weights, List<byte> groups)
        {
            P2Primitive p2Primitive = new P2Primitive();
            p2Primitive.ID = 65600U;
            p2Primitive.Version = 0U;
            p2Primitive.Name = name + "_skin";
            p2Primitive.Unknown1 = 1U;
            p2Primitive.Unknown2 = 1U;
            p2Primitive.NumberOfVertices = (uint)position.Count;
            p2Primitive.Unknown3 = 4U;
            p2Primitive.Unknown4 = (uint)position.Count;
            MeshExportFlags meshExportFlags = MeshExportFlags.Position;
            MeshExportData meshExportData = new MeshExportData();
            meshExportData.vertices = position.Count;
            meshExportData.position = position;
            if (weights != null)
            {
                meshExportFlags |= MeshExportFlags.Weight;
                List<Vector4> list = new List<Vector4>();
                for (int i = 0; i < weights.Count; i += 4)
                {
                    list.Add(new Vector4
                    {
                        X = weights[i],
                        Y = weights[i + 1],
                        Z = weights[i + 2],
                        W = weights[i + 3]
                    });
                }
                meshExportData.weight = list;
            }
            if (groups != null)
            {
                meshExportFlags |= MeshExportFlags.Group;
                List<byte[]> list2 = new List<byte[]>();
                for (int j = 0; j < groups.Count; j += 4)
                {
                    byte[] array = new byte[]
                    {
                        groups[j],
                        0,
                        groups[j + 1],
                        0,
                        groups[j + 2],
                        0,
                        groups[j + 3],
                        0
                    };
                    if (array[4] <= 0)
                    {
                        array[4] = array[0];
                    }
                    if (array[6] <= 0)
                    {
                        array[6] = array[0];
                    }
                    list2.Add(array);
                }
                meshExportData.group = list2;
            }
            if (normal != null)
            {
                meshExportFlags |= MeshExportFlags.Normal;
                meshExportData.normal = normal;
            }
            meshExportFlags = (meshExportFlags | MeshExportFlags.Padding | MeshExportFlags.Tangent);
            P2BufferDescriptor p2BufferDescriptor = Mesh.CreateVertexDescriptor(position.Count, meshExportFlags);
            p2Primitive.Children.Add(p2BufferDescriptor);
            p2Primitive.Children.Add(Mesh.CreateVertexBuffer(endianess, meshExportFlags, meshExportData, p2BufferDescriptor));
            return p2Primitive;
        }
        public static P2Primitive CreatePrimitiveVertices(Endian endianess, string name, List<UVCoordinate> uvs)
        {
            P2Primitive p2Primitive = new P2Primitive();
            p2Primitive.ID = 65600U;
            p2Primitive.Version = 0U;
            p2Primitive.Name = name + "_vertices";
            p2Primitive.Unknown1 = 0U;
            p2Primitive.Unknown2 = 1U;
            p2Primitive.NumberOfVertices = (uint)uvs.Count;
            p2Primitive.Unknown3 = 4U;
            p2Primitive.Unknown4 = (uint)uvs.Count;
            MeshExportFlags flags = (MeshExportFlags)288;
            MeshExportData meshExportData = new MeshExportData();
            meshExportData.vertices = uvs.Count;
            meshExportData.uv = uvs;
            P2BufferDescriptor p2BufferDescriptor = Mesh.CreateVertexDescriptor(uvs.Count, flags);
            p2Primitive.Children.Add(p2BufferDescriptor);
            p2Primitive.Children.Add(Mesh.CreateVertexBuffer(endianess, flags, meshExportData, p2BufferDescriptor));
            return p2Primitive;
        }
        public static P2BufferDescriptor CreateFaceDescriptor(int vertices, MeshExportFlags flags)
        {
            uint num = 0U;
            uint num2 = 0U;
            List<P2Description> list = new List<P2Description>();
            if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Group),
                    BufferSize = (uint)(vertices * 3),
                    Unknown1 = 3U,
                    Unknown2 = 1U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 2U;
                num2 += 2U;
            }
            foreach (P2Description p2Description in list)
            {
                p2Description.ItemSize = num2;
            }
            return new P2BufferDescriptor
            {
                AmountOfDescriptions = (uint)list.Count,
                DescriptionSize = num2,
                Descriptions = list.ToArray()
            };
        }
        public static P2BufferDescriptor CreateVertexDescriptor(int vertices, MeshExportFlags flags)
        {
            uint num = 0U;
            uint num2 = 0U;
            List<P2Description> list = new List<P2Description>();
            if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Position),
                    Unknown1 = 0U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 16U;
                num2 += 16U;
            }
            if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Normal),
                    Unknown1 = 0U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 16U;
                num2 += 16U;
            }
            if ((flags & MeshExportFlags.Tangent) == MeshExportFlags.Tangent)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Tangent),
                    Unknown1 = 0U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 16U;
                num2 += 16U;
            }
            if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Weight),
                    Unknown1 = 0U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 16U;
                num2 += 16U;
            }
            if ((flags & MeshExportFlags.Colour) == MeshExportFlags.Colour)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Colour0),
                    Unknown1 = 7U,
                    Unknown2 = 4U,
                    Unknown5 = 1U,
                    Unknown6 = 0U
                });
                num += 4U;
                num2 += 4U;
            }
            if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.UV),
                    Unknown1 = 0U,
                    Unknown2 = 2U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 8U;
                num2 += 8U;
            }
            if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Group),
                    Unknown1 = 3U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 8U;
                num2 += 8U;
            }
            if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
            {
                list.Add(new P2Description
                {
                    Offset = num,
                    BufferSize = (uint)vertices,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Padding1),
                    Unknown1 = 3U,
                    Unknown2 = 4U,
                    Unknown5 = 0U,
                    Unknown6 = 0U
                });
                num += 8U;
                num2 += 8U;
            }
            foreach (P2Description p2Description in list)
            {
                p2Description.ItemSize = num2;
            }
            return new P2BufferDescriptor
            {
                AmountOfDescriptions = (uint)list.Count,
                DescriptionSize = num2,
                Descriptions = list.ToArray()
            };
        }
        public static P2Buffer CreateVertexBuffer(Endian endianess, MeshExportFlags flags, MeshExportData data, P2BufferDescriptor desc)
        {
            List<BufferItem> list = new List<BufferItem>();
            uint num = 0U;
            for (int i = 0; i < data.vertices; i++)
            {
                if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
                {
                    BufferItem bufferItem = new BufferItem(endianess);
                    MemoryStream memoryStream = new MemoryStream();
                    new Vector4
                    {
                        X = data.position[i].X,
                        Y = data.position[i].Y,
                        Z = data.position[i].Z,
                        W = 1f
                    }.Serialize(memoryStream, endianess);
                    bufferItem.Data = memoryStream.ToArray();
                    bufferItem.BufferType = new DescriptionType(DescriptionTypeEnum.Position);
                    list.Add(bufferItem);
                    num += 16U;
                }
                if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
                {
                    BufferItem bufferItem2 = new BufferItem(endianess);
                    MemoryStream memoryStream2 = new MemoryStream();
                    new Vector4
                    {
                        X = data.normal[i].X,
                        Y = data.normal[i].Y,
                        Z = data.normal[i].Z,
                        W = 1f
                    }.Serialize(memoryStream2, endianess);
                    bufferItem2.Data = memoryStream2.ToArray();
                    bufferItem2.BufferType = new DescriptionType(DescriptionTypeEnum.Normal);
                    list.Add(bufferItem2);
                    num += 16U;
                }
                if ((flags & MeshExportFlags.Tangent) == MeshExportFlags.Tangent)
                {
                    BufferItem bufferItem3 = new BufferItem(endianess);
                    MemoryStream memoryStream3 = new MemoryStream();
                    new Vector4
                    {
                        X = 0f,
                        Y = 0f,
                        Z = 0f,
                        W = 0f
                    }.Serialize(memoryStream3, endianess);
                    bufferItem3.Data = memoryStream3.ToArray();
                    bufferItem3.BufferType = new DescriptionType(DescriptionTypeEnum.Tangent);
                    list.Add(bufferItem3);
                    num += 16U;
                }
                if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
                {
                    BufferItem bufferItem4 = new BufferItem(endianess);
                    MemoryStream memoryStream4 = new MemoryStream();
                    data.weight[i].Serialize(memoryStream4, endianess);
                    bufferItem4.Data = memoryStream4.ToArray();
                    bufferItem4.BufferType = new DescriptionType(DescriptionTypeEnum.Weight);
                    list.Add(bufferItem4);
                    num += 16U;
                }
                if ((flags & MeshExportFlags.Colour) == MeshExportFlags.Colour)
                {
                    BufferItem bufferItem5 = new BufferItem(endianess);
                    BufferItem bufferItem6 = bufferItem5;
                    byte[] array = new byte[4];
                    array[0] = byte.MaxValue;
                    array[2] = byte.MaxValue;
                    bufferItem6.Data = array;
                    bufferItem5.BufferType = new DescriptionType(DescriptionTypeEnum.Colour0);
                    list.Add(bufferItem5);
                    num += 4U;
                }
                if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
                {
                    BufferItem bufferItem7 = new BufferItem(endianess);
                    MemoryStream memoryStream5 = new MemoryStream();
                    data.uv[i].Serialize(memoryStream5, endianess);
                    bufferItem7.Data = memoryStream5.ToArray();
                    bufferItem7.BufferType = new DescriptionType(DescriptionTypeEnum.UV);
                    list.Add(bufferItem7);
                    num += 8U;
                }
                if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
                {
                    list.Add(new BufferItem(endianess)
                    {
                        Data = data.group[i],
                        BufferType = new DescriptionType(DescriptionTypeEnum.Group)
                    });
                    num += 8U;
                }
                if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
                {
                    BufferItem bufferItem8 = new BufferItem(endianess);
                    MemoryStream memoryStream6 = new MemoryStream();
                    new Vector2
                    {
                        X = 0f,
                        Y = 0f
                    }.Serialize(memoryStream6, endianess);
                    bufferItem8.Data = memoryStream6.ToArray();
                    bufferItem8.BufferType = new DescriptionType(DescriptionTypeEnum.Padding1);
                    list.Add(bufferItem8);
                    num += 8U;
                }
            }
            return new P2Buffer
            {
                BufferSize = (uint)((long)data.vertices * (long)((ulong)desc.DescriptionSize)),
                BufferItems = list.ToArray()
            };
        }
    }
}
