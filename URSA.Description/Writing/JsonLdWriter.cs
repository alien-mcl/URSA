using JsonLD.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace VDS.RDF.Writing
{
    /// <summary>Provides a JSON-LD doNetRDF writing integration.</summary>
    /// <remarks>This code is based on <![CDATA[https://github.com/NuGet/NuGet.Services.Metadata/tree/master/src/Catalog/JsonLdIntegration]]>.</remarks>
    [ExcludeFromCodeCoverage]
    public class JsonLdWriter : IRdfWriter
    {
        private const string First = "http://www.w3.org/1999/02/22-rdf-syntax-ns#first";
        private const string Rest = "http://www.w3.org/1999/02/22-rdf-syntax-ns#rest";
        private const string Nil = "http://www.w3.org/1999/02/22-rdf-syntax-ns#nil";

        private readonly JToken _context = null;

        /// <summary>Initializes a new instance of the <see cref="JsonLdWriter" /> class.</summary>
        public JsonLdWriter()
        {
            if (Warning != null)
            {
                // This event is not currently used in this writer.
            }
        }

        /// <summary>Initializes a new instance of the <see cref="JsonLdWriter" /> class.</summary>
        /// <param name="context">Optional JSON-LD context to use for serialization.</param>
        public JsonLdWriter(string context)
        {
            if (context != null)
            {
                _context = JToken.Parse(context);
            }
        }

        /// <inheritdoc />
        public event RdfWriterWarning Warning;

        /// <inheritdoc />
        public void Save(IGraph graph, string filename)
        {
            Save(graph, new StreamWriter(filename));
        }

        /// <inheritdoc />
        public void Save(IGraph graph, TextWriter output)
        {
            JToken data = null;
            if (_context != null)
            {
                data = JsonLdProcessor.Compact(MakeExpandedForm(graph), _context, new JsonLdOptions());
            }
            else
            {
                data = MakeExpandedForm(graph);
            }

            output.Write(data.ToString());
            output.Flush();
        }

        private static IDictionary<INode, List<INode>> GetLists(IGraph graph)
        {
            INode first = graph.CreateUriNode(new Uri(First));
            INode rest = graph.CreateUriNode(new Uri(Rest));
            INode nil = graph.CreateUriNode(new Uri(Nil));

            IEnumerable<Triple> ends = graph.GetTriplesWithPredicateObject(rest, nil);
            IDictionary<INode, List<INode>> lists = new Dictionary<INode, List<INode>>();
            foreach (Triple end in ends)
            {
                List<INode> list = new List<INode>();
                Triple iterator = graph.GetTriplesWithSubjectPredicate(end.Subject, first).First();
                INode head = iterator.Subject;
                while (true)
                {
                    list.Add(iterator.Object);
                    IEnumerable<Triple> restTriples = graph.GetTriplesWithPredicateObject(rest, iterator.Subject);
                    if (restTriples.Any())
                    {
                        break;
                    }

                    iterator = graph.GetTriplesWithSubjectPredicate(restTriples.First().Subject, first).First();
                    head = iterator.Subject;
                }

                list.Reverse();
                lists.Add(head, list);
            }

            return lists;
        }

        private static bool IsListNode(INode subject, IGraph graph)
        {
            INode rest = graph.CreateUriNode(new Uri(Rest));
            return (graph.GetTriplesWithSubjectPredicate(subject, rest).Any());
        }

        private static JToken MakeList(List<INode> nodes)
        {
            JArray list = new JArray();
            nodes.ForEach(node => list.Add(MakeObject(node)));
            return new JObject { { "@list", list } };
        }

        private static JToken MakeObject(INode node)
        {
            if (node is IUriNode)
            {
                return new JObject { { "@id", node.ToString() } };
            }
            
            if (node is IBlankNode)
            {
                return new JObject { { "@id", node.ToString() } };
            }

            return MakeLiteralObject((ILiteralNode)node);
        }

        private static JObject MakeLiteralObject(ILiteralNode node)
        {
            if (node.DataType == null)
            {
                return new JObject { { "@value", node.Value } };
            }

            string dataType = node.DataType.ToString();
            switch (dataType)
            {
                case "http://www.w3.org/2001/XMLSchema#integer":
                    return new JObject { { "@value", int.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#boolean":
                    return new JObject { { "@value", bool.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#decimal":
                    return new JObject { { "@value", decimal.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#long":
                    return new JObject { { "@value", long.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#short":
                    return new JObject { { "@value", short.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#float":
                    return new JObject { { "@value", float.Parse(node.Value) } };
                case "http://www.w3.org/2001/XMLSchema#double":
                    return new JObject { { "@value", double.Parse(node.Value) } };
                default:
                    return new JObject { { "@value", node.Value }, { "@type", dataType } };
            }
        }

        private static JToken MakeExpandedForm(IGraph graph)
        {
            IDictionary<INode, List<INode>> lists = GetLists(graph);
            IDictionary<string, JObject> subjects = new Dictionary<string, JObject>();
            foreach (Triple triple in graph.Triples.Where((t) => !IsListNode(t.Subject, graph)))
            {
                string subject = triple.Subject.ToString();
                string predicate = triple.Predicate.ToString();
                if (predicate == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    predicate = "@type";
                }

                JObject properties;
                if (!subjects.TryGetValue(subject, out properties))
                {
                    properties = new JObject();
                    properties.Add("@id", subject);
                    subjects.Add(subject, properties);
                }

                JArray objects;
                JToken o;
                if (!properties.TryGetValue(predicate, out o))
                {
                    properties.Add(predicate, objects = new JArray());
                }
                else
                {
                    objects = (JArray)o;
                }

                if (predicate == "@type")
                {
                    objects.Add(triple.Object.ToString());
                }
                else
                {
                    objects.Add(lists.ContainsKey(triple.Object) ? MakeList(lists[triple.Object]) : MakeObject(triple.Object));
                }
            }

            JArray result = new JArray();
            foreach (JObject subject in subjects.Values)
            {
                result.Add(subject);
            }

            return result;
        }
    }
}