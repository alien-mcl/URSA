using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Mapping.Fluent;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    [Class("http://temp.uri/vocab#Product")]
    public interface IProduct : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the product's name.</summary>
        [Required]
        [Property("http://temp.uri/vocab#name")]
        string Name { get; set; }

        /// <summary> Gets or sets the products's price.</summary>
        [Property("http://temp.uri/vocab#price")]
        double Price { get; set; }

        /// <summary> Gets or sets the products's features.</summary>
        [Collection("http://temp.uri/vocab#feature")]
        ICollection<string> Features { get; }

        /// <summary> Gets or sets the products's categories.</summary>
        [Collection("http://temp.uri/vocab#category")]
        IList<string> Categories { get; }

        /// <summary>Gets or sets the product that his product is part of.</summary>
        [Property("http://temp.uri/vocab#partOf")]
        IProduct PartOf { get; set; }
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