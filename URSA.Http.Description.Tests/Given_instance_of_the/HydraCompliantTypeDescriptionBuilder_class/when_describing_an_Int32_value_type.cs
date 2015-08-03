#pragma warning disable 1591
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;

namespace Given_instance_of_the.HydraCompliantTypeDescriptionBuilder_class
{
    [TestClass]
    public class when_describing_an_Int32_value_type : HydraCompliantTypeDescriptionBuilderTest<int>
    {
        [TestMethod]
        public void it_should_create_a_resource_description()
        {
            var result = Builder.BuildTypeDescription(DescriptionContext.ForType(ApiDocumentation, typeof(int)));

            result.SingleValue.Should().HaveValue().And.Subject.Value.Should().BeTrue();
            result.Type.Should().BeAssignableTo<IResource>();
        }

        [TestMethod]
        public void it_should_create_a_type_description()
        {
            var result = (IResource)Builder.BuildTypeDescription(DescriptionContext.ForType(ApiDocumentation, typeof(int))).Type;

            result.Label.Should().Be("int");
            result.Description.Should().BeNull();
        }
    }
}