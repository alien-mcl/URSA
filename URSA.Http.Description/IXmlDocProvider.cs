using System;
using System.Reflection;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract for XML documentation provider.</summary>
    public interface IXmlDocProvider
    {
        /// <summary>Gets the type description of the type.</summary>
        /// <param name="type">Type to get description for.</param>
        /// <returns>Description of the given <paramref name="type" />.</returns>
        string GetDescription(Type type);

        /// <summary>Gets the type description of the method.</summary>
        /// <param name="method">Method to get description for.</param>
        /// <returns>Description of the given <paramref name="method" />.</returns>
        string GetDescription(MethodInfo method);

        /// <summary>Gets the type description of the method parameter.</summary>
        /// <param name="method">Parameter owning method.</param>
        /// <param name="parameter">Parameter for which to obtain the description.</param>
        /// <returns>Description of the given <paramref name="parameter" />.</returns>
        string GetDescription(MethodInfo method, ParameterInfo parameter);

        /// <summary>Gets the type description of the property.</summary>
        /// <param name="property">Property to get description for.</param>
        /// <returns>Description of the given <paramref name="property" />.</returns>
        string GetDescription(PropertyInfo property);
    }
}