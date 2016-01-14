namespace URSA.Web.Http.Security
{
    /// <summary>Provides a basic description of an authentication facility.</summary>
    public interface IAuthenticationProvider
    {
        /// <summary>Gets the authentication scheme.</summary>
        string Scheme { get; }

        /// <summary>Authenticates the specified request.</summary>
        /// <param name="request">The request.</param>
        void Authenticate(IRequestInfo request);
    }
}