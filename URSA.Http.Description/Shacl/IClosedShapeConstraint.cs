using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a closed shape constraint.</summary>
    [Class("sh", "ClosedShapeConstraint")]
    public interface IClosedShapeConstraint : IConstraintTemplate
    {
        /// <summary>Gets the ignored properties.</summary>
        [Collection("sh", "ignoredProperties")]
        IList<IEntity> IgnoredProperties { get; }
    }
}