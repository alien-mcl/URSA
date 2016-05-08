using System.Collections.Generic;
using System.Reflection;

namespace URSA.Web.Description
{
    /// <summary>Builds description for given controller.</summary>
    public interface IControllerDescriptionBuilder
    {
        /// <summary>Builds the controller descriptor.</summary>
        /// <returns>Controller description info.</returns>
        ControllerInfo BuildDescriptor();

        /// <summary>Gets the operation URL template.</summary>
        /// <param name="methodInfo">Method for which to obtain the template.</param>
        /// <param name="argumentMapping">Resulting argument mappings.</param>
        /// <returns>URL template for methods with parameters; otherwise <b>null</b>.</returns>
        string GetOperationUrlTemplate(MethodInfo methodInfo, out IEnumerable<ArgumentInfo> argumentMapping);
    }

    /// <summary>Builds description for given controller.</summary>
    /// <typeparam name="T">Type of the controller described.</typeparam>
    public interface IControllerDescriptionBuilder<T> : IControllerDescriptionBuilder where T : IController
    {
        /// <summary>Builds the controller descriptor.</summary>
        /// <returns>Controller description info.</returns>
        new ControllerInfo<T> BuildDescriptor();
    }
}