//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a URSA HTTP client proxy generation tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

        public System.String Firstname { get; set; }

        public System.String Lastname { get; set; }

        public System.Guid Id { get; set; }

        public System.String Roles { get; set; }

        public RemoveResult Remove(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.DELETE, "/person/id/{?id}", uriArguments);
        }

        public QueryResult Query(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.GET, "/person/id/{?id}", uriArguments);
        }

        public System.Nullable%601[[System.Boolean,%20mscorlib,%20Version=4.0.0.Nullable`1 Update(System.Guid id, URSA.Example.WebApplication.Data.Person person)
        {
            dynamic uriArguments = new ExpandoObject();
            uriArguments.id = id;
            Call(Verb.PUT, "/person/id/{?id}", uriArguments, person);
        }

        public System.Collections.Generic.IEnumerable%601[[URSA.Example.WebApplication.Data.Person,%20URSA.Example.WebApplication,%20Version=1.0.0.IEnumerable`1 Get(System.Object page, System.Object pageSize)
        {
            dynamic uriArguments = new ExpandoObject();
            uriArguments.page = page;
            uriArguments.pageSize = pageSize;
            Call(Verb.GET, "/person?page={?page}&pageSize={?pageSize}", uriArguments);
        }

        public CreateResult Create(URSA.Example.WebApplication.Data.Person person)
        {
            dynamic uriArguments = new ExpandoObject();
            Call(Verb.POST, "http://localhost:51509/person/#withPerson", uriArguments, person);
        }

        public class RemoveResult
        {


        public System.Nullable%601[[System.Boolean,%20mscorlib,%20Version=4.0.0.Nullable`1 Nullable`1 { get;}

        public URSA.Example.WebApplication.Data.Person Person { get;}
        }

        public class QueryResult
        {


        public System.Nullable%601[[System.Boolean,%20mscorlib,%20Version=4.0.0.Nullable`1 Nullable`1 { get;}

        public URSA.Example.WebApplication.Data.Person Person { get;}
        }

        public class CreateResult
        {


        public System.Guid& Guid& { get;}

        public System.Nullable%601[[System.Boolean,%20mscorlib,%20Version=4.0.0.Nullable`1 Nullable`1 { get;}
        }
    }
}