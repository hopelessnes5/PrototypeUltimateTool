using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nixson.Common;

namespace Gibbed.Prototype.FileFormats.Pure3D
{
    internal static class NodeFactory
    {
        private static volatile Dictionary<uint, Type> _p1Lookup;
        private static volatile Dictionary<uint, Type> _p2Lookup;
        private static volatile Dictionary<Type, Func<BaseNode>> _nodeFactories;
        private static readonly object _lookupBuildLock = new object();
        private static readonly MethodInfo _genericNodeFactoryMethod =
            typeof(NodeFactory).GetMethod(nameof(CreateNodeGeneric), BindingFlags.Static | BindingFlags.NonPublic);
        private static bool IsLookupBuild => _p1Lookup != null && _p2Lookup != null && _nodeFactories != null;

        private static void BuildLookup()
        {
            // Build into local dictionaries and publish only complete registries. Import/dump
            // can now parse on a worker while another operation is resolving node types.
            var p1Lookup = new Dictionary<uint, Type>();
            var p2Lookup = new Dictionary<uint, Type>();
            var registrations = new List<Tuple<Type, PrototypeGame, bool, KnownTypeAttribute[]>>();
            foreach (Type type in Assembly.GetAssembly(typeof(NodeFactory)).GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(typeof(BaseNode))) continue;
                var knownTypes = (KnownTypeAttribute[])type.GetCustomAttributes(typeof(KnownTypeAttribute), false);
                if (knownTypes.Length == 0) continue;

                var knownGame = (KnownGameAttribute)type.GetCustomAttribute(typeof(KnownGameAttribute), false);
                // Some classes in the original Gibbed/EditP3D format catalogue intentionally
                // omit KnownGame. Treat those legacy definitions as Prototype 1 instead of
                // silently excluding them from the registry.
                bool hadGameAttribute = knownGame != null;
                PrototypeGame game = knownGame != null ? knownGame.Game : PrototypeGame.P1;
                registrations.Add(Tuple.Create(type, game, hadGameAttribute, knownTypes));
            }

            // Register explicit P1/P2 definitions first, then shared definitions, then legacy
            // definitions without KnownGame. This lets a game-specific definition win over a
            // shared fallback when IDs overlap (for example ScaleformData/TextBibleStorage).
            foreach (var item in registrations.OrderBy(x => x.Item3 ? (x.Item2 == PrototypeGame.Any ? 1 : 0) : 2))
            {
                Type type = item.Item1;
                PrototypeGame game = item.Item2;
                foreach (KnownTypeAttribute knownType in item.Item4)
                {
                    if (game == PrototypeGame.P1 || game == PrototypeGame.Any)
                        Register(p1Lookup, knownType.Id, type);
                    if (game == PrototypeGame.P2 || game == PrototypeGame.Any)
                        Register(p2Lookup, knownType.Id, type);
                }
            }

            // Prepare constructor delegates once, outside the hot per-chunk path. Most chunk
            // classes expose a public parameterless constructor and can use a closed generic
            // new T() delegate instead of repeated Activator.CreateInstance reflection.
            // Types that do not satisfy the new() constraint retain the exact Activator fallback.
            var nodeFactories = new Dictionary<Type, Func<BaseNode>>();
            foreach (Type type in p1Lookup.Values.Concat(p2Lookup.Values).Distinct())
            {
                nodeFactories[type] = CreateNodeFactory(type);
            }

            // Publish the completed immutable caches only after all derived data is ready.
            _nodeFactories = nodeFactories;
            _p1Lookup = p1Lookup;
            _p2Lookup = p2Lookup;
        }

        private static void Register(Dictionary<uint, Type> lookup, uint id, Type type)
        {
            if (!lookup.ContainsKey(id)) lookup.Add(id, type);
        }

        public static BaseNode CreateNode(uint typeId, PrototypeGame game = PrototypeGame.Any)
        {
            var candidates = CreateNodeCandidates(typeId, game);
            return candidates.Count == 0 ? null : candidates[0];
        }

        public static List<BaseNode> CreateNodeCandidates(uint typeId, PrototypeGame game = PrototypeGame.Any)
        {
            if (!IsLookupBuild)
            {
                lock (_lookupBuildLock)
                {
                    if (!IsLookupBuild) BuildLookup();
                }
            }

            Type first = null;
            Type second = null;
            if (game == PrototypeGame.P1)
            {
                _p1Lookup.TryGetValue(typeId, out first);
            }
            else if (game == PrototypeGame.P2)
            {
                _p2Lookup.TryGetValue(typeId, out first);
            }
            else
            {
                _p1Lookup.TryGetValue(typeId, out first);
                _p2Lookup.TryGetValue(typeId, out second);
                if (second == first) second = null;
            }

            // The previous implementation allocated an intermediate List<Type> for every chunk,
            // then a second List<BaseNode>. Chunk trees contain thousands of nodes, so keep only
            // the result list and fill it directly from the immutable type lookup.
            int count = (first == null ? 0 : 1) + (second == null ? 0 : 1);
            var result = new List<BaseNode>(count);
            if (first != null) result.Add(CreateNodeInstance(first));
            if (second != null) result.Add(CreateNodeInstance(second));
            return result;
        }

        private static Func<BaseNode> CreateNodeFactory(Type type)
        {
            try
            {
                MethodInfo closed = _genericNodeFactoryMethod.MakeGenericMethod(type);
                return (Func<BaseNode>)closed.CreateDelegate(typeof(Func<BaseNode>));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is MemberAccessException ||
                ex is NotSupportedException || ex is InvalidOperationException)
            {
                // Preserve support for any legacy node whose constructor does not meet new()'s
                // public-parameterless requirement.
                return () => CreateNodeWithActivator(type);
            }
        }

        private static BaseNode CreateNodeGeneric<T>() where T : BaseNode, new()
        {
            return new T();
        }

        private static BaseNode CreateNodeInstance(Type type)
        {
            Func<BaseNode> factory;
            if (_nodeFactories != null && _nodeFactories.TryGetValue(type, out factory))
                return factory();
            return CreateNodeWithActivator(type);
        }

        private static BaseNode CreateNodeWithActivator(Type type)
        {
            try
            {
                return (BaseNode)Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                throw (ex.InnerException ?? ex);
            }
        }
    }
}
