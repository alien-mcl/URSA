using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Security;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.BasicAuthenticationProvider_class
{
    [TestClass]
    public class when_acting_as_IAuthenticationProvider
    {
        private Mock<IIdentityProvider> _identityProvider;
        private BasicAuthenticationProvider _authenticationProvider;

        [TestMethod]
        public async Task it_should_authenticate_user()
        {
            var expectedUser = "user";
            var expectedPassword = "password";
            var authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedUser + ":" + expectedPassword));
            var request = this.CreateRequest(new Header("Authorization", authorization));
            _identityProvider.Setup(instance => instance.ValidateCredentials(expectedUser, expectedPassword)).Returns(new BasicClaimBasedIdentity(expectedUser));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeTrue();
            request.Identity[ClaimTypes.Name].Should().Contain(expectedUser);
        }

        [TestMethod]
        public async Task it_should_call_the_authentication_service()
        {
            var expectedUser = "user";
            var expectedPassword = "password";
            var authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedUser + ":" + expectedPassword));
            var request = this.CreateRequest(new Header("Authorization", authorization));
            _identityProvider.Setup(instance => instance.ValidateCredentials(expectedUser, expectedPassword)).Returns(new BasicClaimBasedIdentity(expectedUser));

            await _authenticationProvider.Process(request);

            _identityProvider.Verify(instance => instance.ValidateCredentials(expectedUser, expectedPassword), Times.Once);
        }

        [TestMethod]
        public async Task it_should_not_authenticate_user_when_wrong_credentials_are_passed()
        {
            var expectedUser = "user";
            var expectedPassword = "wrong";
            var authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedUser + ":" + expectedPassword));
            var request = this.CreateRequest(new Header("Authorization", authorization));
            _identityProvider.Setup(instance => instance.ValidateCredentials(expectedUser, expectedPassword)).Returns((IClaimBasedIdentity)null);

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [TestInitialize]
        public void Setup()
        {
            _identityProvider = new Mock<IIdentityProvider>(MockBehavior.Strict);
            _authenticationProvider = new BasicAuthenticationProvider(_identityProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _identityProvider = null;
            _authenticationProvider = null;
        }
    }
}
