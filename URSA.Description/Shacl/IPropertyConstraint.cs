using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a property constraint.</summary>
    [Class("sh", "PropertyConstraint")]
    public interface IPropertyConstraint : IConstraintTemplate
    {
        /// <summary>Gets or sets the predicate.</summary>
        [Property("sh", "predicate")]
        IEntity Predicate { get; set; }

        /// <summary>Gets or sets the default value.</summary>
        [Property("sh", "defaultValue")]
        object DefaultValue { get; set; }

        /// <summary>Gets the allowed values.</summary>
        [Collection("sh", "allowedValues")]
        IList<IEntity> AllowedValues { get; }

        /// <summary>Gets or sets the data type.</summary>
        [Property("sh", "datatype")]
        IEntity Datatype { get; set; }

        /// <summary>Gets or sets the value shape.</summary>
        [Property("sh", "valueShape")]
        IShape ValueShape { get; set; }

        /// <summary>Gets or sets the qualified value shape.</summary>
        [Property("sh", "qualifiedValueShape")]
        IShape QualifiedValueShape { get; set; }

        /// <summary>Gets or sets the qualified minimum count.</summary>
        [Property("sh", "qualifiedMinCount")]
        int? QualifiedMinCount { get; set; }

        /// <summary>Gets or sets the qualified maximum count.</summary>
        [Property("sh", "qualifiedMaxCount")]
        int? QualifiedMaxCount { get; set; }

        /// <summary>Gets or sets the direct type of all values.</summary>
        [Property("sh", "directValueType")]
        IClass DirectValueType { get; set; }

        /// <summary>Gets or sets the class of all values.</summary>
        [Property("sh", "valueClass")]
        IClass ValueClass { get; set; }

        /// <summary>Gets or sets the value.</summary>
        [Property("sh", "hasValue")]
        object Value { get; set; }

        /// <summary>Gets or sets the minimum count.</summary>
        [Property("sh", "minCount")]
        int? MinCount { get; set; }

        /// <summary>Gets or sets the maximum count.</summary>
        [Property("sh", "maxCount")]
        int? MaxCount { get; set; }

        /// <summary>Gets or sets the minimum exclusive count.</summary>
        [Property("sh", "minExclusive")]
        int? MinExclusive { get; set; }

        /// <summary>Gets or sets the maximum exclusive count.</summary>
        [Property("sh", "maxExclusive")]
        int? MaxExclusive { get; set; }

        /// <summary>Gets or sets the minimum inclusive count.</summary>
        [Property("sh", "minInclusive")]
        int? MinInclusive { get; set; }

        /// <summary>Gets or sets the maximum inclusive count.</summary>
        [Property("sh", "maxInclusive")]
        int? MaxInclusive { get; set; }

        /// <summary>Gets or sets the minimum string length.</summary>
        [Property("sh", "minLength")]
        int? MinLength { get; set; }

        /// <summary>Gets or sets the maximum string length.</summary>
        [Property("sh", "maxLength")]
        int? MaxLength { get; set; }

        /// <summary>Gets or sets the kind of the node.</summary>
        [Property("sh", "nodeKind")]
        INodeKind NodeKind { get; set; }

        /// <summary>Gets or sets the pattern.</summary>
        [Property("sh", "pattern")]
        string Pattern { get; set; }
    }
}