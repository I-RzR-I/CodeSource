// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-14 20:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-14 23:37
// ***********************************************************************
//  <copyright file="StringExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Globalization;

#endregion

namespace CodeSource.Extensions.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A string extensions.
    /// </summary>
    /// =================================================================================================
    internal static class StringExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that query if 'source' is missing.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if missing, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsMissing(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that query if 'source' is present.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if present, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsPresent(this string source)
        {
            return !source.IsMissing();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that if is null then empty.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string IfIsNullThenEmpty(this string source)
        {
            return (source ?? string.Empty).Trim();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that sets copy right.
        /// </summary>
        /// <param name="sourceValue">The sourceValue to act on.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string SetCopyRight(this string sourceValue)
        {
            return $"{(sourceValue.IfIsNullThenEmpty().IsPresent() ? $"© {sourceValue}" : null)}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that sets applied date.
        /// </summary>
        /// <param name="sourceDateValue">The sourceDateValue to act on.</param>
        /// <returns>
        ///     A DateTime?
        /// </returns>
        /// =================================================================================================
        internal static DateTime? SetAppliedDate(this string sourceDateValue)
        {
            try
            {
                var date = sourceDateValue.IfIsNullThenEmpty().IsMissing()
                    ? (DateTime?)null
                    : DateTime.ParseExact(sourceDateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                return date;
            }
            catch
            {
                return null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that validates the source URL described by sourceUrl.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="sourceUrl">The sourceUrl to act on.</param>
        /// =================================================================================================
        internal static void ValidateSourceUrl(this string sourceUrl)
        {
            if (sourceUrl.IsPresent() && !Uri.IsWellFormedUriString(sourceUrl, UriKind.Absolute))
                throw new ArgumentException("SourceUrl must be an absolute URI", nameof(sourceUrl));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets code path.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string SetCodePath(string fullName, string currentItemName)
        => currentItemName.IsPresent()
            ? $"{fullName}{(currentItemName.StartsWith(".") ? currentItemName : ($".{currentItemName}"))}"
            : $"{fullName}{currentItemName}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string SetFullName(string fullName, string currentItemName)
        => $"{fullName}{(currentItemName.StartsWith(".") ? currentItemName : ($".{currentItemName}"))}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that indent multiply.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="multiplex">(Optional) The multiplex.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string IndentMultiply(this string source, int multiplex = 0)
        {
            if (multiplex == -1) return string.Empty;
            if (multiplex == 0) return source;

            var indentResult = source;
            for (var i = 0; i < multiplex; i++) 
                indentResult += source;

            return indentResult;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A string extension method that if not missing.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string IfNotMissing(this string source, string newValue)
            => source.IsNull() ? null : newValue;
    }
}