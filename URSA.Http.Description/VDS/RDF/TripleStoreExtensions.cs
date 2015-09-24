using System;
using System.Linq;
using RomanticWeb;
using VDS.RDF;

namespace URSA.Web.Http.Description.VDS.RDF
{
    /// <summary>Provides useful dotNetRDF extensions to make it easier to push third party triples to RomanticWeb.</summary>
    public static class TripleStoreExtensions
    {
        private static readonly Uri PrimaryTopic = new Uri("http://xmlns.com/foaf/0.1/primaryTopic");

        /// <summary>Maps all triples from default graph to their respective graphs and adds a proper statement to RomanticWeb's meta graph.</summary>
        /// <param name="tripleStore">The triple store.</param>
        /// <param name="metaGraphUri">The meta graph URI.</param>
        public static void MapToMetaGraph(this ITripleStore tripleStore, Uri metaGraphUri)
        {
            if (tripleStore == null)
            {
                throw new ArgumentNullException("tripleStore");
            }

            if (metaGraphUri == null)
            {
                throw new ArgumentNullException("metaGraphUri");
            }

            var defaultGraph = tripleStore.Graphs.FirstOrDefault(item => item.BaseUri == null);
            if (defaultGraph == null)
            {
                return;
            }

            var metaGraph = tripleStore.FindOrCreate(metaGraphUri);
            var primaryTopic = metaGraph.CreateUriNode(PrimaryTopic);
            foreach (var triple in defaultGraph.Triples)
            {
                IGraph graph = null;
                var subject = (triple.Subject is IUriNode ? (IUriNode)triple.Subject : ((IBlankNode)triple.Subject).FindBlankNodeOwner(tripleStore));
                if (subject == null)
                {
                    continue;
                }

                graph = tripleStore.FindOrCreate(subject.Uri);
                if (graph == null)
                {
                    continue;
                }

                var newTriple = new Triple(triple.Subject.CopyNode(graph), triple.Predicate.CopyNode(graph), triple.Object.CopyNode(graph));
                graph.Assert(newTriple);
                var describer = subject.CopyNode(metaGraph);
                metaGraph.Assert(new Triple(describer, primaryTopic, describer));
            }

            defaultGraph.Clear();
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

        private static IGraph FindOrCreate(this ITripleStore tripleStore, Uri uri)
        {
            var graph = tripleStore.Graphs.FirstOrDefault(item => AbsoluteUriComparer.Default.Equals(item.BaseUri, uri));
            if (graph != null)
            {
                return graph;
            }

            graph = new Graph() { BaseUri = uri };
            tripleStore.Add(graph);
            return graph;
        }
    }
}