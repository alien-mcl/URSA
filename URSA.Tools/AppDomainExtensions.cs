using System.Diagnostics.CodeAnalysis;

namespace System
{
    /// <summary>Provides useful <see cref="AppDomain" /> extension methods.</summary>
    public static class AppDomainExtensions
    {
        /// <summary>Gets a primary path storing assemblies for given application domain.</summary> 
        /// <param name="domain">Application domain for which the path is being determined.</param> 
        /// <returns>Primary place where assemblies for given application domain are stored.</returns> 
        [ExcludeFromCodeCoverage]
        public static string GetPrimaryAssemblyDirectory(this AppDomain domain)
        {
            return (String.IsNullOrWhiteSpace(domain.RelativeSearchPath) ? domain.BaseDirectory : domain.RelativeSearchPath);
        }
    }
}