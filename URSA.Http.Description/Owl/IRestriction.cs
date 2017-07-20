using RDeF.Entities;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Owl
{
    /// <summary>Describes a restriction.</summary>
    [Class("owl", "Restriction")]
    public interface IRestriction : IClass
    {
        /// <summary>Gets or sets the property onto which the restriction is put.</summary>
        [Property("owl", "onProperty")]
        Rdfs.IProperty OnProperty { get; set; }

        /// <summary>Gets or sets a type of which all the values are.</summary>
        [Property("owl", "allValuesFrom")]
        IEntity AllValuesFrom { get; set; }

        /// <summary>Gets or sets the maximum cardinality.</summary>
        [Property("owl", "maxCardinality")]
        uint MaxCardinality { get; set; }
    }
}