using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides an abstract description of a response model transformation facility.</summary>
    public interface IRequestModelTransformer
    {
        /// <summary>Transforms an input for a request request.</summary>
        /// <remarks>This layer is called before the request is processed.</remarks>
        /// <param name="arguments">The arguments that will be used to build up a request.</param>
        /// <returns>Transformed arguments.</returns>
        Task<object[]> Transform(object[] arguments);
    }

    /// <summary>Marks a response model transformer as a dependent on another <typeparamref name="T" /> response model transformer.</summary>
    /// <typeparam name="T">Type of the model transformer this transformer depends on.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public interface IRequestModelTransformer<T> : IRequestModelTransformer
    {
    }
}