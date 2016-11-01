using System;
using System.Linq;
using Autofac.Builder;

namespace URSA.AutoFac
{
    /// <summary>Provides custom <see cref="IRegistrationBuilder{TLimit, TActivatorData, TStyle}" /> extensions.</summary>
    public static class RegistrationExtensions
    {
        /// <summary>Defines a tag of an URSA HTTP request lifetime scope.</summary>
        public const string HttpRequestLifetimeScopeTag = "UrsaHttpRequest";

        /// <summary>Share one instance of the component within the context of a single HTTP request.</summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerHttpRequest<TLimit, TActivatorData, TStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
            params object[] lifetimeScopeTags)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            var tags = new[] { HttpRequestLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }
    }
}
