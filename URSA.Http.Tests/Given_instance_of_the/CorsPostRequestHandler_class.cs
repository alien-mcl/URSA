using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Security;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the
{
    [TestFixture]
    public class CorsPostRequestHandler_class
    {
        private const string ExpectedHeader = "Allowed";
        private const string Origin = "temp.uri";

        private IPostRequestHandler _postRequestHandler;

        [Test]
        public async Task it_should_do_nothing_if_no_origin_provided_in_request()
        {
            var response = this.CreateResponse();

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowOrigin.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task it_should_allow_any_domain()
        {
            var response = this.CreateResponseWithOrigin(Origin);

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowOrigin.Should().Be(CorsPostRequestHandler.Any);
        }

        [Test]
        public async Task it_should_allow_any_header_from_request()
        {
            var response = this.CreateResponseWithOrigin(Origin);

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowHeaders.Should().Contain("Content-Type");
        }
        
        [Test]
        public async Task it_should_expose_any_header_from_request()
        {
            var response = this.CreateResponseWithOrigin(Origin, new Header("Accept-Ranges", "bytes"));

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlExposeHeaders.Should().Contain("Content-Range");
        }

        [Test]
        public void it_should_throw_when_no_response_info_is_provided()
        {
            _postRequestHandler.Awaiting(instance => instance.Process(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("responseInfo");
        }

        [Test]
        public void it_should_throw_when_the_response_info_provided_is_of_incorrect_type()
        {
            _postRequestHandler.Awaiting(instance => instance.Process(new Mock<IResponseInfo>(MockBehavior.Strict).Object))
                .ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("responseInfo");
        }

        [SetUp]
        public void Setup()
        {
            _postRequestHandler = new CorsPostRequestHandler();
        }

        [TearDown]
        public void Teardown()
        {
            _postRequestHandler = null;
        }
    }
}