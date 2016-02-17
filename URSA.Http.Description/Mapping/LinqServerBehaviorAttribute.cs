using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description.Mapping
{
    /// <summary>Describes supported LINQ operation behaviors.</summary>
    public enum LinqOperations
    {
        /// <summary>Defines the Take top N elements operation.</summary>
        Take,

        /// <summary>Defines the Skip N elements operation.</summary>
        Skip,

        /// <summary>Defines an entity filter operation.</summary>
        Filter
    }

    /// <summary>Marks as a LINQ based operation behavior.</summary>
    [ExcludeFromCodeCoverage]
    public class LinqServerBehaviorAttribute : ServerBehaviorAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="LinqServerBehaviorAttribute"/> class.</summary>
        /// <param name="linqOperation">The LINQ operation.</param>
        public LinqServerBehaviorAttribute(LinqOperations linqOperation)
        {
            Operation = linqOperation;
        }

        /// <summary>Gets the LINQ operation behavior.</summary>
        public LinqOperations Operation { get; private set; }

        /// <inheritdoc />
        public override void Accept<T>(IServerBehaviorAttributeVisitor visitor, IIriTemplateMapping templateMapping, DescriptionContext descriptionContext)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }

            if (templateMapping == null)
            {
                throw new ArgumentNullException("templateMapping");
            }

            visitor.Visit<T>(this, templateMapping, descriptionContext);
        }
    }
}