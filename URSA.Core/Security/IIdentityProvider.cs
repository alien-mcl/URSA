namespace URSA.Security
{
    /// <summary>Provides a basic identity provider description.</summary>
    public interface IIdentityProvider
    {
        /// <summary>Validates a given credentials.</summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>Identity adequate for the given credentials if valid; otherwise <b>null</b>.</returns>
        IClaimBasedIdentity ValidateCredentials(string userName, string password);
    }
}