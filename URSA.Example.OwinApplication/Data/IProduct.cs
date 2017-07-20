using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RDeF.Entities;
using RDeF.Mapping.Attributes;
using RDeF.Mapping.Mapping.Fluent;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    [Class(Iri = "http://temp.uri/vocab#Product")]
    public interface IProduct : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the product's name.</summary>
        [Required]
        [Property(Iri = "http://temp.uri/vocab#name")]
        string Name { get; set; }

        /// <summary> Gets or sets the products's price.</summary>
        [Property(Iri = "http://temp.uri/vocab#price")]
        double Price { get; set; }

        /// <summary> Gets the products's features.</summary>
        [Collection(Iri = "http://temp.uri/vocab#feature")]
        ICollection<string> Features { get; }

        /// <summary> Gets the products's categories.</summary>
        [Collection(Iri = "http://temp.uri/vocab#category")]
        IList<string> Categories { get; }

        /// <summary>Gets or sets the product that his product is part of.</summary>
        [Property(Iri = "http://temp.uri/vocab#partOf")]
        IProduct PartOf { get; set; }

        /// <summary>Gets the similar products.</summary>
        [Collection(Iri = "http://temp.uri/vocab#similar")]
        ICollection<IProduct> Similar { get; }

        /// <summary>Gets the replacement products.</summary>
        [Collection(Iri = "http://temp.uri/vocab#replacement")]
        IList<IProduct> Replacements { get; }
    }

    /// <summary>Provides additional mappings for the <see cref="IProduct" />.</summary>
    public class ProductMap : EntityMap<IProduct>
    {
        /// <summary>Initializes a new instance of the <see cref="ProductMap"/> class.</summary>
        public ProductMap()
        {
            WithProperty(instance => instance.Key).MappedTo(new Iri("http://temp.uri/vocab#key"));
        }
    }
}