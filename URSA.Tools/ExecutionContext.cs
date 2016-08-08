using System.Diagnostics.CodeAnalysis;
#if CORE
using System.IO;
using System.Reflection;
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
    }
}