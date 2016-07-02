using System;
using System.Threading.Tasks;
using RomanticWeb.Entities;
using URSA.Web.Description;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Handles operation hypermedia control.</summary>
    public class OperationHypermediaControl : HypermediaControl
    {
        private readonly OperationInfo<Verb> _operationInfo;
        private readonly IEntityContextProvider _entityContextProvider;
        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;

        internal OperationHypermediaControl(
            HypermediaControlRules rule,
            OperationInfo<Verb> operationInfo,
            IApiDescriptionBuilder apiDescriptionBuilder,
            IEntityContextProvider entityContextProvider) : base(rule)
        {
            _operationInfo = operationInfo;
            _apiDescriptionBuilder = apiDescriptionBuilder;
            _entityContextProvider = entityContextProvider;
        }

        /// <inheritdoc />
        public override Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
        {
            var hook = _entityContextProvider.EntityContext.Create<IEntity>(new EntityId((Uri)request.Url));
            if (Rule == HypermediaControlRules.Include)
            {
                _apiDescriptionBuilder.BuildOperationDescription(hook, _operationInfo, request.GetRequestedMediaTypeProfiles());
            }
            else
            {
                var existingId = _operationInfo.CreateId(_entityContextProvider.EntityContext.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative))));
                _entityContextProvider.EntityContext.Delete(existingId);
            }

            _entityContextProvider.EntityContext.Commit();
            return Task.FromResult(result);
        }
    }
}
