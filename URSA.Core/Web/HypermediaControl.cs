using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Defines hypermedia control rules.</summary>
    public enum HypermediaControlRules
    {
        /// <summary>Defines a rule that should include a given hypermedia control in the response.</summary>
        Include,

        /// <summary>Defines a rule that should prevent a given hypermedia control in the response.</summary>
        Prevent
    }

    /// <summary>Describes an abstract hypermedia response specification.</summary>
    public abstract class HypermediaControl : IResponseModelTransformer
    {
        /// <summary>Initializes a new instance of the <see cref="HypermediaControl"/> class.</summary>
        /// <param name="rule">The rule associated with that hypermedia control.</param>
        protected HypermediaControl(HypermediaControlRules rule)
        {
            Rule = rule;
        }

        /// <summary>Gets the rule associated with that hypermedia control.</summary>
        public HypermediaControlRules Rule { get; private set; }

        /// <inheritdoc />
        public abstract Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments);
    }
}
