using System;
using System.IO;
using System.Reflection;

namespace PrototypeUltimateTool
{
    /// <summary>
    /// Resolves managed dependencies from resources embedded in the main executable.
    /// This keeps the published application folder free of third-party DLL files.
    /// </summary>
    internal static class EmbeddedAssemblyResolver
    {
        private const string ResourcePrefix = "PrototypeUltimateTool.EmbeddedDependencies.";

        internal static void Register()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
        }

        private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName requestedName;
            try
            {
                requestedName = new AssemblyName(args.Name);
            }
            catch (ArgumentException)
            {
                return null;
            }

            string resourceName = ResourcePrefix + requestedName.Name + ".dll";
            Assembly owner = typeof(EmbeddedAssemblyResolver).Assembly;

            using (Stream resource = owner.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    return null;
                }

                using (var buffer = new MemoryStream())
                {
                    resource.CopyTo(buffer);
                    return Assembly.Load(buffer.ToArray());
                }
            }
        }
    }
}
