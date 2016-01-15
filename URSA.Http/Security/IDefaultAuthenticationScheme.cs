namespace URSA.Web.Http.Security
{
    /// <summary>Provides a facility used to select a default authentication scheme.</summary>
    public interface IDefaultAuthenticationScheme : IPostRequestHandler
    {
        /// <summary>Gets the authentication scheme to be used by default.</summary>
        string Scheme { get; }
    }
}