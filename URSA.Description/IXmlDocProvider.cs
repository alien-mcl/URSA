using System;
using System.Reflection;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract for XML documentation provider.</summary>
    public interface IXmlDocProvider
    {
        /// <summary>Gets the type description.</summary>
        /// <param name="type">Type to get description for.</param>
        /// <returns>Description of the given <paramref name="type" />.</returns>
        string GetDescription(Type type);

        /// <summary>Gets the type description.</summary>
        /// <param name="method">odMeth to get description for.</param>
        /// <returns>Description of the given <paramref name="method" />.</returns>
        string GetDescription(MethodInfo method);

        /// <summary>Gets the type description.</summary>
        /// <param name="property">Property to get description for.</param>
        /// <returns>Description of the given <paramref name="property" />.</returns>
        string GetDescription(PropertyInfo property);
    }
}