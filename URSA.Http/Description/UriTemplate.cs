using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    internal class UriTemplate : ICloneable
    {
        private readonly bool _isRegexMode;
        private readonly Type _controlledEntityType;
        private readonly SegmentList _segments;
        private readonly QueryStringList _queryString;

        internal UriTemplate(Type controlledEntityType, bool isRegexMode = false)
            : this(new SegmentList(controlledEntityType, isRegexMode), new QueryStringList(controlledEntityType, isRegexMode), controlledEntityType, isRegexMode)
        {
        }

        private UriTemplate(SegmentList segments, QueryStringList queryString, Type controlledEntityType, bool isRegexMode = false)
        {
            _controlledEntityType = controlledEntityType;
            _isRegexMode = isRegexMode;
            _segments = segments;
            _queryString = queryString;
        }

        /// <summary>Performs an implicit conversion from <see cref="UriTemplate"/> to <see cref="String"/>.</summary>
        /// <param name="uriTemplate">The URI template.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator String(UriTemplate uriTemplate)
        {
            return uriTemplate == null ? null : uriTemplate.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0}{1}{2}", (_isRegexMode ? String.Empty : "/"), Segments, QueryString);
        }

        public string ToString(bool withQueryString)
        {
            return (withQueryString ? ToString() : String.Format("{0}{1}", (_isRegexMode ? String.Empty : "/"), Segments));
        }

        internal string Segments { get { return _segments.ToString(); } }

        internal string QueryString { get { return _queryString.ToString(); } }

        internal void Add(string part, Attribute source, ICustomAttributeProvider member, bool hasDefaultValue = false)
        {
            part = (_isRegexMode ? ((source is FromQueryStringAttribute) ? part.Replace("=[^&]+", String.Empty) : part) : part).Trim('/');
            if (part.Length == 0)
            {
                return;
            }

            (source is FromQueryStringAttribute ? (UriTemplatePartList)_queryString : _segments).Add(part, source, member, hasDefaultValue);
        }

        internal UriTemplate Clone(bool? isRegexMode = null)
        {
            return new UriTemplate(
                new SegmentList(_controlledEntityType, isRegexMode ?? _isRegexMode, _segments),
                new QueryStringList(_controlledEntityType, isRegexMode ?? _isRegexMode, _queryString),
                _controlledEntityType,
                isRegexMode ?? _isRegexMode);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        internal class UriTemplatePart
        {
            internal UriTemplatePart(string part, Attribute source, ICustomAttributeProvider member, bool hasDefaultValue = false)
            {
                Part = ((source is FromQueryStringAttribute) && (part[0] == '&') ? part.Substring(1) : part);
                Source = source;
                Member = member;
                HasDefaultValue = hasDefaultValue;
            }

            internal string Part { get; private set; }

            internal Attribute Source { get; private set; }

            internal ICustomAttributeProvider Member { get; private set; }

            internal bool HasDefaultValue { get; private set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return Part;
            }
        }

        private abstract class UriTemplatePartList : List<UriTemplatePart>
        {
            protected UriTemplatePartList(Type controlledEntityType, bool isRegexMode, IEnumerable<UriTemplatePart> parts = null) : base(parts ?? new UriTemplatePart[0])
            {
                ControlledEntityType = controlledEntityType;
                IsRegexMode = isRegexMode;
            }

            protected Type ControlledEntityType { get; private set; }

            protected bool IsRegexMode { get; private set; }

            internal virtual void Add(string part, Attribute source, ICustomAttributeProvider member, bool hasDefaultValue = false)
            {
                Add(new UriTemplatePart(part, source, member, hasDefaultValue));
            }
        }

        private class SegmentList : UriTemplatePartList
        {
            internal SegmentList(Type controlledEntityType, bool isRegexMode, IEnumerable<UriTemplatePart> parts = null) : base(controlledEntityType, isRegexMode, parts)
            {
            }

            /// <inheritdoc />
            public override string ToString()
            {
                if (Count == 0)
                {
                    return String.Empty;
                }

                if (!IsRegexMode)
                {
                    return String.Join("/", this.Select(segment => segment.Part));
                }

                StringBuilder result = new StringBuilder(256);
                foreach (var segment in this)
                {
                    result.AppendFormat("{0}/{1}{2}", (segment.HasDefaultValue ? "(" : String.Empty), segment.Part, (segment.HasDefaultValue ? ")?" : String.Empty));
                }

                return result.ToString();
            }

            internal override void Add(string part, Attribute source, ICustomAttributeProvider member, bool hasDefaultValue = false)
            {
                base.Add(part, source, member, hasDefaultValue);
                if (ControlledEntityType == null)
                {
                    return;
                }

                int indexOf = -1;
                Type implementation;
                var identifierSegment = this
                    .Where((item, index) => (item.Source is FromUriAttribute) && (item.Member is ParameterInfo) &&
                        ((implementation = ControlledEntityType.GetInterfaces().First(@interface => (@interface.IsGenericType) && (@interface.GetGenericTypeDefinition() == typeof(IControlledEntity<>)))) != null) &&
                        (((ParameterInfo)item.Member).ParameterType == implementation.GetProperty("Id").PropertyType) &&
                        ((indexOf = index) != -1))
                    .FirstOrDefault();
                if ((indexOf == -1) || ((this[0].Source == null) && (indexOf == 1)) || ((this[0].Source != null) && (indexOf == 2)))
                {
                    return;
                }

                var temp = this[indexOf - 1];
                this[indexOf - 1] = this[indexOf];
                this[indexOf] = temp;
            }
        }

        private class QueryStringList : UriTemplatePartList
        {
            internal QueryStringList(Type controlledEntityType, bool isRegexMode, IEnumerable<UriTemplatePart> parts = null) : base(controlledEntityType, isRegexMode, parts)
            {
            }

            /// <inheritdoc />
            public override string ToString()
            {
                if (Count == 0)
                {
                    return String.Empty;
                }

                if (!IsRegexMode)
                {
                    return String.Format("?{0}", String.Join("&", this.Select(pair => pair.Part)));
                }

                var totalParameters = 0;
                var optionalParameters = 0;
                foreach (var parameter in this)
                {
                    totalParameters++;
                    if (parameter.HasDefaultValue)
                    {
                        optionalParameters++;
                    }
                }

                return (optionalParameters == totalParameters ? "(" : String.Empty) +
                    "[?&](" + String.Join("|", this.Select(pair => pair.Part)) + ")=[^&]+" +
                    (optionalParameters == totalParameters ? "){0,}" : String.Empty);
            }
        }
    }
}