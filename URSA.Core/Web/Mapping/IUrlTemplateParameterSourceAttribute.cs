using System.Reflection;

namespace URSA.Web.Mapping
{
    /// <summary>Defines a contract for URL template parameter source attributes.</summary>
    public interface IUrlTemplateParameterSourceAttribute
    {
        /// <summary>Gets the URL template.</summary>
        string Template { get; }

        /// <summary>Gets the default template.</summary>
        string DefaultTemplate { get; }

        /// <summary>Creates a new instance of the <see cref="IUrlTemplateParameterSourceAttribute" /> for given parameter.</summary>
        /// <param name="parameter">The parameter for which to create a new template parameter source attribute.</param>
        /// <returns>Instance of the <see cref="IUrlTemplateParameterSourceAttribute" />.</returns>
        ParameterSourceAttribute For(ParameterInfo parameter);
    }
}