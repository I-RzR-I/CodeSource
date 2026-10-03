// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="EscapeExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Globalization;
using System.Text;

#endregion

namespace RzR.Core.CodeSource.Extensions.Internal
{
    /// <summary>
    ///     Output escaping helpers for the CSV, Markdown and YAML exporters. Every helper treats its
    ///     input as untrusted data (attribute values and member names from scanned assemblies).
    /// 
    /// </summary>
    internal static class EscapeExtensions
    {
        /// <summary>
        ///     Converts a fixed (non data-derived) value to an RFC 4180 CSV field: the value is trimmed,
        ///     embedded double quotes are doubled and the result is wrapped in double quotes.
        /// </summary>
        /// <param name="value">The value to act on; null is treated as empty.</param>
        /// <returns>
        ///     The quoted CSV field.
        /// </returns>
        internal static string ToCsvField(this string value)
        {
            return QuoteCsv(value.IfIsNullThenEmpty());
        }

        /// <summary>
        ///     Converts a data-derived value to an RFC 4180 CSV field with spreadsheet formula
        ///     neutralisation.
        /// </summary>
        /// <param name="value">The value to act on; null is treated as empty.</param>
        /// <returns>
        ///     The neutralised, quoted CSV field.
        /// </returns>
        internal static string ToCsvDataField(this string value)
        {
            var trimmed = value.IfIsNullThenEmpty();
            var sb = new StringBuilder(trimmed.Length + 4);
            var atCellStart = true;
            for (var i = 0; i < trimmed.Length; i++)
            {
                var c = trimmed[i];
                if (atCellStart && !char.IsWhiteSpace(c))
                {
                    atCellStart = false;
                    if (StartsCsvFormula(trimmed, i))
                        sb.Append('\'');
                }

                sb.Append(c);

                if (c == ';' || c == ',' || c == '\r' || c == '\n')
                    atCellStart = true;
            }

            return QuoteCsv(sb.ToString());
        }

        /// <summary>
        ///     Converts a value to a Markdown code span, including its backtick fence.
        /// </summary>
        /// <param name="value">The value to act on; null is treated as empty.</param>
        /// <returns>
        ///     The fenced code span, or an empty string when there is no content.
        /// </returns>
        internal static string ToMarkdownCodeSpan(this string value)
        {
            var content = ReplaceLineBreaks(value.IfIsNullThenEmpty());

            if (content.Length == 0)
                return string.Empty;

            var longestRun = 0;
            var currentRun = 0;
            foreach (var c in content)
            {
                currentRun = c == '`' ? currentRun + 1 : 0;
                if (currentRun > longestRun)
                    longestRun = currentRun;
            }

            var fence = new string('`', longestRun + 1);
            if (content[0] == '`' || content[content.Length - 1] == '`')
                content = " " + content + " ";

            return fence + content + fence;
        }

        /// <summary>
        ///     Converts a value to Markdown (GFM) table cell text.
        /// </summary>
        /// <param name="value">The value to act on; null is treated as empty.</param>
        /// <returns>
        ///     The escaped cell text.
        /// </returns>
        internal static string ToMarkdownTableCell(this string value)
        {
            var source = ReplaceLineBreaks(value.IfIsNullThenEmpty());
            var sb = new StringBuilder(source.Length + 8);
            foreach (var c in source)
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '\\':
                    case '`':
                    case '*':
                    case '[':
                    case ']':
                    case '|':
                        sb.Append('\\').Append(c);
                        break;
                    default: sb.Append(c); break;
                }

            return sb.ToString();
        }

        /// <summary>
        ///     Converts a URL value to Markdown table cell content.
        /// </summary>
        /// <param name="value">The URL value to act on; null is treated as empty.</param>
        /// <returns>
        ///     The cell content.
        /// </returns>
        internal static string ToMarkdownLinkCell(this string value)
        {
            var trimmed = value.IfIsNullThenEmpty();
            if (trimmed.Length == 0)
                return string.Empty;

            var text = trimmed.ToMarkdownTableCell();
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || !IsHttpScheme(uri.Scheme))
                return text;

