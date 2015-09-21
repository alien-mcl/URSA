using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    [Class("http://temp.uri/vocab#Product")]
    public interface IProduct : IEntity, IControlledEntity<EntityId>
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
}