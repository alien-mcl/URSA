using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using RomanticWeb.NamedGraphs;
using URSA.Web.Description.Http;

namespace URSA.Web.Http.Description.NamedGraphs
{
    /// <summary>Provides a owning resource named graph implementation.</summary>
    public class OwningResourceNamedGraphSelector : INamedGraphSelector
    {
        private readonly IDictionary<EntityId, Uri> _cache = new ConcurrentDictionary<EntityId, Uri>();
        private readonly IEnumerable<Regex> _apiUriTemplates;

        /// <summary>Initializes a new instance of the <see cref="OwningResourceNamedGraphSelector"/> class.</summary>
        /// <param name="descriptionBuilders">The description builders.</param>
        public OwningResourceNamedGraphSelector(IEnumerable<IHttpControllerDescriptionBuilder> descriptionBuilders)
        {
            if (descriptionBuilders == null)
            {
                throw new ArgumentNullException("descriptionBuilders");
            }

            _apiUriTemplates = from descriptionBuilder in descriptionBuilders
                               from operation in descriptionBuilder.BuildDescriptor().Operations
                               orderby operation.TemplateRegex.ToString().Length descending 
                               select operation.TemplateRegex;
        }

        /// <summary>Gets the named graphs mapping cache.</summary>
        protected IDictionary<EntityId, Uri> Cache { get { return _cache; } }

        /// <inheritdoc />
        public virtual Uri SelectGraph(EntityId entityId, IEntityMapping entityMapping, IPropertyMapping predicate)
        {
            Uri result;
            if (_cache.TryGetValue(entityId, out result))
            {
                return result;
            }

            string currentUri = String.Join(String.Empty, entityId.Uri.Segments) + entityId.Uri.Query;
            Uri host = new Uri(entityId.Uri.ToString().Substring(0, entityId.Uri.ToString().Length - currentUri.Length).TrimEnd('/'));
            bool isQueryRemoved = false;
            while (currentUri != null)
            {
                if (_apiUriTemplates.Any(template => template.IsMatch(currentUri)))
                {
                    result = new Uri(currentUri, UriKind.Relative).Combine(host);
                    break;
                }

                if ((entityId.Uri.Query.Length > 0) && (!isQueryRemoved))
                {
                    isQueryRemoved = true;
                    currentUri = currentUri.Substring(0, currentUri.Length - entityId.Uri.Query.Length);
                }
                else
                {
                    int position;
                    currentUri = ((position = currentUri.LastIndexOf("/")) > 0 ? currentUri.Substring(0, position) : null);
                }
            }

            if (result == null)
            {
                result = entityId.Uri;
            }

            return _cache[entityId] = result;
        }
    }
}