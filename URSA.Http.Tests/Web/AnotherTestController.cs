using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [ExcludeFromCodeCoverage]
    [DependentRoute]
    public class AnotherTestController<T> : IController where T : IController
    {
        public IResponseInfo Response { get; set; }

        [Route("/")]
        public string Some()
        {
            return null;
        }

        public void DoSomething(Person person)
        {
        }
    }
}