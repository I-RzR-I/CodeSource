// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="XmlBuilder.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#endregion

namespace RzR.Core.CodeSource.Extensions.Internal.Builder
{
    /// <summary>
    ///     An XML builder helper.
    /// </summary>
    internal static class XmlBuilder
    {
        /// <summary>
        ///     A StreamWriter extension method that writes an XML root.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        internal static StreamWriter WriteXmlRoot(this StreamWriter sw, string value)
        {
            sw.Write(value);
            sw.WriteLine();

            return sw;
        }

        /// <summary>
        ///     A StreamWriter extension method that writes an XML open element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="name">The element name.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <param name="inline">(Optional) True to inline element.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        internal static StreamWriter WriteXmlOpenElement(this StreamWriter sw, string name, string indent,
            Dictionary<string, string> attributes = null, bool inline = false)
        {
            var attributesRow = BuildAttribute(attributes);
            if (inline)
            {
                sw.Write($"<{name}{attributesRow}>");
            }
            else
            {
                sw.Write(indent);
                sw.Write('<');
                sw.Write(name);
                sw.Write(attributesRow);
                sw.Write('>');
            }

            sw.WriteLine();

            return sw;
        }

        /// <summary>
        ///     A StreamWriter extension method that writes an XML close element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="name">The element name.</param>
        /// <param name="indent">The indent.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        internal static StreamWriter WriteXmlCloseElement(this StreamWriter sw, string name, string indent)
        {
            sw.Write(indent);

            sw.Write("</");
            sw.Write(name);
            sw.Write('>');

            sw.WriteLine();

            return sw;
        }

        /// <summary>
        ///     A StreamWriter extension method that writes an XML element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="name">The element name.</param>
        /// <param name="value">The element value.</param>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <param name="inline">(Optional) True to inline element.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        internal static StreamWriter WriteXmlElement(this StreamWriter sw, string indent, string name, string value,
            Dictionary<string, string> attributes = null, bool inline = false)
        {
            var attributesRow = BuildAttribute(attributes);
            var escapedValue = value.IfNotMissing(EscapeXml(value));

            if (inline)
            {
                sw.Write($"<{name}{attributesRow}>{escapedValue.IfIsNullThenEmpty()}</{name}>");
            }
            else
            {
                sw.Write(indent);

                sw.Write('<');
                sw.Write(name);
                sw.Write(attributesRow);
                sw.Write('>');

                if (escapedValue.IsPresent())
                    sw.Write(escapedValue);

                sw.Write("</");
                sw.Write(name);
                sw.Write('>');
            }

            sw.WriteLine();

            return sw;
        }

        /// <summary>
        ///     Escape XML.
        /// </summary>
        /// <param name="source">Source for the XML escape.</param>
        /// <param name="isAttributeValue">
        ///     (Optional) True when escaping a double-quoted attribute value. Attribute values are not
        ///     collapsed to empty when they contain only whitespace.
        /// </param>
        /// <returns>
        ///     A string.
        /// </returns>
        private static string EscapeXml(string source, bool isAttributeValue = false)
        {
            if (source.IsNull() || (!isAttributeValue && source.IsMissing()))
                return string.Empty;

            var sb = new StringBuilder(source.Length + 8);
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (char.IsHighSurrogate(c) && i + 1 < source.Length && char.IsLowSurrogate(source[i + 1]))
                {
                    sb.Append(c).Append(source[++i]);
                    continue;
                }

                if (!IsXmlChar(c))
                {
                    sb.Append('\uFFFD');
                    continue;
                }

                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '\"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&apos;"); break;
                    case '\r': sb.Append("&#xD;"); break;
                    case '\t' when isAttributeValue: sb.Append("&#x9;"); break;
                    case '\n' when isAttributeValue: sb.Append("&#xA;"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Query if a single UTF-16 code unit is an XML 1.0 <c>Char</c> on its own (#x9 | #xA | #xD
        ///     | [#x20-#xD7FF] | [#xE000-#xFFFD]). Surrogates are valid only as a well-formed pair,
        ///     which the caller checks first.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns>
        ///     True if the character is allowed, false if not.
        /// </returns>
        private static bool IsXmlChar(char c)
        {
            return c == '\t' || c == '\n' || c == '\r'
                   || (c >= '\u0020' && c <= '\uD7FF')
                   || (c >= '\uE000' && c <= '\uFFFD');
        }

        /// <summary>
        ///     Builds an attribute. Values are escaped for a double-quoted attribute; keys are written
        ///     as-is and must be internal constants, never data-derived.
        /// </summary>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        private static string BuildAttribute(Dictionary<string, string> attributes = null)
        {
            if (attributes.HasAnyData())
            {
                var attributesRow = string.Empty;
                if (attributes.HasAnyData())
                    attributesRow = attributes!.Keys.Aggregate(attributesRow,
                        (current, key) => current + $" {key} = \"{EscapeXml(attributes[key], true)}\"");

                return attributesRow;
            }

            return string.Empty;
        }
    }
}