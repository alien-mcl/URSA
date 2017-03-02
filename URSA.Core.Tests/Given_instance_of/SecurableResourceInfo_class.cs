using System;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using URSA;
using URSA.Security;
using URSA.Testing;
using URSA.Tests.Web;
using URSA.Web.Description;
using URSA.Web.Http;

namespace Given_instance_of
{
    [TestFixture]
    public class SecurableResourceInfo_class
    {
        private FakeOperationInfo _operationInfo;

        [Test]
        public void it_should_provide_unified_security_specification_correctly()
        {
            var result = _operationInfo.UnifiedSecurityRequirements;

            result.Allowed[ClaimTypes.Name].Should().Contain("test");
            result.Allowed[ClaimTypes.Name].Should().Contain("other");
            result.Denied.Should().BeEmpty();
        }

        [Test]
        public void it_should_throw_when_no_allowings_check_subject_is_passed()
        {
            ((FakeOperationInfo)null).Invoking(instance => instance.Allows(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("securableResource");
        }

        [Test]
        public void it_should_throw_when_no_identity_is_passed_for_allowings_check()
        {
            _operationInfo.Invoking(instance => instance.Allows(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("identity");
        }

        [SetUp]
        public void Setup()
        {
            UrlParser.Register<RelativeUrlParser>();
            var controllerType = typeof(TestController).GetTypeInfo();
            var operationUrl = "/api/test/operation";
            var methodInfo = controllerType.GetMethod("Operation");
            var entryPoint = new EntryPointInfo(UrlParser.Parse("/api")).WithSecurityDetailsFrom(controllerType.Assembly);
            _operationInfo = new FakeOperationInfo(methodInfo, UrlParser.Parse(operationUrl), null, new Regex(operationUrl)).WithSecurityDetailsFrom(methodInfo);
            new FakeControllerInfo(entryPoint, UrlParser.Parse("/api/test"), _operationInfo).WithSecurityDetailsFrom(controllerType);
        }

        [TearDown]
        public void Teardown()
        {
            _operationInfo = null;
        }

        private class FakeControllerInfo : ControllerInfo
        {
            internal FakeControllerInfo(EntryPointInfo entryPoint, Url url, params OperationInfo[] operations)
                : base(entryPoint, url, operations)
            {
            }

            public override Type ControllerType { get { return typeof(void); } }
        }

        private class FakeOperationInfo : OperationInfo
        {
            internal FakeOperationInfo(MethodInfo underlyingMethod, Url url, string urlTemplate, Regex regexTemplate, params ValueInfo[] values) :
                base(underlyingMethod, url, urlTemplate, regexTemplate, values)
            {
            }
        }
    }
}