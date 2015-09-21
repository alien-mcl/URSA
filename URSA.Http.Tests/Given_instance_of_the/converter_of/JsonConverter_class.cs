using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class JsonConverter_class : ComplexTypeConverterTest<URSA.Web.Http.Converters.JsonConverter>
    {
        private const string ContentType = "application/json";

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override string SerializeObject<TI>(TI obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}