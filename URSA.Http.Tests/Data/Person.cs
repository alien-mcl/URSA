using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Tests.Data
{
    [ExcludeFromCodeCoverage]
    public class Person : IControlledEntity<int>
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string[] Roles { get; set; }
    }
}