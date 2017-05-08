using System;
using System.Threading.Tasks;
using RDeF.Entities;
using URSA.Web.Description;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Handles operation hypermedia control.</summary>
    public class OperationHypermediaControl : HypermediaControl
    {
        private readonly OperationInfo<Verb> _operationInfo;
        private readonly IEntityContext _entityContext;
        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;
        private readonly IHttpServerConfiguration _httpServerConfiguration;

        internal OperationHypermediaControl(
            HypermediaControlRules rule,
            OperationInfo<Verb> operationInfo,
            IApiDescriptionBuilder apiDescriptionBuilder,
            IEntityContext entityContext,
            IHttpServerConfiguration httpServerConfiguration) : base(rule)
        {
            _operationInfo = operationInfo;
            _apiDescriptionBuilder = apiDescriptionBuilder;
            _entityContext = entityContext;
            _httpServerConfiguration = httpServerConfiguration;
        }

        /// <inheritdoc />
        public override Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
        {
            var hook = _entityContext.Create<IEntity>(new Iri((Uri)request.Url));
            if (Rule == HypermediaControlRules.Include)
            {
                _apiDescriptionBuilder.BuildOperationDescription(hook, _operationInfo, request.GetRequestedMediaTypeProfiles());
            }
            else
            {
                var existingId = _operationInfo.CreateId(_httpServerConfiguration.BaseUri);
                _entityContext.Delete(existingId);
            }

            _entityContext.Commit();
            return Task.FromResult(result);
        }
    }
}
