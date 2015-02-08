using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a property.</summary>
    [Class("rdfs", "Property")]
    public interface IProperty : IEntity
    {
        /// <summary>Gets or sets the label.</summary>
        [Property("rdfs", "label")]
        string Label { get; set; }

        /// <summary>Gets or sets the description.</summary>
        [Property("rdfs", "comment")]
        string Description { get; set; }

        /// <summary>Gets the range.</summary>
        [Collection("rdfs", "range")]
        ICollection<IEntity> Range { get; }

        /// <summary>Gets the domain.</summary>
        [Collection("rdfs", "domain")]
        ICollection<IClass> Domain { get; }
    }
}