using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using RomanticWeb.Vocabularies;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>Provides a JSON-LD dotNetRDF parsing integration.</summary>
    /// <remarks>This code is based on <![CDATA[https://github.com/NuGet/NuGet.Services.Metadata/tree/master/src/Catalog/JsonLdIntegration]]>.</remarks>
    [ExcludeFromCodeCoverage]
    public class JsonLdParser : IRdfReader
    {
        private static volatile int _index = 0;

        /// <inheritdoc />
        public event RdfReaderWarning Warning;

        /// <inheritdoc />
        public void Load(IRdfHandler handler, string filename)
        {
            Load(handler, new StreamReader(filename));
        }

        /// <inheritdoc />
        public void Load(IRdfHandler handler, StreamReader input)
        {
            Load(handler, (TextReader)input);
        }

        /// <inheritdoc />
        public void Load(IGraph g, string filename)
        {
            Load(g, new StreamReader(filename));
        }

        /// <inheritdoc />
        public void Load(IGraph g, TextReader input)
        {
            Load(new GraphHandler(g), input);
        }

        /// <inheritdoc />
        public void Load(IGraph g, StreamReader input)
        {
            Load(g, (TextReader)input);
        }

        /// <inheritdoc />
        public void Load(IRdfHandler handler, TextReader input)
        {
            bool finished = false;
            try
            {
                handler.StartRdf();
                using (JsonReader jsonReader = new JsonTextReader(input))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    JToken json = JsonLD.Core.JsonLdProcessor.Expand(JToken.Load(jsonReader));
                    foreach (JObject subjectJObject in json)
                    {
                        string subject = subjectJObject["@id"].ToString();
                        JToken type;
                        if (subjectJObject.TryGetValue("@type", out type))
                        {
                            HandleType(handler, subject, type);
                        }

                        foreach (var property in subjectJObject.Properties().Where(property => (property.Name != "@id") && (property.Name != "@type")))
                        {
                            HandleProperty(handler, subject, property);
                        }
                    }
                }

                finished = true;
                handler.EndRdf(true);
            }
            catch
            {
                finished = true;
                handler.EndRdf(false);
                throw;
            }
            finally
            {
                if (!finished)
                {
                    handler.EndRdf(true);
                }
            }
        }

        private static void HandleType(IRdfHandler handler, string subject, JToken type)
        {
            JArray types = type as JArray ?? new JArray(type);
            foreach (var item in types)
            {
                HandleTriple(handler, subject, Rdf.type.ToString(), item.ToString(), null, false);
            }
        }

        private static void HandleTriple(IRdfHandler handler, string subject, string predicate, string obj, string datatype, bool isLiteral)
        {
            INode subjectNode = (subject.StartsWith("_") ? (INode)handler.CreateBlankNode(subject.Substring(2)) : handler.CreateUriNode(new Uri(subject)));
            INode predicateNode = handler.CreateUriNode(new Uri(predicate));
            INode objNode;
            if (isLiteral)
            {
                if (datatype == "http://www.w3.org/2001/XMLSchema#boolean")
                {
                    obj = obj.ToLowerInvariant();
                }

                objNode = (datatype == null ? handler.CreateLiteralNode(obj) : handler.CreateLiteralNode(obj, new Uri(datatype)));
            }
            else
            {
                objNode = (obj.StartsWith("_") ? (INode)handler.CreateBlankNode(obj.Substring(2)) : handler.CreateUriNode(new Uri(obj)));
            }

            if (!handler.HandleTriple(new Triple(subjectNode, predicateNode, objNode)))
            {
                throw new InvalidOperationException(String.Format("Could not add triple {0} {1} {2} .", subjectNode, predicateNode, objNode));
            }
        }

        private void HandleProperty(IRdfHandler handler, string subject, JProperty property)
        {
            foreach (JObject valueJObject in property.Value)
            {
                JToken list;
                if (valueJObject.TryGetValue("@list", out list))
                {
                    HandleList(handler, subject, property, (JArray)list);
                }
                else
                {
                    var triple = HandleValue(valueJObject);
                    if (triple == null)
                    {
                        continue;
                    }

                    HandleTriple(handler, subject, property.Name, triple.Item1, triple.Item2, triple.Item3);
                }
            }
        }

        private void HandleList(IRdfHandler handler, string subject, JProperty property, JArray list)
        {
            int nextIndex = 0;
            HandleTriple(handler, subject, property.Name, (list.Count == 0 ? Rdf.nil.ToString() : "_:item" + (nextIndex = _index++)), null, false);
            for (var index = 0; index < list.Count; index++)
            {
                var item = (JObject)list[index];
                var triple = HandleValue(item);
                if (triple == null)
                {
                    continue;
                }

                var currentSubject = "_:item" + nextIndex;
                HandleTriple(handler, currentSubject, Rdf.first.ToString(), triple.Item1, triple.Item2, triple.Item3);
                HandleTriple(handler, currentSubject, Rdf.rest.ToString(), (index == list.Count - 1 ? Rdf.nil.ToString() : "_:item" + (nextIndex = _index++)), null, false);
            }
        }

        private Tuple<string, string, bool> HandleValue(JObject objectJObject)
        {
            JToken id;
            JToken value;
            if (objectJObject.TryGetValue("@id", out id))
            {
                return new Tuple<string, string, bool>(id.ToString(), null, false);
            }

            if (!objectJObject.TryGetValue("@value", out value))
            {
                return null;
            }

            JToken datatypeJToken;
            string datatype = (objectJObject.TryGetValue("@type", out datatypeJToken) ? datatypeJToken.ToString() : MapType(value.Type));
            return new Tuple<string, string, bool>(value.ToString(), datatype, true);
        }

        private string MapType(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Boolean:
                    return "http://www.w3.org/2001/XMLSchema#boolean";
                case JTokenType.Float:
                    return "http://www.w3.org/2001/XMLSchema#double";
                case JTokenType.Integer:
                    return "http://www.w3.org/2001/XMLSchema#integer";
                default:
                    if (Warning != null)
                    {
                        Warning.Invoke(String.Format("Token of type '{0}' could not be mapped to literal data type.", type));
                    }

                    break;
            }

            return null;
        }
    }
}