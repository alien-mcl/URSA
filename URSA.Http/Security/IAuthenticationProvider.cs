namespace URSA.Web.Http.Security
{
    /// <summary>Provides a basic description of an authentication facility.</summary>
    public interface IAuthenticationProvider : IPreRequestHandler
    {
        /// <summary>Gets the authentication scheme.</summary>
        string Scheme { get; }
    }
}