            return "[" + text + "](" + EncodeMarkdownLinkDestination(uri.AbsoluteUri) + ")";
        }

        /// <summary>
        ///     Converts a value to a YAML double-quoted scalar, including the surrounding quotes.
        /// </summary>
        /// <param name="value">The value to act on; null is emitted as <c>""</c>.</param>
        /// <returns>
        ///     The double-quoted YAML scalar.
        /// </returns>
        internal static string ToYamlDoubleQuoted(this string value)
        {
            if (value.IsNull())
                return "\"\"";

            var sb = new StringBuilder(value.Length + 8);
            sb.Append('"');
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    sb.Append(c).Append(value[++i]);
                    continue;
                }

                if (char.IsSurrogate(c))
                {
                    sb.Append('\uFFFD');
                    continue;
                }

                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\0': sb.Append("\\0"); break;
                    case '\a': sb.Append("\\a"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\v': sb.Append("\\v"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\u001B': sb.Append("\\e"); break;
                    case '\u0085': sb.Append("\\N"); break;
                    case '\u2028': sb.Append("\\L"); break;
                    case '\u2029': sb.Append("\\P"); break;
                    case '\uFEFF':
                    case '\uFFFE':
                    case '\uFFFF':
                        sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                        break;
                    default:
                        if (c < '\u0020' || (c >= '\u007F' && c <= '\u009F'))
                            sb.Append("\\x").Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }

            sb.Append('"');

            return sb.ToString();
        }

        /// <summary>
        ///     Doubles embedded double quotes and wraps the value in double quotes (RFC 4180).
        /// </summary>
        /// <param name="value">The value, already trimmed and never null.</param>
        /// <returns>
        ///     The quoted CSV field.
        /// </returns>
        private static string QuoteCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>
        ///     Query if the cell content starting at <paramref name="index" /> begins, after any run of
        ///     <c>"</c> characters, with a spreadsheet formula trigger.
        /// </summary>
        /// <param name="value">The trimmed value.</param>
        /// <param name="index">The cell-start position.</param>
        /// <returns>
        ///     True if an apostrophe must be inserted at <paramref name="index" />, false if not.
        /// </returns>
        private static bool StartsCsvFormula(string value, int index)
        {
            var i = index;
            while (i < value.Length && value[i] == '"')
                i++;

            return i < value.Length && IsCsvFormulaTrigger(value[i]);
        }

        /// <summary>
        ///     Query if a character starts a spreadsheet formula.
        /// </summary>
        /// <param name="c">The first non-quote character of a cell.</param>
        /// <returns>
        ///     True if the value must be neutralised, false if not.
        /// </returns>
        private static bool IsCsvFormulaTrigger(char c)
        {
            switch (c)
            {
                case '=':
                case '+':
                case '-':
                case '@':
                case '\uFF1D':
                case '\uFF0B':
                case '\uFF0D':
                case '\uFF20':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Replaces every line break (CRLF, CR, LF, U+0085, U+2028, U+2029) with a single space.
        /// </summary>
        /// <param name="value">The value, never null.</param>
        /// <returns>
        ///     The single-line value.
        /// </returns>
        private static string ReplaceLineBreaks(string value)
        {
            var sb = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '\r':
                        if (i + 1 < value.Length && value[i + 1] == '\n')
                            i++;
                        sb.Append(' ');
                        break;
                    case '\n':
                    case '\u0085':
                    case '\u2028':
                    case '\u2029':
                        sb.Append(' ');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Query if a URI scheme is http or https (case-insensitive).
        /// </summary>
        /// <param name="scheme">The URI scheme.</param>
        /// <returns>
        ///     True if the scheme is allowed in a Markdown link, false if not.
        /// </returns>
        private static bool IsHttpScheme(string scheme)
        {
            return string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Percent-encodes the characters that would end or break a Markdown inline link destination
        ///     or a GFM table cell: <c>( ) | space &lt; &gt;</c>, every control character up to U+0020
        ///     and DEL (U+007F).
        /// </summary>
        /// <param name="absoluteUri">The absolute URI string.</param>
        /// <returns>
        ///     The encoded link destination.
        /// </returns>
        private static string EncodeMarkdownLinkDestination(string absoluteUri)
        {
            var sb = new StringBuilder(absoluteUri.Length + 8);
            foreach (var c in absoluteUri)
                switch (c)
                {
                    case '(': sb.Append("%28"); break;
                    case ')': sb.Append("%29"); break;
                    case '|': sb.Append("%7C"); break;
                    case ' ': sb.Append("%20"); break;
                    case '<': sb.Append("%3C"); break;
                    case '>': sb.Append("%3E"); break;
                    default:
                        if (c <= 0x20 || c == 0x7F)
                            sb.Append('%').Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }

            return sb.ToString();
        }
    }
}