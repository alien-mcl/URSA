using System.Collections.Generic;
using RDeF.Entities;
using RDeF.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a closed shape constraint.</summary>
    [Class("shacl", "ClosedShapeConstraint")]
    public interface IClosedShapeConstraint : IConstraintTemplate
    {
        /// <summary>Gets the ignored properties.</summary>
        [Collection("shacl", "ignoredProperties")]
        IList<IEntity> IgnoredProperties { get; }
    }
}