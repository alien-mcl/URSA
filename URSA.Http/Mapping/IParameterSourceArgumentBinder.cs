using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Provides a basic description of all details required to bind a value to an method argument.</summary>
    public interface IParameterSourceArgumentBinder
    {
        /// <summary>Retrieves the value from th egiven request.</summary>
        /// <param name="context">Binding context.</param>
        /// <returns>Bound value.</returns>
        object GetArgumentValue(ArgumentBindingContext context);
    }

    /// <summary>Provides a basic description of all details required to bind a value to an method argument.</summary>
    /// <typeparam name="T">Type of parameters source.</typeparam>
    public interface IParameterSourceArgumentBinder<T> : IParameterSourceArgumentBinder where T : ParameterSourceAttribute
    {
        /// <summary>Retrieves the value from th egiven request.</summary>
        /// <param name="context">Binding context.</param>
        /// <returns>Bound value.</returns>
        object GetArgumentValue(ArgumentBindingContext<T> context);
    }
}