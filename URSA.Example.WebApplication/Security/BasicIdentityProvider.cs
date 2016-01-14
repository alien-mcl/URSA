using System;
using URSA.Security;

namespace URSA.Example.WebApplication.Security
{
    /// <summary>Provides a fixed identity.</summary>
    public class BasicIdentityProvider : IIdentityProvider
    {
        /// <inheritdoc />
        public IClaimBasedIdentity ValidateCredentials(string userName, string password)
        {
            if (userName == null)
            {
                throw new ArgumentNullException("userName");
            }

            if (userName.Length == 0)
            {
                throw new ArgumentOutOfRangeException("userName");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            if (password.Length == 0)
            {
                throw new ArgumentOutOfRangeException("password");
            }

            if ((String.Compare(userName, "guest", true) != 0) || (String.Compare(password, "guest", true) != 0))
            {
                return null;
            }

            return new BasicClaimBasedIdentity("guest");
        }
    }
}