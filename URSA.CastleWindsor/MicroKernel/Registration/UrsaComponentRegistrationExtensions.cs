using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration.Lifestyle;

namespace Castle.MicroKernel.Registration
{
    /// <summary>Provides useful <see cref="ComponentRegistration{TService}" /> extension methods.</summary>
    public static class UrsaComponentRegistrationExtensions
    {
        /// <summary>Registers a component to use an URSA universal web request scope.</summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="lifestyle">The component registration lifestyle group.</param>
        /// <returns>Configured component registration.</returns>
        public static ComponentRegistration<TService> PerUniversalWebRequest<TService>(this LifestyleGroup<TService> lifestyle) where TService : class
        {
            return lifestyle.Scoped<UniversalWebRequestScopeAccessor>();
        }
    }
}
