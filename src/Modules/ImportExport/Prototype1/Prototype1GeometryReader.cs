using System;
using System.Collections.Generic;
using System.Linq;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;
using GameVector3 = Nixson.Common.Vector3;

namespace PrototypeUltimateTool.Modules.ImportExport.Prototype1
{
    /// <summary>One triangle from a Prototype 1 primitive group.</summary>
    public struct TriangleIndices
    {
        public int A { get; private set; }
        public int B { get; private set; }
        public int C { get; private set; }

        public TriangleIndices(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    /// <summary>Decoded, renderer-independent geometry for one Prototype 1 PrimitiveGroup.</summary>
    public sealed class Prototype1GeometryData
    {
        public PrimitiveGroup Source { get; internal set; }
        public string ShaderName { get; internal set; }
        public List<GameVector3> Positions { get; internal set; } = new List<GameVector3>();
        public List<GameVector3> Normals { get; internal set; } = new List<GameVector3>();
        public List<UVCoordinate> UVs { get; internal set; } = new List<UVCoordinate>();
        public List<GameVector3> Weights { get; internal set; } = new List<GameVector3>();
        public List<byte[]> Groups { get; internal set; } = new List<byte[]>();
        public List<TriangleIndices> Triangles { get; internal set; } = new List<TriangleIndices>();

        public bool HasRenderableGeometry
        {
            get { return Positions != null && Positions.Count > 0 && Triangles != null && Triangles.Count > 0; }
        }
    }

    /// <summary>
    /// Prototype 1 geometry reader adapted from Elan-S/EditP3D. It supports both regular vertex/index buffers
    /// and Nixson's raw-buffer representation, and doesn't depend on OpenGL or a specific renderer.
    /// </summary>
    public static class Prototype1GeometryReader
    {
        public sealed class PrimitiveGroupSource
        {
            public PrimitiveGroup Group { get; private set; }
            public string OwnerName { get; private set; }
            /// <summary>Rigid bone index from a CompositeDrawable reference for legacy Geometry chunks; -1 means weighted or unbound.</summary>
            public int RigidBoneIndex { get; private set; }

            internal PrimitiveGroupSource(PrimitiveGroup group, string ownerName, int rigidBoneIndex = -1)
            {
                Group = group;
                OwnerName = ownerName ?? string.Empty;
                RigidBoneIndex = rigidBoneIndex;
            }
        }

        public static Prototype1GeometryData Read(PrimitiveGroup group)
        {
            var result = new Prototype1GeometryData
            {
                Source = group,
                ShaderName = group == null ? null : group.ShaderName
            };

            if (group == null)
                return result;

            bool hasRawPositions = group.GetChildNodes<RawPositionBuffer>().Count > 0;
            result.Positions = ReadPositions(group);
            // A P1 PrimitiveGroup can contain more than one historical UV representation.
            // Do not blindly accept the first RawUVBuffer: a mismatched count shifts the UV-to-vertex
            // mapping or makes the viewer discard all coordinates. Prefer a buffer whose count matches
            // the position stream selected above.
            result.UVs = ReadUvs(group, result.Positions.Count);
            result.Normals = ReadNormals(group);
            // Skin streams must use the exact same per-vertex indexing as the selected position stream.
            // Legacy containers can contain stale raw attribute buffers next to a newer position stream.
            List<GameVector3> skinWeights;
            List<byte[]> skinGroups;
            ReadSkinStreams(group, result.Positions.Count, out skinWeights, out skinGroups);
            result.Weights = skinWeights;
            result.Groups = skinGroups;
            result.Triangles = ReadTriangles(group, result.Positions.Count, hasRawPositions);
            return result;
        }

        /// <summary>Resolve the selected node to geometry, including CompositeDrawable references to sibling nodes.</summary>
        public static List<PrimitiveGroupSource> ResolveGroups(BaseNode selected, Pure3DFile file)
        {
            var result = new List<PrimitiveGroupSource>();
            if (selected == null)
                return result;

            var allNodes = Flatten(file == null ? null : file.Nodes).ToList();
            var seen = new HashSet<PrimitiveGroup>();

            Action<BaseNode, string, int> addSubtree = null;
            addSubtree = (node, ownerName, rigidBoneIndex) =>
            {
                if (node == null)
                    return;

                var group = node as PrimitiveGroup;
                if (group != null)
                {
                    if (seen.Add(group))
                        result.Add(new PrimitiveGroupSource(group, ownerName, rigidBoneIndex));
                    return;
                }

                if (node.Children == null)
                    return;

                foreach (var child in node.Children)
                    addSubtree(child, ownerName, rigidBoneIndex);
            };

            var directGroup = selected as PrimitiveGroup;
            if (directGroup != null)
            {
                addSubtree(directGroup, GetName(selected), -1);
                return result;
            }

            var polySkin = selected as PolySkin;
            if (polySkin != null)
            {
                addSubtree(polySkin, GetName(polySkin), -1);
                return result;
            }

            var geometry = selected as Geometry;
            if (geometry != null)
            {
                addSubtree(geometry, GetName(geometry), -1);
                return result;
            }

            var composite = selected as CompositeDrawable;
            if (composite != null)
            {
                string compositeName = GetName(composite);
                // Some files embed a few geometry nodes directly under the composite. Keep those,
                // then resolve the reference chunks to sibling PolySkin/Geometry nodes in the file.
                addSubtree(composite, compositeName, -1);

                var candidates = allNodes
                    .Where(n => (n is PolySkin || n is Geometry) && ContainsPrimitiveGroup(n))
                    .ToList();
                var references = Flatten(new[] { composite })
                    .OfType<CompositeDrawablePolySkinReference>()
                    .Where(r => !string.IsNullOrWhiteSpace(r.PolySkinName))
                    .ToList();

                int resolvedReferences = 0;
                foreach (var reference in references)
                {
                    // Resolve every reference independently. A composite may resolve most parts
                    // exactly while one name differs slightly; a global "result.Count == 0"
                    // fallback silently loses those remaining parts.
                    var targets = FindCompositeTargets(reference.PolySkinName, candidates, false);
                    if (targets.Count == 0)
                    {
                        targets = FindCompositeTargets(reference.PolySkinName, candidates, true);
                    }

                    if (targets.Count > 0)
                    {
                        resolvedReferences++;
                        foreach (var target in targets)
                        {
                            // EditP3D treats CompositeDrawable references to Geometry as rigidly bound
                            // to Unknown5, while PolySkin normally gets its per-vertex weights.
                            int rigidBoneIndex = -1;
                            if (target is Geometry && reference.Unknown5 <= int.MaxValue)
                                rigidBoneIndex = (int)reference.Unknown5;
                            addSubtree(target, GetName(target), rigidBoneIndex);
                        }
                    }
                }

                // Legacy fallback for files with no usable references at all. Keep it last-resort
                // only; otherwise it can pull unrelated geometry whose name happens to overlap.
                if (references.Count == 0 && result.Count == 0 && candidates.Count > 0)
                {
                    foreach (var target in FindLegacyCompositeFallbackTargets(compositeName, candidates))
                        addSubtree(target, GetName(target), -1);
                }
                else if (resolvedReferences == 0 && result.Count == 0 && references.Count > 0)
                {
                    // If no reference could be resolved, retain the old compatibility fallback,
                    // but only in this all-or-nothing failure case.
                    foreach (var target in FindLegacyCompositeFallbackTargets(compositeName, candidates))
                        addSubtree(target, GetName(target), -1);
                }
                return result;
            }

            var referenceNode = selected as CompositeDrawablePolySkinReference;
            if (referenceNode != null && !string.IsNullOrWhiteSpace(referenceNode.PolySkinName))
            {
                var candidates = allNodes
                    .Where(n => (n is PolySkin || n is Geometry) && ContainsPrimitiveGroup(n))
                    .ToList();
                var targets = FindCompositeTargets(referenceNode.PolySkinName, candidates, false);
                if (targets.Count == 0)
                    targets = FindCompositeTargets(referenceNode.PolySkinName, candidates, true);
                foreach (var target in targets)
                {
                    int rigidBoneIndex = target is Geometry && referenceNode.Unknown5 <= int.MaxValue
                        ? (int)referenceNode.Unknown5
                        : -1;
                    addSubtree(target, GetName(target), rigidBoneIndex);
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves a CompositeDrawable reference against PolySkin nodes first (the order used by
        /// EditP3D), then Geometry nodes. Approximate matching is reserved for unresolved names.
        /// </summary>
        private static List<BaseNode> FindLegacyCompositeFallbackTargets(string compositeName, List<BaseNode> candidates)
        {
            var result = new List<BaseNode>();
            string compositeKey = MakeNodeNameKey(compositeName);
            if (compositeKey.Length < 5 || candidates == null)
                return result;

            // This fallback is used only when the composite has no usable references (or every
            // reference failed). Keep both node types here; unlike normal reference resolution,
            // this is a best-effort search for older files without usable links.
            foreach (var candidate in candidates)
            {
                string candidateKey = MakeNodeNameKey(GetName(candidate));
                if (IsPlausiblePartialNameMatch(compositeKey, candidateKey))
                    result.Add(candidate);
            }
            return result;
        }

        private static bool ContainsPrimitiveGroup(BaseNode node)
        {
            if (node == null)
                return false;
            if (node is PrimitiveGroup)
                return true;
            if (node.Children == null)
                return false;

            foreach (var child in node.Children)
            {
                if (ContainsPrimitiveGroup(child))
                    return true;
            }
            return false;
        }

        private static List<BaseNode> FindCompositeTargets(
            string referenceName,
            List<BaseNode> candidates,
            bool allowPartialMatch)
        {
            var result = new List<BaseNode>();
            if (string.IsNullOrWhiteSpace(referenceName) || candidates == null || candidates.Count == 0)
                return result;

            string wanted = NormalizeNodeName(referenceName);
            if (wanted.Length == 0)
                return result;

            // First use a whitespace/NUL-cleaned exact comparison.
            var exactPolySkins = candidates.OfType<PolySkin>()
                .Where(n => string.Equals(NormalizeNodeName(GetName(n)), wanted, StringComparison.OrdinalIgnoreCase))
                .Cast<BaseNode>().ToList();
            if (exactPolySkins.Count > 0)
                return exactPolySkins;

            var exactGeometries = candidates.OfType<Geometry>()
                .Where(n => string.Equals(NormalizeNodeName(GetName(n)), wanted, StringComparison.OrdinalIgnoreCase))
                .Cast<BaseNode>().ToList();
            if (exactGeometries.Count > 0)
                return exactGeometries;

            // Names in older assets sometimes differ only by punctuation/spacing, e.g. "part_l"
            // vs "part-l". Canonical matching remains exact and is safe to do before fuzzy lookup.
            string wantedKey = MakeNodeNameKey(wanted);
            if (wantedKey.Length == 0)
                return result;

            var keyedPolySkins = candidates.OfType<PolySkin>()
                .Where(n => string.Equals(MakeNodeNameKey(GetName(n)), wantedKey, StringComparison.OrdinalIgnoreCase))
                .Cast<BaseNode>().ToList();
            if (keyedPolySkins.Count > 0)
                return keyedPolySkins;

            var keyedGeometries = candidates.OfType<Geometry>()
                .Where(n => string.Equals(MakeNodeNameKey(GetName(n)), wantedKey, StringComparison.OrdinalIgnoreCase))
                .Cast<BaseNode>().ToList();
            if (keyedGeometries.Count > 0)
                return keyedGeometries;

            if (!allowPartialMatch || wantedKey.Length < 5)
                return result;

            // Only use partial matching for this still-unresolved reference. It is deliberately
            // conservative (minimum length 5) and prefers PolySkin candidates just like EditP3D.
            var partialPolySkins = candidates.OfType<PolySkin>()
                .Where(n => IsPlausiblePartialNameMatch(wantedKey, MakeNodeNameKey(GetName(n))))
                .Cast<BaseNode>().ToList();
            if (partialPolySkins.Count > 0)
                return partialPolySkins;

            return candidates.OfType<Geometry>()
                .Where(n => IsPlausiblePartialNameMatch(wantedKey, MakeNodeNameKey(GetName(n))))
                .Cast<BaseNode>().ToList();
        }

        private static bool IsPlausiblePartialNameMatch(string wantedKey, string candidateKey)
        {
            if (string.IsNullOrEmpty(wantedKey) || string.IsNullOrEmpty(candidateKey) ||
                wantedKey.Length < 5 || candidateKey.Length < 5)
                return false;

            // Avoid matching very generic names against arbitrary longer identifiers.
            return candidateKey.IndexOf(wantedKey, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   wantedKey.IndexOf(candidateKey, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string MakeNodeNameKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
            return new string(chars);
        }

        private static string NormalizeNodeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return value.Trim().Trim('\0', ' ', '\t', '\r', '\n').Trim();
        }

        private static List<GameVector3> ReadPositions(PrimitiveGroup group)
        {
            var raw = group.GetChildNodes<RawPositionBuffer>().FirstOrDefault();
            if (raw != null && raw.Position != null && raw.Position.Length > 0 && raw.Position.All(IsFinite))
                return raw.Position.ToList();

            var structured = ReadStructuredValues<GameVector3>(group, DescriptionTypeEnum.Position, 2);
            return structured.All(IsFinite) ? structured : new List<GameVector3>();
        }

        private static List<UVCoordinate> ReadUvs(PrimitiveGroup group, int expectedVertexCount)
        {
            if (group == null || expectedVertexCount <= 0)
                return new List<UVCoordinate>();

            // Raw UV streams are preferred only when they actually correspond one-to-one with
            // the selected position stream. Some files contain an auxiliary/legacy UV buffer
            // whose size differs from the drawable vertex count.
            var raw = group.GetChildNodes<RawUVBuffer>()
                .Where(candidate => candidate != null && candidate.UVs != null &&
                    candidate.UVs.Length == expectedVertexCount && candidate.UVs.All(IsFinite))
                .FirstOrDefault();
            if (raw != null)
                return raw.UVs.ToList();

            // Fall back to an interleaved VertexBuffer. For UV extraction, match the actual
            // position count rather than truncating by PrimitiveGroup.NumVertices, which is
            // inconsistent in some older P1 files.
            var structured = ReadStructuredValues<UVCoordinate>(
                group, DescriptionTypeEnum.UV, 1, expectedVertexCount);
            return structured.Count == expectedVertexCount && structured.All(IsFinite)
                ? structured
                : new List<UVCoordinate>();
        }

        private static List<GameVector3> ReadNormals(PrimitiveGroup group)
        {
            var raw = group.GetChildNodes<RawNormalBuffer>().FirstOrDefault();
            if (raw != null && raw.Normals != null && raw.Normals.Length > 0 && raw.Normals.All(IsFinite))
                return raw.Normals.ToList();

            var structured = ReadStructuredValues<GameVector3>(group, DescriptionTypeEnum.Normal, 2);
            return structured.All(IsFinite) ? structured : new List<GameVector3>();
        }

        private static void ReadSkinStreams(
            PrimitiveGroup group,
            int expectedVertexCount,
            out List<GameVector3> weights,
            out List<byte[]> groups)
        {
            weights = new List<GameVector3>();
            groups = new List<byte[]>();
            if (group == null || expectedVertexCount <= 0)
                return;

            // Skin weights and group indices are a paired vertex stream. Never mix a raw weight
            // buffer with a structured group buffer (or vice versa): even individually valid arrays
            // can use different layouts and assign the wrong bone to a vertex.
            var rawWeights = group.GetChildNodes<RawWeightBuffer>().FirstOrDefault();
            var rawGroups = group.GetChildNodes<RawGroupBuffer>().FirstOrDefault();
            bool hasExactRawPair = rawWeights != null && rawWeights.Weights != null &&
                rawWeights.Weights.Length == expectedVertexCount && rawWeights.Weights.All(IsFinite) &&
                rawGroups != null && rawGroups.Groups != null && rawGroups.Groups.Length == expectedVertexCount &&
                rawGroups.Groups.All(item => item != null && item.Length >= 4);

            if (hasExactRawPair)
            {
                weights = rawWeights.Weights.ToList();
                groups = rawGroups.Groups.ToList();
                return;
            }

            // Fall back to the structured pair only when both streams exactly match the selected
            // position array. Otherwise leave skinning data absent so the caller can choose a safe
            // rigid binding instead of applying mismatched influences.
            var structuredWeights = ReadStructuredValues<GameVector3>(
                group, DescriptionTypeEnum.Weight, 2, expectedVertexCount);
            var structuredGroups = ReadStructuredValues<byte[]>(
                group, DescriptionTypeEnum.Group, 2, expectedVertexCount);
            bool hasExactStructuredPair = structuredWeights.Count == expectedVertexCount &&
                structuredWeights.All(IsFinite) && structuredGroups.Count == expectedVertexCount &&
                structuredGroups.All(item => item != null && item.Length >= 3);
            if (hasExactStructuredPair)
            {
                weights = structuredWeights;
                groups = structuredGroups;
            }
        }

        private static List<T> ReadStructuredValues<T>(PrimitiveGroup group, DescriptionTypeEnum type, uint preferredParam, int expectedCount = -1)
        {
            var result = new List<T>();
            if (group == null)
                return result;

            var buffers = group.GetChildNodes<VertexBuffer>()
                .OrderBy(v => v.Param == preferredParam ? 0 : 1)
                .ToList();
            List<T> best = result;

            foreach (var buffer in buffers)
            {
                var description = ResolveDescription(group, buffer);
                if (description == null || description.Descriptions == null || description.Descriptions.Length == 0 ||
                    buffer.BufferItems == null || buffer.BufferItems.Length == 0)
                    continue;

                int elementIndex = -1;
                for (int i = 0; i < description.Descriptions.Length; i++)
                {
                    var itemDescription = description.Descriptions[i];
                    if (itemDescription != null && itemDescription.BufferType != null && itemDescription.BufferType.EnumValue == type)
                    {
                        elementIndex = i;
                        break;
                    }
                }
                if (elementIndex < 0)
                    continue;

                int stride = description.Descriptions.Length;
                if (stride <= 0)
                    continue;

                int count = buffer.BufferItems.Length / stride;
                if (expectedCount >= 0)
                {
                    // Attribute arrays used for rendering must line up with the position array.
                    // Keep the entire buffer count here: NumVertices may be stale in legacy files.
                    if (count != expectedCount)
                        continue;
                }
                else if (group.NumVertices > 0)
                {
                    count = Math.Min(count, (int)Math.Min((uint)int.MaxValue, group.NumVertices));
                }
                if (count <= 0)
                    continue;

                var values = new List<T>(count);
                bool complete = true;
                for (int vertex = 0; vertex < count; vertex++)
                {
                    int itemIndex = vertex * stride + elementIndex;
                    if (itemIndex < 0 || itemIndex >= buffer.BufferItems.Length || buffer.BufferItems[itemIndex] == null)
                    {
                        complete = false;
                        break;
                    }

                    object value;
                    try { value = buffer.BufferItems[itemIndex].GetValueP1(); }
                    catch { complete = false; break; }

                    if (!(value is T))
                    {
                        complete = false;
                        break;
                    }
                    values.Add((T)value);
                }

                if (complete && values.Count > best.Count)
                    best = values;

                // For UVs, an exact vertex-count match is mandatory. For other attributes,
                // preserve the historical preference for a stream covering declared vertices.
                if (complete && (expectedCount >= 0
                    ? values.Count == expectedCount
                    : (group.NumVertices == 0 || values.Count >= group.NumVertices)))
                    return values;
            }

            return best;
        }

        private static VertexDescriptionList ResolveDescription(PrimitiveGroup group, VertexBuffer buffer)
        {
            if (buffer == null)
                return null;

            if (buffer.Description != null && buffer.Description.Param == buffer.Param)
                return buffer.Description;

            var description = group.GetChildNodes<VertexDescriptionList>()
                .FirstOrDefault(d => d.Param == buffer.Param);
            if (description != null)
                buffer.Description = description;
            return description;
        }

        private static List<TriangleIndices> ReadTriangles(PrimitiveGroup group, int vertexCount, bool preferRaw)
        {
            var result = new List<TriangleIndices>();
            if (group == null || vertexCount <= 0)
                return result;

            var raw = group.GetChildNodes<RawIndexBuffer>().FirstOrDefault();
            var standard = group.GetChildNodes<IndexBuffer>().FirstOrDefault();

            if (preferRaw && raw != null && raw.Faces != null)
                AddRawTriangles(result, raw.Faces, vertexCount);
            else if (standard != null && standard.Faces != null)
                AddStandardTriangles(result, standard.Faces, vertexCount);
            else if (raw != null && raw.Faces != null)
                AddRawTriangles(result, raw.Faces, vertexCount);

            return result;
        }

        private static void AddStandardTriangles(List<TriangleIndices> target, IEnumerable<Face> faces, int vertexCount)
        {
            foreach (var face in faces)
            {
                if (face == null)
                    continue;
                AddTriangleIfValid(target, face.Point1, face.Point2, face.Point3, vertexCount);
            }
        }

        private static void AddRawTriangles(List<TriangleIndices> target, IEnumerable<Vector3Int> faces, int vertexCount)
        {
            foreach (var face in faces)
            {
                if (face == null || face.X > int.MaxValue || face.Y > int.MaxValue || face.Z > int.MaxValue)
                    continue;
                AddTriangleIfValid(target, (int)face.X, (int)face.Y, (int)face.Z, vertexCount);
            }
        }

        private static void AddTriangleIfValid(List<TriangleIndices> target, int a, int b, int c, int vertexCount)
        {
            if (a < 0 || b < 0 || c < 0 || a >= vertexCount || b >= vertexCount || c >= vertexCount)
                return;
            target.Add(new TriangleIndices(a, b, c));
        }

        private static bool IsFinite(GameVector3 value)
        {
            return value != null && IsFinite(value.X) && IsFinite(value.Y) && IsFinite(value.Z);
        }

        private static bool IsFinite(UVCoordinate value)
        {
            return value != null && IsFinite(value.U) && IsFinite(value.V);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static IEnumerable<BaseNode> Flatten(IEnumerable<BaseNode> nodes)
        {
            if (nodes == null)
                yield break;

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;
                yield return node;
                if (node.Children != null)
                {
                    foreach (var child in Flatten(node.Children))
                        yield return child;
                }
            }
        }

        private static string GetName(BaseNode node)
        {
            if (node == null)
                return string.Empty;
            var property = node.GetType().GetProperty("Name");
            if (property != null)
            {
                try
                {
                    var name = property.GetValue(node, null) as string;
                    if (!string.IsNullOrWhiteSpace(name))
                        return name.Trim('\0', ' ', '\t', '\r', '\n');
                }
                catch { }
            }

            var text = node.ToString() ?? string.Empty;
            int open = text.IndexOf('(');
            int close = text.LastIndexOf(')');
            return open >= 0 && close > open ? text.Substring(open + 1, close - open - 1) : text;
        }
    }
}
