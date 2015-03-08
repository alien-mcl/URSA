using System;
using URSA.Web.Http;

namespace URSA.Example.WebApplication.Data
{
    public class Person : Client
    {
        public Person(Uri baseUri) : base(baseUri)
        {
        }

        public void Create(URSA.Example.WebApplication.Data.Person person)
        {
            Call(Verb.POST, new Uri("http://localhost:51509/person"), person);
        }

    }
}