using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides an abstract description of a model transformation facility.</summary>
    public interface IResponseModelTransformer
    {
        /// <summary>Transforms an output of a processed request.</summary>
        /// <remarks>This layer is called after the request is processed (all the <see cref="IPreRequestHandler" /> are already invoked) and just before processing <see cref="IPostRequestHandler" />s.</remarks>
        /// <param name="requestMapping">The request mapping.</param>
        /// <param name="request">The request.</param>
        /// <param name="result">Result of the underlying method invoked by the request.</param>
        /// <param name="arguments">The arguments passed to the underlying method invoked by the request.</param>
        /// <returns>Transformed result.</returns>
        Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments);
    }

    /// <summary>Marks a model transformer as a dependent on another <typeparamref name="T" /> model transformer.</summary>
    /// <typeparam name="T">Type of the model transformer this transformer depends on.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public interface IResponseModelTransformer<T> : IResponseModelTransformer
    {
    }
}