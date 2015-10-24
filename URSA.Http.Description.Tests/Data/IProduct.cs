using System;
using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Mapping.Fluent;

namespace URSA.Web.Http.Description.Tests.Data
{
    /// <summary>Describes a person.</summary>
    [Class("http://temp.uri/vocab#Product")]
    public interface IProduct : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the product's name.</summary>
        [Property("http://temp.uri/vocab#name")]
        string Name { get; set; }

        /// <summary> Gets or sets the products's price.</summary>
        [Property("http://temp.uri/vocab#price")]
        float Price { get; set; }

        /// <summary> Gets or sets the products's features.</summary>
        [Collection("http://temp.uri/vocab#feature")]
        ICollection<string> Features { get; set; }
    }

    /// <summary>Provides additional mappings for the <see cref="IProduct" />.</summary>
    public class ProductMap : EntityMap<IProduct>
    {
        /// <summary>Initializes a new instance of the <see cref="ProductMap"/> class.</summary>
        public ProductMap()
        {
            Property(instance => instance.Key).Term.Is(new Uri("http://temp.uri/vocab#key"));
        }
    }
}