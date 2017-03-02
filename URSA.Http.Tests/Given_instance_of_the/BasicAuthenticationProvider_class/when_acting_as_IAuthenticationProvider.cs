using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Security;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.BasicAuthenticationProvider_class
{
    [TestFixture]
    public class when_acting_as_IAuthenticationProvider
    {
        private Mock<IIdentityProvider> _identityProvider;
        private BasicAuthenticationProvider _authenticationProvider;

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public async Task it_should_not_authenticate_user_when_the_Base64_token_is_broken()
        {
            var request = this.CreateRequest(new Header("Authorization", "Basic test"));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Test]
        public async Task it_should_not_authenticate_user_when_credentials_contains_more_than_user_name_and_password()
        {
            var request = this.CreateRequest(new Header("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("test:password:whatever"))));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Test]
        public async Task it_should_not_authenticate_user_when_user_name__is_empty()
        {
            var request = this.CreateRequest(new Header("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":password"))));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Test]
        public async Task it_should_not_authenticate_user_when_password__is_empty()
        {
            var request = this.CreateRequest(new Header("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("test:"))));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Test]
        public async Task it_should_not_authenticate_user_when_no_authorization_header_is_present()
        {
            var request = this.CreateRequest();

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [Test]
        public async Task it_should_not_authenticate_user_when_the_authorization_header_is_of_a_different_scheme()
        {
            var request = this.CreateRequest(new Header("Authorization", "Bearer test"));

            await _authenticationProvider.Process(request);

            request.Identity.IsAuthenticated.Should().BeFalse();
        }

        [SetUp]
        public void Setup()
        {
            _identityProvider = new Mock<IIdentityProvider>(MockBehavior.Strict);
            _authenticationProvider = new BasicAuthenticationProvider(_identityProvider.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _identityProvider = null;
            _authenticationProvider = null;
        }
    }
}
