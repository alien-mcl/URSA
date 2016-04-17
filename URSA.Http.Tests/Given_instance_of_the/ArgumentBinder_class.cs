using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Security;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ArgumentBinder_class
    {
        private static readonly Type ControllerType = typeof(TestController);
        private Mock<IParameterSourceArgumentBinder<FromQueryStringAttribute>> _fromQueryStringBinder;
        private ArgumentBinder _binder;

        [TestMethod]
        public void it_should_bind_arguments_for_Add_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1 : 2));
            var method = ControllerType.GetMethod("Add");
            var arguments = BindArguments("/api/test/add?operandA=1&operandB=2", method, Verb.GET);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1);
            arguments[1].Should().Be(2);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Substract_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1 : 2));
            var method = ControllerType.GetMethod("Substract");
            var arguments = BindArguments("/api/test/sub/operandA/1?operandB=2", method, Verb.GET);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1);
            arguments[1].Should().Be(2);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Multiply_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1 : 2));
            var method = ControllerType.GetMethod("Multiply");
            var arguments = BindArguments("/api/test/multiply/operandB/2?operandA=1", method, Verb.GET);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1);
            arguments[1].Should().Be(2);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Divide_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1 : 2));
            var method = ControllerType.GetMethod("Divide");
            var arguments = BindArguments("/api/test/div/operandA/1", method, Verb.POST);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1);
            arguments[1].Should().Be(2);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Modulo_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1 : 2));
            var method = ControllerType.GetMethod("PostModulo");
            var body =
                "--test\r\nContent-Type: text/plain\r\nContent-Length:3\r\n\r\n1\r\n" +
                "--test\r\nContent-Type: text/plain\r\nContent-Length:3\r\n\r\n2\r\n--test--";
            var headers = new HeaderCollection();
            headers.ContentLength = body.Length;
            ((IDictionary<string, string>)headers)["Content-Type"] = "multipart/mixed; boundary=\"test\"";
            var arguments = BindArguments("/api/test/modulo", method, Verb.POST);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1);
            arguments[1].Should().Be(2);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Power_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? 1.0 : 2.0));
            var method = ControllerType.GetMethod("Power");
            var arguments = BindArguments("/api/test/power/operandA/1/operandB/2", method, Verb.GET);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(1.0);
            arguments[1].Should().Be(2.0);
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Log_method_correctly()
        {
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => new[] { 1.0, 2.0 });
            var method = ControllerType.GetMethod("Log");
            var arguments = BindArguments("/api/test/log?operands=1&operands=2", method, Verb.GET);

            arguments.Length.Should().Be(method.GetParameters().Length);
            ((double[])arguments[0]).Should().BeEquivalentTo(new[] { 1.0, 2.0 });
        }

        [TestMethod]
        public void it_should_bind_arguments_for_Upload_method_correctly()
        {
            string fileName = "test.txt";
            byte[] data = new byte[] { 1, 2 };
            _fromQueryStringBinder.Setup(instance => instance.GetArgumentValue(It.IsAny<ArgumentBindingContext>()))
                .Returns<ArgumentBindingContext>(context => (context.Index == 0 ? (object)fileName : (object)data));
            var method = ControllerType.GetMethod("Upload");
            string body = String.Format(
                "--test\r\nContentType: text/plain\r\nContent-Lenght: {0}\r\nContent-Disposition: form-data; name=\"filename\"\r\n\r\n{1}\r\n" +
                "--test\r\nContentType: text/plain\r\nContent-Length: {2}\r\nContent-Disposition: form-data; name=\"data\"\r\n\r\n{3}\r\n" +
                "--test--",
                fileName.Length + 2,
                fileName,
                System.Convert.ToBase64String(data).Length + 2,
                System.Convert.ToBase64String(data));
            var headers = new HeaderCollection();
            headers.ContentType = "multipart/form-data; boundary=\"test\"";
            headers.ContentLength = body.Length;
            var arguments = BindArguments("/api/test/upload", method, Verb.POST);

            arguments.Length.Should().Be(method.GetParameters().Length);
            arguments[0].Should().Be(fileName);
            ((byte[])arguments[1]).Should().BeEquivalentTo(data);
        }

        [TestMethod]
        public void it_should_throw_when_no_request_is_provided()
        {
            _binder.Invoking(instance => instance.BindArguments((IRequestInfo)null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [TestMethod]
        public void it_should_throw_when_the_request_provided_is_of_not_a_correct_type()
        {
            _binder.Invoking(instance => instance.BindArguments(new Mock<IRequestInfo>(MockBehavior.Strict).Object, null)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void it_should_throw_when_no_request_info_is_provided()
        {
            _binder.Invoking(instance => instance.BindArguments(null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [TestMethod]
        public void it_should_throw_when_no_request_mapping_is_provided()
        {
            var request = new RequestInfo(Verb.GET, new Uri("/", UriKind.Relative), new MemoryStream(), new BasicClaimBasedIdentity());
            _binder.Invoking(instance => instance.BindArguments(request, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("requestMapping");
        }

        [TestInitialize]
        public void Setup()
        {
            Mock<IConverter> converter = new Mock<IConverter>(MockBehavior.Strict);
            converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>())).Returns<Type, IRequestInfo>(Convert);
            converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<string>())).Returns<Type, string>(Convert);
            _fromQueryStringBinder = new Mock<IParameterSourceArgumentBinder<FromQueryStringAttribute>>();
            _binder = new ArgumentBinder(new IParameterSourceArgumentBinder[]
                {
                    _fromQueryStringBinder.Object,
                });
        }

        [TestCleanup]
        public void Teardown()
        {
            _binder = null;
            _fromQueryStringBinder = null;
        }

        private object[] BindArguments(string callUri, MethodInfo method, Verb verb)
        {
            var methodUri = Regex.Replace(callUri, "\\?.+", String.Empty);
            var queryStringParameters = Regex.Matches(callUri, "[?&]([^=]+)=[^&]+").Cast<System.Text.RegularExpressions.Match>();
            var queryStringRegex = (queryStringParameters.Any() ? "[?&](" + String.Join("|", queryStringParameters.Select(item => item.Groups[1].Value)) + ")=[^&]+" : String.Empty);
            var arguments = method.GetParameters()
                .Select(item => new ArgumentInfo(item, FromQueryStringAttribute.For(item), "&test={?test}", "test"));
            var operation = new OperationInfo<Verb>(
                method,
                new Uri(methodUri, UriKind.RelativeOrAbsolute),
                callUri,
                new Regex("^" + methodUri + queryStringRegex + "$"),
                verb,
                arguments.ToArray());
            return _binder.BindArguments(
                new RequestInfo(Verb.GET, new Uri("http://temp.uri" + callUri), new MemoryStream(), new BasicClaimBasedIdentity()),
                new RequestMapping(GetControllerInstance(), operation, new Uri(methodUri, UriKind.Relative)));
        }

        private object Convert(Type type, IRequestInfo request)
        {
            string content = null;
            using (var reader = new StreamReader(request.Body))
            {
                content = reader.ReadToEnd();
            }

            return Convert(type, content);
        }

        private object Convert(Type type, string body)
        {
            if (type == typeof(double))
            {
                return Double.Parse(body);
            }
            else if (type == typeof(int))
            {
                return Int32.Parse(body);
            }
            else if (type == typeof(byte[]))
            {
                return System.Convert.FromBase64String(body);
            }
            else
            {
                return body;
            }
        }

        private IController GetControllerInstance()
        {
            return (IController)ControllerType.GetConstructor(new Type[0]).Invoke(null);
        }
    }
}