using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Security;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the
{
    [TestClass]
    public class CorsPostRequestHandler_class
    {
        private const string ExpectedHeader = "Allowed";
        private const string Origin = "temp.uri";

        private IPostRequestHandler _postRequestHandler;

        [TestMethod]
        public async Task it_should_do_nothing_if_no_origin_provided_in_request()
        {
            var response = this.CreateResponse();

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowOrigin.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public async Task it_should_allow_any_domain()
        {
            var response = this.CreateResponseWithOrigin(Origin);

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowOrigin.Should().Be(CorsPostRequestHandler.Any);
        }

        [TestMethod]
        public async Task it_should_allow_any_header_from_request()
        {
            var response = this.CreateResponseWithOrigin(Origin);

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlAllowHeaders.Should().Contain("Content-Type");
        }
        
        [TestMethod]
        public async Task it_should_expose_any_header_from_request()
        {
            var response = this.CreateResponseWithOrigin(Origin, new Header("Accept-Ranges", "bytes"));

            await _postRequestHandler.Process(response);

            response.Headers.AccessControlExposeHeaders.Should().Contain("Content-Range");
        }

        [TestMethod]
        public void it_should_throw_when_no_response_info_is_provided()
        {
            _postRequestHandler.Awaiting(instance => instance.Process(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("responseInfo");
        }

        [TestMethod]
        public void it_should_throw_when_the_response_info_provided_is_of_incorrect_type()
        {
            _postRequestHandler.Awaiting(instance => instance.Process(new Mock<IResponseInfo>(MockBehavior.Strict).Object))
                .ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("responseInfo");
        }

        [TestInitialize]
        public void Setup()
        {
            _postRequestHandler = new CorsPostRequestHandler();
        }

        [TestCleanup]
        public void Teardown()
        {
            _postRequestHandler = null;
        }
    }
}