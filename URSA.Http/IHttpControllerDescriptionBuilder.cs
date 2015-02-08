using System.Reflection;
using URSA.Web.Http;

namespace URSA.Web.Description.Http
{
    /// <summary>Provides an HTTP controller description building facility.</summary>
    public interface IHttpControllerDescriptionBuilder : IControllerDescriptionBuilder
    {
        /// <summary>Gets the HTTP verb for given controller method.</summary>
        /// <param name="methodInfo">Method for which to obtain the verb.</param>
        /// <returns>HTTP verb for given method.</returns>
        Verb GetMethodVerb(MethodInfo methodInfo);
    }

    /// <summary>Provides an HTTP controller description building facility.</summary>
    /// <typeparam name="T">Type of the controller described.</typeparam>
    public interface IHttpControllerDescriptionBuilder<T> : IHttpControllerDescriptionBuilder, IControllerDescriptionBuilder<T> where T : IController
    {
    }
}