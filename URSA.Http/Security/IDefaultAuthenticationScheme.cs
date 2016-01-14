namespace URSA.Web.Http.Security
{
    /// <summary>Provides a facility used to select a default authentication scheme.</summary>
    public interface IDefaultAuthenticationScheme
    {
        /// <summary>Gets the authentication scheme to be used by default.</summary>
        string Scheme { get; }

        /// <summary>Challenges the specified response.</summary>
        /// <param name="response">The response.</param>
        void Challenge(IResponseInfo response);
    }
}