using System.Collections.Generic;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a data shape.</summary>
    [Class("shacl", "Shape")]
    public interface IShape : IResource
    {
        /// <summary>Gets the properties of a given shape.</summary>
        [Collection("shacl", "property")]
        ICollection<IPropertyConstraint> Properties { get; }

        /// <summary>Gets the constraints.</summary>
        [Collection("shacl", "constraint")]
        ICollection<IConstraint> Constraints { get; }

            /// <summary>Gets the filter shapes.</summary>
        [Collection("shacl", "filterShape")]
        ICollection<IShape> FilterShapes { get; }
    }
}