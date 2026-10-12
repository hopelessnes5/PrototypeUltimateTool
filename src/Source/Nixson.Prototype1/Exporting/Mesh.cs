using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;
using Nixson.Common;

namespace Nixson.Prototype1.Exporting
{
    public static class Mesh
    {
        public static PrimitiveGroup CreatePrimitiveGroup(Endian endian, List<Vector3> position, List<Vector3Int> faces, List<Vector3> normal, List<float> weights, List<byte> groups, List<UVCoordinate> uvs, List<uint> skeletonGroups)
        {
            PrimitiveGroup primitiveGroup = new PrimitiveGroup();
            primitiveGroup.MemoryImaged = 1U;
            primitiveGroup.Optimized = 1U;
            primitiveGroup.Version = 0U;
            primitiveGroup.VertexAnimated = 0U;
            primitiveGroup.VertexAnimationMask = 0U;
            primitiveGroup.VertexType = 45489U;
            primitiveGroup.NumIndices = (uint)(faces.Count * 3);
            primitiveGroup.NumVertices = (uint)position.Count;
            primitiveGroup.NumMatrices = (uint)skeletonGroups.Count;
            MeshExportFlags meshExportFlags = MeshExportFlags.Position;
            MeshData meshData = new MeshData();
            meshData.position = position;
            if (weights != null)
            {
                meshExportFlags |= MeshExportFlags.Weight;
                List<Vector3> list = new List<Vector3>();
                for (int i = 0; i < weights.Count; i += 3)
                {
                    list.Add(new Vector3
                    {
                        X = weights[i],
                        Y = weights[i + 1],
                        Z = weights[i + 2]
                    });
                }
                meshData.weight = list;
            }
            if (groups != null)
            {
                meshExportFlags |= MeshExportFlags.Group;
                List<byte[]> list2 = new List<byte[]>();
                for (int j = 0; j < groups.Count; j += 4)
                {
                    list2.Add(new byte[]
                    {
                        groups[j],
                        groups[j + 1],
                        groups[j + 2],
                        groups[j + 2]
                    });
                }
                meshData.group = list2;
            }
            if (uvs != null)
            {
                meshData.uv = uvs;
            }
            if (normal != null)
            {
                meshExportFlags |= MeshExportFlags.Normal;
                meshData.normal = normal;
            }
            primitiveGroup.Children.Add(Mesh.CreateVertexDescriptor(position.Count, meshExportFlags, false));
            primitiveGroup.Children.Add(Mesh.CreateVertexDescriptor(position.Count, MeshExportFlags.UV | MeshExportFlags.Padding, true));
            primitiveGroup.Children.Add(Mesh.CreateVertexBuffer(meshExportFlags, meshData, false, endian));
            primitiveGroup.Children.Add(Mesh.CreateVertexBuffer(MeshExportFlags.UV | MeshExportFlags.Padding, meshData, true, endian));
            primitiveGroup.Children.Add(Mesh.CreateIndexBuffer(faces));
            primitiveGroup.Children.Add(Mesh.CreatePrimitiveMatrix(skeletonGroups));
            return primitiveGroup;
        }
        public static PrimitiveGroup CreatePrimitiveGroupGeometry(Endian endian, List<Vector3> position, List<Vector3Int> faces, List<Vector3> normal, List<UVCoordinate> uvs)
        {
            PrimitiveGroup primitiveGroup = new PrimitiveGroup();
            primitiveGroup.MemoryImaged = 1U;
            primitiveGroup.Optimized = 1U;
            primitiveGroup.Version = 0U;
            primitiveGroup.VertexAnimated = 0U;
            primitiveGroup.VertexAnimationMask = 0U;
            primitiveGroup.VertexType = 45489U;
            primitiveGroup.NumIndices = (uint)(faces.Count * 3);
            primitiveGroup.NumVertices = (uint)position.Count;
            primitiveGroup.NumMatrices = 0U;
            MeshExportFlags meshExportFlags = MeshExportFlags.Position;
            MeshData meshData = new MeshData();
            meshData.position = position;
            if (uvs != null)
            {
                meshExportFlags |= MeshExportFlags.UV;
                meshData.uv = uvs;
            }
            if (normal != null)
            {
                meshExportFlags |= MeshExportFlags.Normal;
                meshData.normal = normal;
            }
            primitiveGroup.Children.Add(Mesh.CreateVertexDescriptor4(position.Count, meshExportFlags, false));
            primitiveGroup.Children.Add(Mesh.CreateVertexBuffer4(meshExportFlags, meshData, false, endian));
            primitiveGroup.Children.Add(Mesh.CreateIndexBuffer(faces));
            return primitiveGroup;
        }
        public static VertexDescriptionList CreateVertexDescriptor(int vertices, MeshExportFlags flags, bool uv)
        {
            uint num = 0U;
            uint num2 = 0U;
            List<VertexDescription> list = new List<VertexDescription>();
            if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.UVPadding1),
                    Unknown1 = 7U,
                    Unknown2 = 16778240U
                });
                num += 4U;
                num2 += 4U;
            }
            if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Position),
                    Unknown1 = 0U,
                    Unknown2 = 0U
                });
                num += 12U;
                num2 += 12U;
            }
            if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.UV),
                    Unknown1 = 0U,
                    Unknown2 = 512U
                });
                num += 8U;
                num2 += 8U;
            }
            if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Normal),
                    Unknown1 = 0U,
                    Unknown2 = 16777984U
                });
                num += 12U;
                num2 += 12U;
            }
            if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Weight),
                    Unknown1 = 0U,
                    Unknown2 = 0U
                });
                num += 12U;
                num2 += 12U;
            }
            if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Group),
                    Unknown1 = 0U,
                    Unknown2 = 0U
                });
                num += 4U;
                num2 += 4U;
            }
            foreach (VertexDescription vertexDescription in list)
            {
                vertexDescription.VertexObjectSize = num2;
            }
            VertexDescriptionList vertexDescriptionList = new VertexDescriptionList();
            vertexDescriptionList.Version = 131073U;
            vertexDescriptionList.Param = (uv ? 1U : 2U);
            vertexDescriptionList.AmountOfDescriptions = (uint)list.Count;
            vertexDescriptionList.AmountOfDescriptions *= 17U;
            vertexDescriptionList.BufferSize = (uint)((ulong)num2 * (ulong)((long)vertices));
            vertexDescriptionList.Descriptions = list.ToArray();
            return vertexDescriptionList;
        }
        public static VertexDescriptionList CreateVertexDescriptor4(int vertices, MeshExportFlags flags, bool uv)
        {
            uint num = 0U;
            uint num2 = 0U;
            List<VertexDescription> list = new List<VertexDescription>();
            if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.UVPadding1),
                    Unknown1 = 7U,
                    Unknown2 = 16778240U
                });
                num += 4U;
                num2 += 4U;
            }
            if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Position),
                    Unknown1 = 0U,
                    Unknown2 = 1024U
                });
                num += 16U;
                num2 += 16U;
            }
            if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.UV),
                    Unknown1 = 0U,
                    Unknown2 = 512U
                });
                num += 8U;
                num2 += 8U;
            }
            if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Normal),
                    Unknown1 = 0U,
                    Unknown2 = 16777984U
                });
                num += 12U;
                num2 += 12U;
            }
            if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Weight),
                    Unknown1 = 0U,
                    Unknown2 = 0U
                });
                num += 12U;
                num2 += 12U;
            }
            if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
            {
                list.Add(new VertexDescription
                {
                    Offset = num,
                    BufferType = new DescriptionType(DescriptionTypeEnum.Group),
                    Unknown1 = 0U,
                    Unknown2 = 0U
                });
                num += 4U;
                num2 += 4U;
            }
            foreach (VertexDescription vertexDescription in list)
            {
                vertexDescription.VertexObjectSize = num2;
            }
            VertexDescriptionList vertexDescriptionList = new VertexDescriptionList();
            vertexDescriptionList.Version = 131073U;
            vertexDescriptionList.Param = (uv ? 1U : 2U);
            vertexDescriptionList.AmountOfDescriptions = (uint)list.Count;
            vertexDescriptionList.AmountOfDescriptions *= 17U;
            vertexDescriptionList.BufferSize = (uint)((ulong)num2 * (ulong)((long)vertices));
            vertexDescriptionList.Descriptions = list.ToArray();
            return vertexDescriptionList;
        }
        public static VertexBuffer CreateVertexBuffer4(MeshExportFlags flags, MeshData data, bool uv, Endian endian)
        {
            List<BufferItem> list = new List<BufferItem>();
            uint num = 0U;
            for (int i = 0; i < data.position.Count; i++)
            {
                if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
                {
                    list.Add(new BufferItem(endian)
                    {
                        Data = new byte[]
                        {
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue
                        }
                    });
                    num += 4U;
                }
                if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
                {
                    BufferItem bufferItem = new BufferItem(endian);
                    MemoryStream memoryStream = new MemoryStream();
                    data.position[i].Serialize(memoryStream, endian);
                    byte[] array = new byte[]
                    {
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                    };
                    array[14] = 0;
                    array[13] = 0;
                    array[12] = 0;
                    byte[] array2 = memoryStream.ToArray();
                    for (int j = 0; j < array2.Length; j++)
                    {
                        array[j] = array2[j];
                    }
                    bufferItem.Data = array;
                    list.Add(bufferItem);
                    num += 16U;
                }
                if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
                {
                    BufferItem bufferItem2 = new BufferItem(endian);
                    MemoryStream memoryStream2 = new MemoryStream();
                    data.uv[i].Serialize(memoryStream2, endian);
                    bufferItem2.Data = memoryStream2.ToArray();
                    list.Add(bufferItem2);
                    num += 8U;
                }
                if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
                {
                    BufferItem bufferItem3 = new BufferItem(endian);
                    MemoryStream memoryStream3 = new MemoryStream();
                    data.normal[i].Serialize(memoryStream3, endian);
                    bufferItem3.Data = memoryStream3.ToArray();
                    list.Add(bufferItem3);
                    num += 12U;
                }
                if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
                {
                    BufferItem bufferItem4 = new BufferItem(endian);
                    MemoryStream memoryStream4 = new MemoryStream();
                    data.weight[i].Serialize(memoryStream4, endian);
                    bufferItem4.Data = memoryStream4.ToArray();
                    list.Add(bufferItem4);
                    num += 12U;
                }
                if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
                {
                    list.Add(new BufferItem(endian)
                    {
                        Data = data.group[i]
                    });
                    num += 4U;
                }
            }
            return new VertexBuffer
            {
                Version = 131073U,
                Param = (uv ? 1U : 2U),
                BufferSize = num,
                BufferItems = list.ToArray()
            };
        }
        public static VertexBuffer CreateVertexBuffer(MeshExportFlags flags, MeshData data, bool uv, Endian endian)
        {
            List<BufferItem> list = new List<BufferItem>();
            uint num = 0U;
            for (int i = 0; i < data.position.Count; i++)
            {
                if ((flags & MeshExportFlags.Padding) == MeshExportFlags.Padding)
                {
                    list.Add(new BufferItem(endian)
                    {
                        Data = new byte[]
                        {
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue
                        }
                    });
                    num += 4U;
                }
                if ((flags & MeshExportFlags.Position) == MeshExportFlags.Position)
                {
                    BufferItem bufferItem = new BufferItem(endian);
                    MemoryStream memoryStream = new MemoryStream();
                    data.position[i].Serialize(memoryStream, endian);
                    bufferItem.Data = memoryStream.ToArray();
                    list.Add(bufferItem);
                    num += 12U;
                }
                if ((flags & MeshExportFlags.Normal) == MeshExportFlags.Normal)
                {
                    BufferItem bufferItem2 = new BufferItem(endian);
                    MemoryStream memoryStream2 = new MemoryStream();
                    data.normal[i].Serialize(memoryStream2, endian);
                    bufferItem2.Data = memoryStream2.ToArray();
                    list.Add(bufferItem2);
                    num += 12U;
                }
                if ((flags & MeshExportFlags.Weight) == MeshExportFlags.Weight)
                {
                    BufferItem bufferItem3 = new BufferItem(endian);
                    MemoryStream memoryStream3 = new MemoryStream();
                    data.weight[i].Serialize(memoryStream3, endian);
                    bufferItem3.Data = memoryStream3.ToArray();
                    list.Add(bufferItem3);
                    num += 12U;
                }
                if ((flags & MeshExportFlags.UV) == MeshExportFlags.UV)
                {
                    BufferItem bufferItem4 = new BufferItem(endian);
                    MemoryStream memoryStream4 = new MemoryStream();
                    data.uv[i].Serialize(memoryStream4, endian);
                    bufferItem4.Data = memoryStream4.ToArray();
                    list.Add(bufferItem4);
                    num += 8U;
                }
                if ((flags & MeshExportFlags.Group) == MeshExportFlags.Group)
                {
                    list.Add(new BufferItem(endian)
                    {
                        Data = data.group[i]
                    });
                    num += 4U;
                }
            }
            return new VertexBuffer
            {
                Version = 131073U,
                Param = (uv ? 1U : 2U),
                BufferSize = num,
                BufferItems = list.ToArray()
            };
        }
        public static IndexBuffer CreateIndexBuffer(List<Vector3Int> faces)
        {
            IndexBuffer indexBuffer = new IndexBuffer();
            indexBuffer.Version = 131073U;
            indexBuffer.Param = 0U;
            indexBuffer.BufferSize = (uint)(faces.Count * 6);
            indexBuffer.Faces = new Face[faces.Count];
            for (int i = 0; i < faces.Count; i++)
            {
                Face face = new Face();
                face.Point1 = (ushort)faces[i].X;
                face.Point2 = (ushort)faces[i].Y;
                face.Point3 = (ushort)faces[i].Z;
                indexBuffer.Faces[i] = face;
            }
            return indexBuffer;
        }
        public static PrimitiveMatrix CreatePrimitiveMatrix(List<uint> skeletonGroups)
        {
            return new PrimitiveMatrix
            {
                Count = (uint)skeletonGroups.Count,
                Indices = skeletonGroups.ToArray()
            };
        }
    }
}
