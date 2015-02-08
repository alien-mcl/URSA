using System.Reflection;
using URSA.Web.Http;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary>Provides a basic default parameter source selection policy description.</summary>
    public interface IDefaultParameterSourceSelector
    {
        /// <summary>Gets the default parameter source mapping for given type.</summary>
        /// <param name="parameter">Argument for which to obtain a default parameter source mapping.</param>
        /// <param name="verb">HTTP verb of the method given <paramref name="parameter" /> belongs to.</param>
        /// <returns>Parameter source attribute for given type.</returns>
        ParameterSourceAttribute ProvideDefault(ParameterInfo parameter, Verb verb);
    }
}