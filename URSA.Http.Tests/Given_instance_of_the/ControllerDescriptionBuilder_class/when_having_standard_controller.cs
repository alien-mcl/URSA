using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Specialized;
using Moq;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.ControllerDescriptionBuilder_class
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class when_having_standard_controller
    {
        private ControllerDescriptionBuilder<TestController> _builder;

        [Test]
        public void it_should_describe_Add_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Add");
            var details = ((IControllerDescriptionBuilder)_builder).BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UrlTemplate.Should().Be("/api/test/add{?operandA,operandB}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/add$");
            details.Url.ToString().Should().Be("/api/test/add");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("{?operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be("{?operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [Test]
        public void it_should_describe_Substract_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Substract");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UrlTemplate.Should().Be("/api/test/sub/{operandA}?operandB={operandB}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/sub/[^/?]+$");
            details.Url.ToString().Should().Be("/api/test/sub");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("/api/test/sub/{operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be("&operandB={operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [Test]
        public void it_should_describe_Multiply_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Multiply");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UrlTemplate.Should().Be("/api/test/multiply/{operandB}?operandA={operandA}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/multiply/[^/?]+$");
            details.Url.ToString().Should().Be("/api/test/multiply");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("&operandA={operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be("/api/test/multiply/{operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [Test]
        public void it_should_describe_Divide_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Divide");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
            details.UrlTemplate.Should().Be("/api/test/div/{operandA}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/div/[^/?]+$");
            details.Url.ToString().Should().Be("/api/test/div");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("/api/test/div/{operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be(null);
            details.Arguments.Last().VariableName.Should().Be(null);
        }

        [Test]
        public void it_should_describe_Modulo_method_correctly()
        {
            var method = typeof(TestController).GetMethod("PostModulo");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.POST);
            details.Url.ToString().Should().Be("/api/test/modulo");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be(null);
            details.Arguments.First().VariableName.Should().Be(null);
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be(null);
            details.Arguments.Last().VariableName.Should().Be(null);
        }

        [Test]
        public void it_should_describe_Power_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Power");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UrlTemplate.Should().Be("/api/test/power/{operandA}/{operandB}");
            details.TemplateRegex.ToString().Should().Be("^/api/test/power(/[^/?]+)?(/[^/?]+)?$");
            details.Url.ToString().Should().Be("/api/test/power");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("/api/test/power/{operandA}");
            details.Arguments.First().VariableName.Should().Be("operandA");
            details.Arguments.Last().Parameter.Should().Be(method.GetParameters().Last());
            details.Arguments.Last().UrlTemplate.Should().Be("/api/test/power/{operandA}/{operandB}");
            details.Arguments.Last().VariableName.Should().Be("operandB");
        }

        [Test]
        public void it_should_describe_Log_method_correctly()
        {
            var method = typeof(TestController).GetMethod("Log");
            var details = _builder.BuildDescriptor().Operations.Cast<OperationInfo<Verb>>().FirstOrDefault(operation => operation.UnderlyingMethod == method);

            details.Should().NotBeNull();
            details.ProtocolSpecificCommand.Should().Be(Verb.GET);
            details.UrlTemplate.Should().Be("/api/test/log{?operands*}");
            details.Url.ToString().Should().Be("/api/test/log");
            details.Arguments.Should().HaveCount(method.GetParameters().Length);
            details.Arguments.First().Parameter.Should().Be(method.GetParameters().First());
            details.Arguments.First().UrlTemplate.Should().Be("{?operands*}");
            details.Arguments.First().VariableName.Should().Be("operands");
        }

        [Test]
        public void it_should_retrieve_methods_associated_HTTP_verb()
        {
            var result = _builder.GetMethodVerb(typeof(TestController).GetMethod("Log"));

            result.Should().Be(Verb.GET);
        }

        [Test]
        public void it_should_throw_when_obtaining_associated_HTTP_verb_for_null_method()
        {
            _builder.Invoking(instance => instance.GetMethodVerb(null)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void it_should_throw_when_obtaining_associated_HTTP_verb_for_method_which_doesnt_belong_to_given_type()
        {
            _builder.Invoking(instance => instance.GetMethodVerb(typeof(object).GetMethod("ToString"))).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void it_should_retrieve_methods_Uri()
        {
            var method = typeof(TestController).GetMethod("Log");
            IEnumerable<ArgumentInfo> mappings;
            var result = _builder.GetOperationUrlTemplate(method, out mappings);

            result.Should().Be("/api/test/log{?operands*}");
            mappings.Should().HaveCount(method.GetParameters().Length);
            mappings.First().Parameter.Should().Be(method.GetParameters()[0]);
        }

        [Test]
        public void it_should_throw_when_obtaining_operation_Uri_for_null_method()
        {
            IEnumerable<ArgumentInfo> mappings;
            _builder.Invoking(instance => instance.GetOperationUrlTemplate(null, out mappings)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void it_should_throw_when_obtaining_operation_Uri_for_method_which_doesnt_belong_to_given_type()
        {
            IEnumerable<ArgumentInfo> mappings;
            _builder.Invoking(instance => instance.GetOperationUrlTemplate(typeof(object).GetMethod("ToString"), out mappings)).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [SetUp]
        public void Setup()
        {
            Mock<IDefaultValueRelationSelector> defaultSourceSelector = new Mock<IDefaultValueRelationSelector>(MockBehavior.Strict);
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>(), It.IsAny<Verb>()))
                .Returns<ParameterInfo, Verb>((parameter, verb) => FromQueryStringAttribute.For(parameter));
            defaultSourceSelector.Setup(instance => instance.ProvideDefault(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(parameter => new ToBodyAttribute());
            _builder = new ControllerDescriptionBuilder<TestController>(defaultSourceSelector.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _builder = null;
        }
    }
}