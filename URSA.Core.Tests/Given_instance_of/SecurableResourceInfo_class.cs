using System;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Security;
using URSA.Tests.Web;
using URSA.Web.Description;

namespace URSA.Tests.Given_instance_of
{
    [TestClass]
    public class SecurableResourceInfo_class
    {
        [TestMethod]
        public void it_should_provide_unified_security_specification_correctly()
        {
            var controllerType = typeof(TestController);
            var operationUri = "/api/test/operation";
            var methodInfo = controllerType.GetMethod("Operation");
            var entryPoint = new EntryPointInfo(new Uri("/api", UriKind.Relative)).WithSecurityDetailsFrom(controllerType.Assembly);
            var operation = new FakeOperationInfo(methodInfo, new Uri(operationUri, UriKind.Relative), null, new Regex(operationUri)).WithSecurityDetailsFrom(methodInfo);
            var controller = new FakeControllerInfo(entryPoint, new Uri("/api/test", UriKind.Relative), operation).WithSecurityDetailsFrom(controllerType);

            var result = operation.UnifiedSecurityRequirements;

            result.Allowed[ClaimTypes.Name].Should().Contain("test");
            result.Allowed[ClaimTypes.Name].Should().Contain("other");
            result.Denied.Should().BeEmpty();
        }

        private class FakeControllerInfo : ControllerInfo
        {
            internal FakeControllerInfo(EntryPointInfo entryPoint, Uri uri, params OperationInfo[] operations) : base(entryPoint, uri, operations)
            {
            }

            public override Type ControllerType { get { return typeof(void); } }
        }

        private class FakeOperationInfo : OperationInfo
        {
            internal FakeOperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, Regex regexTemplate, params ValueInfo[] values) :
                base(underlyingMethod, uri, uriTemplate, regexTemplate, values)
            {
            }
        }
    }
}