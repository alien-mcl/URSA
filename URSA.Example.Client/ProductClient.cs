//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a URSA HTTP client proxy generation tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using URSA.Web.Http;

namespace Vocab
{
    [System.CodeDom.Compiler.GeneratedCode("URSA HTTP client proxy generation tool", "1.0")]
    public partial class ProductClient : Client
    {
        public ProductClient(HttpUrl baseUri, string authenticationScheme) : base(baseUri, authenticationScheme)
        {
        }

        public ProductClient(HttpUrl baseUri) : base(baseUri)
        {
        }

        public System.Guid Create(Vocab.IProduct product)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json" };
            var contentType = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json" };
            var result = System.Threading.Tasks.Task.Run(async () => await Call<System.Guid>(Verb.POST, "/api/product#POSTProduct", accept, contentType, uriArguments, product)).Result;
            return result;
        }

        public void Delete(System.Guid id)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[0];
            var contentType = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            uriArguments["id"] = id;
            System.Threading.Tasks.Task.Run(async () => await Call(Verb.DELETE, "/api/product/{id}", accept, contentType, uriArguments)).Wait();
        }

        public Vocab.IProduct Get(System.Guid id)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[] {
                "text/turtle",
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml" };
            var contentType = new string[] {
                "text/turtle",
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml" };
            uriArguments["id"] = id;
            var result = System.Threading.Tasks.Task.Run(async () => await Call<Vocab.IProduct>(Verb.GET, "/api/product/{id}", accept, contentType, uriArguments)).Result;
            return result;
        }

        public void Update(System.Guid id, Vocab.IProduct product)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[0];
            var contentType = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            uriArguments["id"] = id;
            System.Threading.Tasks.Task.Run(async () => await Call(Verb.PUT, "/api/product/{id}", accept, contentType, uriArguments, product)).Wait();
        }

        public System.Collections.Generic.ICollection<Vocab.IProduct> List(out System.Int32 totalEntities, System.Int32 _top, System.Int32 _skip, System.String _filter)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            var contentType = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            uriArguments["%24filter"] = _filter;
            uriArguments["totalEntities"] = totalEntities = 0;
            uriArguments["%24skip"] = _skip;
            uriArguments["%24top"] = _top;
            var result = System.Threading.Tasks.Task.Run(async () => await Call<System.Collections.Generic.ICollection<Vocab.IProduct>>(Verb.GET, "/api/product{?%24skip,%24top,%24filter}", accept, contentType, uriArguments)).Result;
            totalEntities = (int)uriArguments["totalEntities"];
            return result;
        }
    }
}