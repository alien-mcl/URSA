using System.Collections.Generic;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a data shape.</summary>
    [Class("sh", "Shape")]
    public interface IShape : IResource
    {
        /// <summary>Gets the properties of a given shape.</summary>
        [Collection("sh", "property")]
        ICollection<IPropertyConstraint> Properties { get; }

        /// <summary>Gets the constraints.</summary>
        [Collection("sh", "constraint")]
        ICollection<IConstraint> Constraints { get; }

            /// <summary>Gets the filter shapes.</summary>
        [Collection("sh", "filterShape")]
        ICollection<IShape> FilterShapes { get; }
    }
}