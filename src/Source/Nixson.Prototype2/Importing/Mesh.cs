using System;
using System.Collections.Generic;
using System.Linq;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;

namespace Nixson.Prototype2.Importing
{
    public static class Mesh
    {
        public static List<MeshImportData> GetDrawables(Pure3DFile p3d)
        {
            List<P2PolySkinMetadata> list = new List<P2PolySkinMetadata>();
            foreach (CompositeDrawable compositeDrawable in p3d.Nodes.OfType<CompositeDrawable>())
            {
                List<P2PolySkinMetadata> childNodesRecursive = compositeDrawable.GetChildNodesRecursive<P2PolySkinMetadata>();
                list.AddRange(childNodesRecursive);
            }
            List<MeshImportData> list2 = new List<MeshImportData>();
            foreach (P2PolySkinMetadata p2PolySkinMetadata in list)
            {
                MeshImportData meshImportData = new MeshImportData();
                CompositeDrawable compositeDrawable2 = (CompositeDrawable)p2PolySkinMetadata.ParentNode.ParentNode;
                meshImportData.name = compositeDrawable2.Name;
                meshImportData.skeletonName = compositeDrawable2.SkeletonName;
                foreach (P2Primitive p2Primitive in p3d.Nodes.OfType<P2Primitive>())
                {
                    if (p2Primitive.Name == p2PolySkinMetadata.IndicesName)
                    {
                        meshImportData.indices = p2Primitive;
                    }
                    else if (p2Primitive.Name == p2PolySkinMetadata.SkinName)
                    {
                        meshImportData.skin = p2Primitive;
                    }
                    else if (p2Primitive.Name == p2PolySkinMetadata.VerticesName)
                    {
                        meshImportData.vertices = p2Primitive;
                    }
                }
                if (meshImportData.skin == null)
                {
                    meshImportData.skin = meshImportData.vertices;
                }
                list2.Add(meshImportData);
            }
            return list2;
        }
        public static MeshImportData GetDrawable(Pure3DFile p3d, P2PolySkin polySkin)
        {
            P2PolySkinMetadata childNode = polySkin.GetChildNode<P2PolySkinMetadata>();
            MeshImportData meshImportData = new MeshImportData();
            CompositeDrawable compositeDrawable = (CompositeDrawable)childNode.ParentNode.ParentNode;
            meshImportData.name = compositeDrawable.Name;
            meshImportData.skeletonName = compositeDrawable.SkeletonName;
            foreach (P2Primitive p2Primitive in p3d.Nodes.OfType<P2Primitive>())
            {
                if (p2Primitive.Name == childNode.IndicesName)
                {
                    meshImportData.indices = p2Primitive;
                }
                else if (p2Primitive.Name == childNode.SkinName)
                {
                    meshImportData.skin = p2Primitive;
                }
                else if (p2Primitive.Name == childNode.VerticesName)
                {
                    meshImportData.vertices = p2Primitive;
                }
            }
            if (meshImportData.skin == null)
            {
                meshImportData.skin = meshImportData.vertices;
            }
            return meshImportData;
        }
        public static List<T> GetBuffer<T>(DescriptionTypeEnum descriptionType, P2Primitive primitive, bool isUV = false)
        {
            List<T> list = new List<T>();
            if (primitive == null)
            {
                throw new ArgumentNullException(nameof(primitive));
            }

            P2Buffer childNode = primitive.GetChildNode<P2Buffer>();
            if (childNode != null)
            {
                int num = Utils.FindIndexOfDescription(descriptionType, childNode.Description);
                if (num < 0)
                {
                    return list;
                }

                int num2 = 0;
                while ((long)num2 < (long)((ulong)primitive.NumberOfVertices))
                {
                    int num3 = (int)((long)num2 * (long)((ulong)childNode.Description.AmountOfDescriptions) + (long)num);
                    T item = (T)((object)childNode.BufferItems[num3].GetValueP2());
                    list.Add(item);
                    num2++;
                }
                return list;
            }
            throw new Exception("Vertex buffer not found.");
        }
        public static List<Face> GetFaces(P2Primitive primitive)
        {
            List<Face> list = new List<Face>();
            if (primitive == null)
            {
                throw new ArgumentNullException(nameof(primitive));
            }

            P2Buffer childNode = primitive.GetChildNode<P2Buffer>();
            if (childNode == null)
            {
                throw new InvalidOperationException("Index buffer not found.");
            }

            int num = 0;
            while ((long)num < (long)((ulong)(primitive.NumberOfVertices / 3U)))
            {
                ushort[] array = new ushort[3];
                for (int i = 0; i < 3; i++)
                {
                    BufferItem bufferItem = childNode.BufferItems[num * 3 + i];
                    array[i] = BitConverter.ToUInt16(bufferItem.Data, 0);
                }
                list.Add(new Face
                {
                    Point1 = array[0],
                    Point2 = array[1],
                    Point3 = array[2]
                });
                num++;
            }
            return list;
        }
    }
}
