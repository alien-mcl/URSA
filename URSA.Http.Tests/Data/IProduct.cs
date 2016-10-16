using System;
using System.Collections.Generic;
using RomanticWeb.Entities;

namespace URSA.Web.Http.Tests.Data
{
    /// <summary>Describes a person.</summary>
    public interface IProduct : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the product's name.</summary>
        string Name { get; set; }

        /// <summary> Gets or sets the products's price.</summary>
        double Price { get; set; }

        /// <summary> Gets the products's features.</summary>
        ICollection<string> Features { get; }

        /// <summary> Gets the products's categories.</summary>
        IList<string> Categories { get; }

        /// <summary>Gets or sets the product that his product is part of.</summary>
        IProduct PartOf { get; set; }

        /// <summary>Gets the similar products.</summary>
        ICollection<IProduct> Similar { get; }

        /// <summary>Gets the replacement products.</summary>
        IList<IProduct> Replacements { get; }
    }
}