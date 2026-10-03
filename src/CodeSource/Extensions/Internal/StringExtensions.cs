// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="StringExtensions.cs" company="RzR SOFT & TECH">
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

#endregion

namespace RzR.Core.CodeSource.Extensions.Internal
{
    /// <summary>
    ///     A string extensions.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        ///     A string extension method that query if 'source' is missing.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if missing, false if not.
        /// </returns>
        internal static bool IsMissing(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        ///     A string extension method that query if 'source' is present.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if present, false if not.
        /// </returns>
        internal static bool IsPresent(this string source)
        {
            return !source.IsMissing();
        }

        /// <summary>
        ///     A string extension method that if is null then empty.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     An empty string when <paramref name="source" /> is null; otherwise the trimmed value.
        /// </returns>
        internal static string IfIsNullThenEmpty(this string source)
        {
            return (source ?? string.Empty).Trim();
        }

        /// <summary>
        ///     A string extension method that sets copy right.
        /// </summary>
        /// <param name="sourceValue">The sourceValue to act on.</param>
        /// <returns>
        ///     The value prefixed with ©, or null when the value is null, empty or white space.
        /// </returns>
        internal static string SetCopyRight(this string sourceValue)
        {
            if (sourceValue.IsMissing())
                return null;

            return sourceValue.TrimStart().StartsWith("©", StringComparison.Ordinal)
                ? sourceValue
                : $"© {sourceValue}";
        }

        /// <summary>
        ///     A string extension method that sets applied date.
        /// </summary>
        /// <param name="sourceDateValue">The sourceDateValue to act on. FORMAT: 'yyyy-MM-dd'.</param>
        /// <returns>
        ///     The parsed date, or null when the value is null, empty, white space or not a valid 'yyyy-
        ///     MM-dd' date. Leading and trailing white space is ignored.
        /// </returns>
        internal static DateTime? SetAppliedDate(this string sourceDateValue)
        {
            if (sourceDateValue.IsMissing())
                return null;

            return DateTime.TryParseExact(sourceDateValue.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date)
                ? date
                : null;
        }

        /// <summary>
        ///     A string extension method that validates the source URL described by sourceUrl.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="sourceUrl">The sourceUrl to act on.</param>
        internal static void ValidateSourceUrl(this string sourceUrl)
        {
            if (sourceUrl.IsPresent() && !Uri.IsWellFormedUriString(sourceUrl, UriKind.Absolute))
                throw new ArgumentException("SourceUrl must be an absolute URI", nameof(sourceUrl));
        }

        /// <summary>
        ///     Sets code path.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        internal static string SetCodePath(string fullName, string currentItemName)
        {
            return currentItemName.IsPresent()
                ? $"{fullName}{(currentItemName.StartsWith(".") ? currentItemName : $".{currentItemName}")}"
                : $"{fullName}{currentItemName}";
        }

        /// <summary>
        ///     Sets full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        internal static string SetFullName(string fullName, string currentItemName)
        {
            return $"{fullName}{(currentItemName.StartsWith(".") ? currentItemName : $".{currentItemName}")}";
        }

        /// <summary>
        ///     A string extension method that indent multiply.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="multiplex">(Optional) The multiplex.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        internal static string IndentMultiply(this string source, int multiplex = 0)
        {
            if (multiplex == -1) return string.Empty;
            if (multiplex == 0) return source;

            var indentResult = source;
            for (var i = 0; i < multiplex; i++)
                indentResult += source;

            return indentResult;
        }

        /// <summary>
        ///     A string extension method that if not missing.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        internal static string IfNotMissing(this string source, string newValue)
        {
            return source.IsNull() ? null : newValue;
        }
    }
}