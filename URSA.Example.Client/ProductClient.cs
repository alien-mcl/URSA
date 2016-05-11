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

        public System.Guid Create(Vocab.Product product)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[] {
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json",
                "application/owl+xml" };
            var contentType = new string[] {
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json",
                "application/owl+xml" };
            var result = Call<System.Guid>(Verb.POST, "/api/product#POSTProduct", accept, contentType, uriArguments, product);
            return result;
        }

        public System.Collections.Generic.ICollection<Product> List(out System.Int32 totalEntities, System.String _filter, System.Int32 _skip, System.Int32 _top)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[] {
                "application/ld+json",
                "text/turtle",
                "application/owl+xml",
                "application/rdf+xml" };
            var contentType = new string[] {
                "application/ld+json",
                "text/turtle",
                "application/owl+xml",
                "application/rdf+xml" };
            uriArguments["%24filter"] = _filter;
            uriArguments["totalEntities"] = totalEntities = 0;
            uriArguments["%24skip"] = _skip;
            uriArguments["%24top"] = _top;
            var result = Call<System.Collections.Generic.ICollection<Product>>(Verb.GET, "/api/product{?%24skip,%24top,%24filter}", accept, contentType, uriArguments);
            totalEntities = (int)uriArguments["totalEntities"];
            return result;
        }

        public Vocab.Product Get(System.Guid id)
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
            uriArguments["id"] = id;
            var result = Call<Vocab.Product>(Verb.GET, "/api/product/{id}", accept, contentType, uriArguments);
            return result;
        }

        public void Update(System.Guid id, Vocab.Product product)
        {
            System.Collections.Generic.IDictionary<string, object> uriArguments = new System.Collections.Generic.Dictionary<string, object>();
            var accept = new string[0];
            var contentType = new string[] {
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json",
                "application/owl+xml" };
            uriArguments["id"] = id;
            Call(Verb.PUT, "/api/product/{id}", accept, contentType, uriArguments, product);
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
            Call(Verb.DELETE, "/api/product/{id}", accept, contentType, uriArguments);
        }
    }
}