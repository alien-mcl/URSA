using System;
using System.ComponentModel.DataAnnotations;
using RDeF.Entities;
using RDeF.Mapping.Attributes;
using RDeF.Mapping.Mapping.Fluent;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    [Class(Iri = "http://temp.uri/vocab#Article")]
    public interface IArticle : IEntity, IControlledEntity<Guid>
    {
        /// <summary> Gets or sets the articles's title.</summary>
        [Required]
        [Property(Iri = "http://temp.uri/vocab#title")]
        string Title { get; set; }

        /// <summary> Gets or sets the articles's body.</summary>
        [Property(Iri = "http://temp.uri/vocab#body")]
        string Body { get; set; }
    }

    /// <summary>Provides additional mappings for the <see cref="IArticle" />.</summary>
    public class ArticleMap : EntityMap<IArticle>
    {
        /// <summary>Initializes a new instance of the <see cref="ArticleMap"/> class.</summary>
        public ArticleMap()
        {
            WithProperty(instance => instance.Key).MappedTo(new Iri("http://temp.uri/vocab#key"));
        }
    }
}