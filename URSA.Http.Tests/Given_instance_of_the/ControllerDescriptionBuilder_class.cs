using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Reflection;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class ControllerDescriptionBuilder_class
    {
        private ControllerDescriptionBuilder<TestController> _builder;

        [TestMethod]
        public void it_should_describe_Add_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Add");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/add?operandA={?operandA}&operandB={?operandB}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/add([?&](operandA|operandB)=[^&]+){0,}$");
            details.Uri.ToString().Should().Be("/api/test/add");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("&operandA={?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be("&operandB={?operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [TestMethod]
        public void it_should_describe_Substract_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Substract");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/sub/operandA/{?operandA}?operandB={?operandB}");
            details.Uri.ToString().Should().Be("/api/test/sub");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("/api/test/sub/operandA/{?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be("&operandB={?operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [TestMethod]
        public void it_should_describe_Multiply_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Multiply");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/multiply/operandB/{?operandB}?operandA={?operandA}");
            details.Uri.ToString().Should().Be("/api/test/multiply");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("&operandA={?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be("/api/test/multiply/operandB/{?operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [TestMethod]
        public void it_should_describe_Divide_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Divide");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.POST);
            details.UriTemplate.Should().Be("/api/test/div/operandA/{?operandA}");
            details.Uri.ToString().Should().Be("/api/test/div");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("/api/test/div/operandA/{?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be(null);
            details.Arguments.Last().VariableName.Should().Be(null);
        }

        [TestMethod]
        public void it_should_describe_Modulo_method_correctly()
        {
            var method = typeof(TestController).GetMethod("PostModulo");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.POST);
            details.Uri.ToString().Should().Be("/api/test/modulo");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be(null);
            details.Arguments.First().VariableName.Should().Be(null);
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be(null);
            details.Arguments.Last().VariableName.Should().Be(null);
        }

        [TestMethod]
        public void it_should_describe_Power_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Power");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/power/operandA/{?operandA}/operandB/{?operandB}");
            details.Uri.ToString().Should().Be("/api/test/power");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("/api/test/power/operandA/{?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UriTemplate.Should().Be("/api/test/power/operandA/{?operandA}/operandB/{?operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [TestMethod]
        public void it_should_describe_Log_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Log");
            var details = _builder.BuildDescriptor().Operations.Cast<URSA.Web.Description.Http.OperationInfo>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.Verb.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/log?operands={?operands}");
            details.Uri.ToString().Should().Be("/api/test/log");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("&operands={?operands}");
            details.Arguments.First().VariableName.Should().Be("operands");
        }

        [TestInitialize]
        public void Setup()
        {
            Mock<IDefaultParameterSourceSelector> defaultSourceSelector = new Mock<IDefaultParameterSourceSelector>();
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) => FromQueryStringAttribute.For(parameter));
            _builder = new ControllerDescriptionBuilder<TestController>(defaultSourceSelector.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _builder = null;
        }
    }
}