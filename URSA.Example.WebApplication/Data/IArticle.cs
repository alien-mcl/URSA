using System;
using System.ComponentModel.DataAnnotations;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Mapping.Fluent;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    [Class("http://temp.uri/vocab#Article")]
    public interface IArticle : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the articles's title.</summary>
        [Required]
        [Property("http://temp.uri/vocab#title")]
        string Title { get; set; }

        /// <summary> Gets or sets the articles's body.</summary>
        [Property("http://temp.uri/vocab#body")]
        string Body { get; set; }
    }

    /// <summary>Provides additional mappings for the <see cref="IArticle" />.</summary>
    public class ArticleMap : EntityMap<IArticle>
    {
        /// <summary>Initializes a new instance of the <see cref="ArticleMap"/> class.</summary>
        public ArticleMap()
        {
            Property(instance => instance.Key).Term.Is(new Uri("http://temp.uri/vocab#key"));
        }
    }
}