using System.Reflection;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP handler mapper.</summary>
    public interface IHttpDelegateMapper : IDelegateMapper
    {
        /// <summary>Gets the HTTP verb for a given controller method.</summary>
        /// <remarks>If the method fails to pin-point the verb, it will return the <see cref="Verb.GET" /> by default.</remarks>
        /// <param name="methodInfo">Method to retrieve verb for.</param>
        /// <returns>Instance of the <see cref="Verb" /> class.</returns>
        Verb GetMethodVerb(MethodInfo methodInfo);
    }
}