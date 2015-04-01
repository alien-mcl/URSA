using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides an XML documentation based on <![CDATA[Jolt]]>.</summary>
    public class XmlDocProvider : IXmlDocProvider
    {
        private static readonly IDictionary<Assembly, XDocument> AssemblyCache = new ConcurrentDictionary<Assembly, XDocument>();

        /// <inheritdoc />
        public string GetDescription(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            EnsureAssemblyDocumentation(type.Assembly);
            return GetText(AssemblyCache[type.Assembly], element => (element.Attribute("name") != null) && (element.Attribute("name").Value == CreateMemberName(type)));
        }

        /// <inheritdoc />
        public string GetDescription(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            EnsureAssemblyDocumentation(method.DeclaringType.Assembly);
            return GetText(
                AssemblyCache[method.DeclaringType.Assembly], 
                element => (element.Attribute("name") != null) && (element.Attribute("name").Value == CreateMemberName(method)));
        }

        /// <inheritdoc />
        public string GetDescription(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            EnsureAssemblyDocumentation(property.DeclaringType.Assembly);
            return GetText(
                AssemblyCache[property.DeclaringType.Assembly],
                element => (element.Attribute("name") != null) && (element.Attribute("name").Value == CreateMemberName(property)));
        }

        private static string CreateMemberName(MemberInfo member)
        {
            if (member is ConstructorInfo)
            {
                var parameters = ((MethodInfo)member).GetParameters();
                return String.Format(
                    "M:{0}.#ctor{1}",
                    member.DeclaringType.FullName,
                    (parameters.Length == 0 ? String.Empty : String.Format("({0})", String.Join(",", parameters.Select(parameter => parameter.ParameterType.FullName.Replace("&", "@"))))));
            }

            if (member is PropertyInfo)
            {
                return String.Format("P:{0}.{1}", member.DeclaringType.FullName, member.Name);
            }

            if (member is MethodInfo)
            {
                var parameters = ((MethodInfo)member).GetParameters();
                return String.Format(
                    "M:{0}.{1}{2}",
                    member.DeclaringType.FullName,
                    member.Name,
                    (parameters.Length == 0 ? String.Empty : String.Format("({0})", String.Join(",", parameters.Select(parameter => parameter.ParameterType.FullName.Replace("&", "@"))))));
            }

            if (member is Type)
            {
                return String.Format("T:{0}", ((Type)member).FullName);
            }

            return null;
        }

        private static string GetText(XDocument document, Func<XElement, bool> predicate)
        {
            var element = document.Descendants("member").Where(predicate).FirstOrDefault();
            if (element != null)
            {
                element = element.Descendants("summary").FirstOrDefault();
            }

            return element != null ? element.Value : String.Empty;
        }

        private static void EnsureAssemblyDocumentation(Assembly assembly)
        {
            if (AssemblyCache.ContainsKey(assembly))
            {
                return;
            }

            var documentPath = Path.Combine(AppDomain.CurrentDomain.GetPrimaryAssemblyDirectory(), assembly.GetName().Name + ".xml");
            if (!File.Exists(documentPath))
            {
                AssemblyCache[assembly] = new XDocument();
                return;
            }

            Stream documentStream = null;
            try
            {
                documentStream = File.OpenRead(documentPath);
                var document = XDocument.Load(documentStream);
                AssemblyCache[assembly] = document;
            }
            catch
            {
                AssemblyCache[assembly] = new XDocument();
            }
            finally
            {
                if (documentStream != null)
                {
                    documentStream.Close();
                }
            }
        }
    }
}