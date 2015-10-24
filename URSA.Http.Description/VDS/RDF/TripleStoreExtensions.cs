using System;
using System.Linq;
using RomanticWeb;
using RomanticWeb.Configuration;
using URSA.Configuration;
using VDS.RDF;

namespace URSA.Web.Http.Description.VDS.RDF
{
    /// <summary>Provides useful dotNetRDF extensions to make it easier to push third party triples to RomanticWeb.</summary>
    public static class TripleStoreExtensions
    {
        private static readonly Uri PrimaryTopic = new Uri("http://xmlns.com/foaf/0.1/primaryTopic");

        /// <summary>Maps all triples from default graph to their respective graphs and adds a proper statement to RomanticWeb's meta graph.</summary>
        /// <param name="tripleStore">The triple store.</param>
        /// <param name="graphUri">The target graph URI.</param>
        public static void MapToMetaGraph(this ITripleStore tripleStore, Uri graphUri)
        {
            if (tripleStore == null)
            {
                throw new ArgumentNullException("tripleStore");
            }

            if (graphUri == null)
            {
                throw new ArgumentNullException("graphUri");
            }

            var metaGraphUri = ConfigurationSectionHandler.Default.Factories[DescriptionConfigurationSection.Default.DefaultStoreFactoryName].MetaGraphUri;
            var metaGraph = tripleStore.FindOrCreate(metaGraphUri);
            var graph = tripleStore.FindOrCreate(graphUri);
            var graphNode = metaGraph.CreateUriNode(graphUri);
            var primaryTopicNode = metaGraph.CreateUriNode(PrimaryTopic);
            foreach (var triple in graph.Triples)
            {
                var subject = (triple.Subject is IUriNode ? (IUriNode)triple.Subject : ((IBlankNode)triple.Subject).FindBlankNodeOwner(tripleStore));
                if (subject == null)
                {
                    continue;
                }

                metaGraph.Assert(new Triple(graphNode, primaryTopicNode, subject.CopyNode(metaGraph)));
            }
        }

        internal static IGraph FindOrCreate(this ITripleStore tripleStore, Uri uri)
        {
            var graph = tripleStore.Graphs.FirstOrDefault(item => AbsoluteUriComparer.Default.Equals(item.BaseUri, uri));
            if (graph != null)
            {
                return graph;
            }

            graph = new ThreadSafeGraph() { BaseUri = uri };
            tripleStore.Add(graph);
            return graph;
        }

        private static IUriNode FindBlankNodeOwner(this IBlankNode blankNode, ITripleStore tripleStore)
        {
            INode current = blankNode;
            while ((current = tripleStore.Triples.Where(triple => triple.Object.Equals(current)).Select(triple => triple.Subject).FirstOrDefault()) != null)
            {
                if (current is IUriNode)
                {
                    return (IUriNode)current;
                }
            }

            return null;
        }
    }
}