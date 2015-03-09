using System;
using System.Dynamic;
using URSA.Web.Http;

namespace URSA.Example.WebApplication.Data
{
    public class Person : Client
    {
        public Person(Uri baseUri) : base(baseUri)
        {
        }

        public void Delete(System.Guid idURSA.Example.WebApplication.Data.Person person)
        {
            ExpandoObject uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.DELETE, new Uri("http://localhost:51509/person/#withId"), uriArguments, person);
        }

        public void Delete(System.Guid idURSA.Example.WebApplication.Data.Person person)
        {
            ExpandoObject uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.GET, new Uri("http://localhost:51509/person/#withId"), uriArguments, person);
        }

        public void Delete(System.Guid idURSA.Example.WebApplication.Data.Person person)
        {
            ExpandoObject uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.PUT, new Uri("http://localhost:51509/person/#withId"), uriArguments, person);
        }

        public void Get(System.Object pageSystem.Object pageSize)
        {
            ExpandoObject uriArguments = new ExpandoObject();
            uriArguments.page = page;
            uriArguments.pageSize = pageSize;
            Call(Verb.GET, new Uri("http://localhost:51509/person/#withPageAndPageSize"), uriArguments);
        }

        public void Create(URSA.Example.WebApplication.Data.Person person)
        {
            ExpandoObject uriArguments = new ExpandoObject();
            Call(Verb.POST, new Uri("http://localhost:51509/person"), uriArguments, person);
        }

    }
}