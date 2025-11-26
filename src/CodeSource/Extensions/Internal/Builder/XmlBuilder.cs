// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-19 20:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-20 19:14
// ***********************************************************************
//  <copyright file="XmlBuilder.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#endregion

namespace CodeSource.Extensions.Internal.Builder
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An XML builder helper.
    /// </summary>
    /// =================================================================================================
    internal static class XmlBuilder
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes an XML root.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="value">The value.</param>
        /// =================================================================================================
        internal static StreamWriter WriteXmlRoot(this StreamWriter sw, string value)
        {
            sw.Write(value);
            sw.WriteLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes an XML open element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="name">The element name.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <param name="inline">(Optional) True to inline element.</param>
        /// =================================================================================================
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes an XML close element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="name">The element name.</param>
        /// <param name="indent">The indent.</param>
        /// =================================================================================================
        internal static StreamWriter WriteXmlCloseElement(this StreamWriter sw, string name, string indent)
        {
            sw.Write(indent);

            sw.Write("</");
            sw.Write(name);
            sw.Write('>');

            sw.WriteLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes an XML element.
        /// </summary>
        /// <param name="sw">The StreamWriter to act on.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="name">The element name.</param>
        /// <param name="value">The element value.</param>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <param name="inline">(Optional) True to inline element.</param>
        /// =================================================================================================
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Escape XML.
        /// </summary>
        /// <param name="source">Source for the XML escape.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string EscapeXml(string source)
        {
            if (source.IsMissing())
                return source.IfIsNullThenEmpty();

            var sb = new StringBuilder(source.Length + 8);
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '\"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&apos;"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.ToString();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds an attribute.
        /// </summary>
        /// <param name="attributes">(Optional) The attributes.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string BuildAttribute(Dictionary<string, string> attributes = null)
        {
            if (attributes.HasAnyData())
            {
                var attributesRow = string.Empty;
                if (attributes.HasAnyData())
                    attributesRow = attributes!.Keys.Aggregate(attributesRow,
                        (current, key) => current + $" {key} = \"{attributes[key]}\"");

                return attributesRow;
            }

            return string.Empty;
        }
    }
}