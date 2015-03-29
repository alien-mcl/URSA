using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Http.Description.Hydra;

namespace URSA.CodeGen
{
    /// <summary>Provides a contract for class code generator.</summary>
    public interface IClassGenerator
    {
        /// <summary>Creates a code the given <paramref name="supportedClass" />.</summary>
        /// <param name="supportedClass">The supported class.</param>
        /// <returns>Map of file names with corresponding code.</returns>
        IDictionary<string, string> CreateCode(IClass supportedClass);

        /// <summary>Creates the namespace of the given <paramref name="resource" />.</summary>
        /// <param name="resource">The resource.</param>
        /// <returns>String with namespace.</returns>
        string CreateNamespace(IResource resource);

        /// <summary>Creates the name of the given <paramref name="resource" />.</summary>
        /// <param name="resource">The resource.</param>
        /// <returns>String with name.</returns>
        string CreateName(IResource resource);
    }
}