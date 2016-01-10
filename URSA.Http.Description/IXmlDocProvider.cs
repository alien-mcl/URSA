using System;
using System.Collections.Generic;
using System.Reflection;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract for XML documentation provider.</summary>
    public interface IXmlDocProvider
    {
        /// <summary>Gets the type description of the <paramref name="type" />.</summary>
        /// <param name="type">Type to get description for.</param>
        /// <returns>Description of the given <paramref name="type" />.</returns>
        string GetDescription(Type type);

        /// <summary>Gets the type description of the <paramref name="method" />.</summary>
        /// <param name="method">Method to get description for.</param>
        /// <returns>Description of the given <paramref name="method" />.</returns>
        string GetDescription(MethodInfo method);

        /// <summary>Gets the type description of the <paramref name="method" /> <paramref name="parameter" />.</summary>
        /// <param name="method">Parameter owning method.</param>
        /// <param name="parameter">Parameter for which to obtain the description.</param>
        /// <returns>Description of the given <paramref name="parameter" />.</returns>
        string GetDescription(MethodInfo method, ParameterInfo parameter);

        /// <summary>Gets the type description of the <paramref name="property" />.</summary>
        /// <param name="property">Property to get description for.</param>
        /// <returns>Description of the given <paramref name="property" />.</returns>
        string GetDescription(PropertyInfo property);

        /// <summary>Gets the type names of the exceptions thrown by the <paramref name="method" />.</summary>
        /// <param name="method">Method to get description for.</param>
        /// <returns>Enumeration of exception type names thrown by the given <paramref name="method" />.</returns>
        IEnumerable<string> GetExceptions(MethodInfo method);
    }
}