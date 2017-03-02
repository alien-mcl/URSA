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
    public class when_acting_as_IDefaultAuthenticationScheme
    {
        private Mock<IIdentityProvider> _identityProvider;
        private BasicAuthenticationProvider _authenticationProvider;

        [Test]
        public async Task it_should_challenge_the_response()
        {
            var response = this.CreateResponse();

            await _authenticationProvider.Process(response);

            response.Headers.WWWAuthenticate.Should().Be(BasicAuthenticationProvider.AuthenticationScheme + " realm=\"" + RequestExtensions.DefaultHost + "\"");
        }

        [Test]
        public async Task it_should_add_an_exposed_challenge_header()
        {
            var response = this.CreateResponseWithOrigin();

            await _authenticationProvider.Process(response);

            response.Headers.AccessControlExposeHeaders.Should().Contain(Header.WWWAuthenticate);
        }

        [Test]
        public async Task it_should_not_add_an_exposed_challenge_header()
        {
            var response = this.CreateResponse();

            await _authenticationProvider.Process(response);

            response.Headers.AccessControlExposeHeaders.Should().NotContain(Header.WWWAuthenticate);
        }

        [Test]
        public async Task it_should_add_custom_headers_for_AJAX_originated_request()
        {
            var response = this.CreateResponseWithOrigin("localhost", new Header(Header.XRequestedWith, BasicAuthenticationProvider.XMLHttpRequest));

            await _authenticationProvider.Process(response);

            response.Headers[BasicAuthenticationProvider.XWWWAuthenticate].Value.Should().Be(BasicAuthenticationProvider.AuthenticationScheme + " realm=\"" + RequestExtensions.DefaultHost + "\"");
            response.Headers.AccessControlExposeHeaders.Should().Contain(BasicAuthenticationProvider.XWWWAuthenticate);
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
