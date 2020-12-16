using System;
using System.IO;
using System.Reflection;

namespace Delivery.Azure.Library.Core.Extensions.Assemblies
{
    /// <summary>
    ///     Provides methods to conveniently locate application versions
    /// </summary>
    public static class AssemblyVersionExtensions
    {
        /// <summary>
        ///     Parses the <see cref="Assembly" /> to get the FileVersion property
        /// </summary>
        public static string GetAssemblyVersion(this Assembly assembly)
        {
            if (assembly.Location == null)
            {
                throw new NotImplementedException($"Assembly location {assembly.Location} not found");
            }

            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (assemblyInformationalVersionAttribute == null)
            {
                throw new NotSupportedException($"Assembly {assembly.FullName} has no attribute of type {nameof(AssemblyInformationalVersionAttribute)}. This is required to determine the assembly version");
            }

            var version = assemblyInformationalVersionAttribute.Version;
            return version;
        }

        /// <summary>
        ///     Uses the assembly found in the current project to get the assembly version
        /// </summary>
        public static string GetAssemblyVersion<T>()
        {
            var assembly = typeof(T).Assembly;
            return GetAssemblyVersion(assembly);
        }

        /// <summary>
        ///     Gets the path of the assembly which exists in the current <see cref="AppDomain" />
        /// </summary>
        public static string GetAssemblyPath()
        {
            var assemblyLocation = AppDomain.CurrentDomain.BaseDirectory;

            if (!Directory.Exists(assemblyLocation))
            {
                assemblyLocation = Directory.GetCurrentDirectory();
            }

            if (assemblyLocation == null || !Directory.Exists(assemblyLocation))
            {
                throw new DirectoryNotFoundException("Assembly location could not be determined");
            }

            return assemblyLocation;
        }
    }
}