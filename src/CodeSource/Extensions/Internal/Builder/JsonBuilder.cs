// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-19 20:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-20 19:17
// ***********************************************************************
//  <copyright file="JsonBuilder.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.IO;
using System.Text;

#endregion

namespace CodeSource.Extensions.Internal.Builder
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A JSON builder.
    /// </summary>
    /// =================================================================================================
    internal static class JsonBuilder
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON indent.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="indent">(Optional) The indent.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonIndent(this StreamWriter sw, string indent = null)
        {
            if (indent.IsNotNull())
                sw.Write(indent);

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON new line.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonNewLine(this StreamWriter sw)
        {
            sw.Write("\n");

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON open array.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="indentBefore">(Optional) The indent before.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonOpenArray(this StreamWriter sw, string indentBefore = null)
        {
            if (indentBefore.IsNotNull())
                sw.Write(indentBefore);

            sw.Write("[");
            sw.WriteJsonNewLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON close array.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="indentBefore">(Optional) The indent before.</param>
        /// <param name="newLineBefore">(Optional) True to new line before.</param>
        /// <param name="newLineAfter">(Optional) True to new line after.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonCloseArray(this StreamWriter sw, string indentBefore = null,
            bool newLineBefore = true, bool newLineAfter = true)
        {
            if (indentBefore.IsNotNull())
                sw.Write(indentBefore);

            if (newLineBefore)
                sw.WriteJsonNewLine();

            sw.Write("]");

            if (newLineAfter)
                sw.WriteJsonNewLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON object delimiter.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="newLine">(Optional) True to new line.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonObjDelimiter(this StreamWriter sw, bool newLine = true)
        {
            sw.Write("," + (newLine ? "\n" : ""));

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON open object.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="indentBefore">(Optional) The indent before.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonOpenObject(this StreamWriter sw, string indentBefore = null)
        {
            if (!indentBefore.IsNotNull())
                sw.Write(indentBefore);

            sw.Write("{");
            sw.WriteJsonNewLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a JSON close object.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="indentBefore">(Optional) The indent before.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteJsonCloseObject(this StreamWriter sw, string indentBefore = null)
        {
            if (indentBefore.IsNotNull())
                sw.Write(indentBefore);

            sw.Write("}");

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A StreamWriter extension method that writes a property.
        /// </summary>
        /// <param name="sw">The sw to act on.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="comma">(Optional) True to comma.</param>
        /// <param name="writeValue">(Optional) True to write value.</param>
        /// <param name="newLine">(Optional) True to new line.</param>
        /// <returns>
        ///     A StreamWriter.
        /// </returns>
        /// =================================================================================================
        internal static StreamWriter WriteProp(this StreamWriter sw, string name, string value, bool comma = true,
            bool writeValue = true, bool newLine = true)
        {
            sw.Write("\"");
            sw.Write(Escape(name));
            sw.Write("\" : ");

            if (writeValue)
            {
                sw.Write(value.IsNull() ? "null" : EscapeAndQuote(value));
            }

            if (comma && writeValue)
                sw.Write(",");

            if (newLine)
                sw.WriteJsonNewLine();

            return sw;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Escape and quote.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string EscapeAndQuote(string value)
        {
            return "\"" + Escape(value) + "\"";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Escapes.
        /// </summary>
        /// <param name="source">The string.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string Escape(string source)
        {
            if (source.IsMissing())
                return source.IfIsNullThenEmpty();

            var sb = new StringBuilder(source.Length + 12);
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32 || c > 0x7F)
                            sb.Append("\\u" + ((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}