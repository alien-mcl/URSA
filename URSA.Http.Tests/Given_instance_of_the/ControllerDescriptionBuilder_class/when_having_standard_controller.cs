using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.ControllerDescriptionBuilder_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_having_standard_controller
    {
        private ControllerDescriptionBuilder<TestController> _builder;

        [TestMethod]
        public void it_should_describe_Add_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Add");
            var details = ((IControllerDescriptionBuilder)_builder).BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/power/operandA/{?operandA}/operandB/{?operandB}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/power(/operandA/[^/?]+)?(/operandB/[^/?]+)?$");
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
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UriTemplate.Should().Be("/api/test/log?operands={?operands}");
            details.Uri.ToString().Should().Be("/api/test/log");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UriTemplate.Should().Be("&operands={?operands}");
            details.Arguments.First().VariableName.Should().Be("operands");
        }

        [TestMethod]
        public void it_should_retrieve_methods_associated_HTTP_verb()
        {
            var result = _builder.GetMethodVerb(typeof(TestController).GetMethod("Log"));

            result.Should().Be(Verb.GET);
        }

        [TestMethod]
        public void it_should_throw_when_obtaining_associated_HTTP_verb_for_null_method()
        {
            _builder.Invoking(instance => instance.GetMethodVerb(null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_throw_when_obtaining_associated_HTTP_verb_for_method_which_doesnt_belong_to_given_type()
        {
            _builder.Invoking(instance => instance.GetMethodVerb(typeof(object).GetMethod("ToString"))).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void it_should_retrieve_methods_Uri()
        {
            var method = typeof(TestController).GetMethod("Log");
            IEnumerable<ArgumentInfo> mappings;
            var result = _builder.GetOperationUriTemplate(method, out mappings);

            result.Should().Be("/api/test/log?operands={?operands}");
            mappings.Should().HaveCount(method.GetParameters().Length);
            mappings.First().Parameter.Should().Be(method.GetParameters()[0]);
        }

        [TestMethod]
        public void it_should_throw_when_obtaining_operation_Uri_for_null_method()
        {
            IEnumerable<ArgumentInfo> mappings;
            _builder.Invoking(instance => instance.GetOperationUriTemplate(null, out mappings)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_throw_when_obtaining_operation_Uri_for_method_which_doesnt_belong_to_given_type()
        {
            IEnumerable<ArgumentInfo> mappings;
            _builder.Invoking(instance => instance.GetOperationUriTemplate(typeof(object).GetMethod("ToString"), out mappings)).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestInitialize]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) => FromQueryStringAttribute.For(parameter));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new ControllerDescriptionBuilder<TestController>(defaultSourceSelector.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _builder = null;
        }
    }
}