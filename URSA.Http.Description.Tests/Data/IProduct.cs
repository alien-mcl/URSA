using System;
using System.Collections.Generic;
using RDeF.Entities;
using RDeF.Mapping.Attributes;
using RDeF.Mapping.Mapping.Fluent;

namespace URSA.Web.Http.Description.Tests.Data
{
    /// <summary>Describes a person.</summary>
    [Class(Iri = "http://temp.uri/vocab#Product")]
    public interface IProduct : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the product's name.</summary>
        [Property(Iri = "http://temp.uri/vocab#name")]
        string Name { get; set; }

        /// <summary> Gets or sets the products's price.</summary>
        [Property(Iri = "http://temp.uri/vocab#price")]
        float Price { get; set; }

        /// <summary> Gets or sets the products's features.</summary>
        [Collection(Iri = "http://temp.uri/vocab#feature")]
        ICollection<string> Features { get; set; }

        /// <summary>Gets or sets the related product.</summary>
        [Property(Iri = "http://temp.uri/vocab#related")]
        IProduct RelatedProduct { get; set; }

        /// <summary>Gets or sets similar products.</summary>
        [Collection(Iri = "http://temp.uri/vocab#similar")]
        ICollection<IProduct> Similar { get; set; } 
    }

    /// <summary>Provides additional mappings for the <see cref="IProduct" />.</summary>
    public class ProductMap : EntityMap<IProduct>
    {
        /// <summary>Initializes a new instance of the <see cref="ProductMap"/> class.</summary>
        public ProductMap()
        {
            WithProperty(instance => instance.Key).MappedTo(new Uri("http://temp.uri/vocab#key"));
        }
    }
}