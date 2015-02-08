using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>Provides a JSON-LD dotNetRDF parsing integration.</summary>
    /// <remarks>This code is based on <![CDATA[https://github.com/NuGet/NuGet.Services.Metadata/tree/master/src/Catalog/JsonLdIntegration]]>.</remarks>
    public class JsonLdParser : IRdfReader
    {
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
                    JToken json = JToken.Load(jsonReader);
                    json = JsonLD.Core.JsonLdProcessor.Expand(json);
                    foreach (JObject subjectJObject in json)
                    {
                        string subject = subjectJObject["@id"].ToString();
                        JToken type;
                        if ((subjectJObject.TryGetValue("@type", out type)) && (!HandleType(type, handler, subject)))
                        {
                            return;
                        }

                        foreach (JProperty property in subjectJObject.Properties())
                        {
                            if ((property.Name == "@id") || (property.Name == "@type"))
                            {
                                continue;
                            }

                            if (!HandleProperty(property, handler, subject))
                            {
                                return;
                            }
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

        private bool HandleProperty(JProperty property, IRdfHandler handler, string subject)
        {
            foreach (JObject objectJObject in property.Value)
            {
                JToken id;
                JToken value;
                if (objectJObject.TryGetValue("@id", out id))
                {
                    if (!HandleTriple(handler, subject, property.Name, id.ToString(), null, false))
                    {
                        return false;
                    }
                }
                else if (objectJObject.TryGetValue("@value", out value))
                {
                    string datatype = null;
                    JToken datatypeJToken;
                    if (objectJObject.TryGetValue("@type", out datatypeJToken))
                    {
                        datatype = datatypeJToken.ToString();
                    }
                    else
                    {
                        datatype = MapType(value.Type);
                    }

                    if (!HandleTriple(handler, subject, property.Name, value.ToString(), datatype, true))
                    {
                        return false;
                    }
                }
            }

            return true;
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

        private bool HandleType(JToken type, IRdfHandler handler, string subject)
        {
            if (type is JArray)
            {
                foreach (JToken t in (JArray)type)
                {
                    if (!HandleTriple(handler, subject, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", t.ToString(), null, false))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!HandleTriple(handler, subject, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", type.ToString(), null, false))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HandleTriple(IRdfHandler handler, string subject, string predicate, string obj, string datatype, bool isLiteral)
        {
            INode subjectNode;
            if (subject.StartsWith("_"))
            {
                string nodeId = subject.Substring(subject.IndexOf(":") + 1);
                subjectNode = handler.CreateBlankNode(nodeId);
            }
            else
            {
                subjectNode = handler.CreateUriNode(new Uri(subject));
            }

            INode predicateNode = handler.CreateUriNode(new Uri(predicate));
            INode objNode;
            if (isLiteral)
            {
                if (datatype == "http://www.w3.org/2001/XMLSchema#boolean")
                {
                    obj = ((string)obj).ToLowerInvariant();
                }

                objNode = (datatype == null) ? handler.CreateLiteralNode((string)obj) : handler.CreateLiteralNode((string)obj, new Uri(datatype));
            }
            else
            {
                if (obj.StartsWith("_"))
                {
                    string nodeId = obj.Substring(obj.IndexOf(":") + 1);
                    objNode = handler.CreateBlankNode(nodeId);
                }
                else
                {
                    objNode = handler.CreateUriNode(new Uri(obj));
                }
            }

            return handler.HandleTriple(new Triple(subjectNode, predicateNode, objNode));
        }
    }
}