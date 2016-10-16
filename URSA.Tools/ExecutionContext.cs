using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
#if CORE
using System.IO;
using Microsoft.Extensions.DependencyModel;
#endif

namespace System
{
    /// <summary>Provides useful code execution context methods.</summary>
    public static class ExecutionContext
    {
        /// <summary>Gets a primary path storing assemblies for given application.</summary> 
        /// <returns>Primary place where assemblies for given application domain are stored.</returns> 
        public static string GetPrimaryAssemblyDirectory()
        {
#if CORE
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#else
            return (String.IsNullOrWhiteSpace(AppDomain.CurrentDomain.RelativeSearchPath) ? AppDomain.CurrentDomain.BaseDirectory : AppDomain.CurrentDomain.RelativeSearchPath);
#endif
        }

        /// <summary>Gets loaded assemblies.</summary>
        /// <param name="assemblyNameRegex">Optional assembly name filter.</param>
        /// <returns>Enumeration of matching assemblies.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Method uses local file system, which may proove to be diffucult for testing.")]
        public static IEnumerable<Assembly> GetLoadedAssemblies(Regex assemblyNameRegex = null)
        {
#if CORE
            return from library in DependencyContext.Default.RuntimeLibraries
                   from assembly in library.Assemblies
                   where (assemblyNameRegex == null) || (assemblyNameRegex.IsMatch(assembly.Name.FullName))
                   select Assembly.Load(assembly.Name);
#else
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   where (assemblyNameRegex == null) || (assemblyNameRegex.IsMatch(assembly.FullName))
                   select assembly;
#endif
        }
    }
}