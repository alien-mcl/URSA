using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Shacl
{
    /// <summary>Describes a property constraint.</summary>
    [Class("shacl", "PropertyConstraint")]
    public interface IPropertyConstraint : IConstraintTemplate
    {
        /// <summary>Gets or sets the predicate.</summary>
        [Property("shacl", "predicate")]
        IEntity Predicate { get; set; }

        /// <summary>Gets or sets the default value.</summary>
        [Property("shacl", "defaultValue")]
        object DefaultValue { get; set; }

        /// <summary>Gets the allowed values.</summary>
        [Collection("shacl", "allowedValues")]
        IList<IEntity> AllowedValues { get; }

        /// <summary>Gets or sets the data type.</summary>
        [Property("shacl", "datatype")]
        IEntity Datatype { get; set; }

        /// <summary>Gets or sets the value shape.</summary>
        [Property("shacl", "valueShape")]
        IShape ValueShape { get; set; }

        /// <summary>Gets or sets the qualified value shape.</summary>
        [Property("shacl", "qualifiedValueShape")]
        IShape QualifiedValueShape { get; set; }

        /// <summary>Gets or sets the qualified minimum count.</summary>
        [Property("shacl", "qualifiedMinCount")]
        int? QualifiedMinCount { get; set; }

        /// <summary>Gets or sets the qualified maximum count.</summary>
        [Property("shacl", "qualifiedMaxCount")]
        int? QualifiedMaxCount { get; set; }

        /// <summary>Gets or sets the direct type of all values.</summary>
        [Property("shacl", "directValueType")]
        IClass DirectValueType { get; set; }

        /// <summary>Gets or sets the class of all values.</summary>
        [Property("shacl", "valueClass")]
        IClass ValueClass { get; set; }

        /// <summary>Gets or sets the value.</summary>
        [Property("shacl", "hasValue")]
        object Value { get; set; }

        /// <summary>Gets or sets the minimum count.</summary>
        [Property("shacl", "minCount")]
        int? MinCount { get; set; }

        /// <summary>Gets or sets the maximum count.</summary>
        [Property("shacl", "maxCount")]
        int? MaxCount { get; set; }

        /// <summary>Gets or sets the minimum exclusive count.</summary>
        [Property("shacl", "minExclusive")]
        int? MinExclusive { get; set; }

        /// <summary>Gets or sets the maximum exclusive count.</summary>
        [Property("shacl", "maxExclusive")]
        int? MaxExclusive { get; set; }

        /// <summary>Gets or sets the minimum inclusive count.</summary>
        [Property("shacl", "minInclusive")]
        int? MinInclusive { get; set; }

        /// <summary>Gets or sets the maximum inclusive count.</summary>
        [Property("shacl", "maxInclusive")]
        int? MaxInclusive { get; set; }

        /// <summary>Gets or sets the minimum string length.</summary>
        [Property("shacl", "minLength")]
        int? MinLength { get; set; }

        /// <summary>Gets or sets the maximum string length.</summary>
        [Property("shacl", "maxLength")]
        int? MaxLength { get; set; }

        /// <summary>Gets or sets the kind of the node.</summary>
        [Property("shacl", "nodeKind")]
        INodeKind NodeKind { get; set; }

        /// <summary>Gets or sets the pattern.</summary>
        [Property("shacl", "pattern")]
        string Pattern { get; set; }
    }
}