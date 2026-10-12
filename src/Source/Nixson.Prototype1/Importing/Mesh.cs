using System;
using System.Collections.Generic;
using System.Linq;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;
using Nixson.Common;

namespace Nixson.Prototype1.Importing
{
    public static class Mesh
    {
        public static List<T> GetBuffer<T>(DescriptionTypeEnum descriptionType, PrimitiveGroup primitiveGroup, bool isUV = false)
        {
            List<T> list = new List<T>();
            List<VertexBuffer> childNodes = primitiveGroup.GetChildNodes<VertexBuffer>();
            VertexBuffer vertexBuffer = childNodes.Find((VertexBuffer x) => (ulong)x.Param == (ulong)(isUV ? 1L : 2L));
            if (vertexBuffer == null && childNodes.Count > 0)
            {
                vertexBuffer = childNodes[0];
            }
            if (vertexBuffer != null)
            {
                int num = Utils.FindIndexOfDescription(descriptionType, vertexBuffer.Description);
                if (num == -1)
                {
                    return list;
                }
                int num2 = 0;
                while ((long)num2 < (long)((ulong)primitiveGroup.NumVertices))
                {
                    int num3 = (int)((long)num2 * (long)((ulong)vertexBuffer.Description.AmountOfDescriptions) + (long)num);
                    T item = (T)((object)vertexBuffer.BufferItems[num3].GetValueP1());
                    list.Add(item);
                    num2++;
                }
            }
            return list;
        }
        public static List<Face> GetFaces(PrimitiveGroup primitiveGroup)
        {
            IndexBuffer childNode = primitiveGroup.GetChildNode<IndexBuffer>();
            if (childNode != null)
            {
                return childNode.Faces.ToList<Face>();
            }
            throw new Exception("No faces found");
        }
        public static List<uint> GetMatrixIndices(PrimitiveGroup primitiveGroup)
        {
            List<uint> list = new List<uint>();
            PrimitiveMatrix childNode = primitiveGroup.GetChildNode<PrimitiveMatrix>();
            if (childNode == null || childNode.Count < 1U)
            {
                return list;
            }
            int num = 0;
            while ((long)num < (long)((ulong)childNode.Count))
            {
                list.Add(childNode.Indices[num]);
                num++;
            }
            return list;
        }
        public static List<Vector3> GetRawPositionBuffer(PrimitiveGroup primitiveGroup)
        {
            RawPositionBuffer childNode = primitiveGroup.GetChildNode<RawPositionBuffer>();
            if (childNode != null)
            {
                return childNode.Position.ToList<Vector3>();
            }
            throw new Exception("No position vertices found");
        }
        public static List<UVCoordinate> GetRawUVBuffer(PrimitiveGroup primitiveGroup)
        {
            RawUVBuffer childNode = primitiveGroup.GetChildNode<RawUVBuffer>();
            if (childNode != null)
            {
                return childNode.UVs.ToList<UVCoordinate>();
            }
            throw new Exception("No position vertices found");
        }
        public static List<Face> GetRawFaceBuffer(PrimitiveGroup primitiveGroup)
        {
            List<Face> list = new List<Face>();
            RawIndexBuffer childNode = primitiveGroup.GetChildNode<RawIndexBuffer>();
            if (childNode != null)
            {
                for (int i = 0; i < childNode.Faces.Length; i++)
                {
                    list.Add(new Face
                    {
                        Point1 = (ushort)childNode.Faces[i].X,
                        Point2 = (ushort)childNode.Faces[i].Y,
                        Point3 = (ushort)childNode.Faces[i].Z
                    });
                }
                return list;
            }
            throw new Exception("No faces found");
        }
    }
}
