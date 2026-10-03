// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="EnumerableExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Collections.Generic;
using System.Linq;

// ReSharper disable ConstantConditionalAccessQualifier

#endregion

namespace RzR.Core.CodeSource.Extensions.Internal
{
    /// <summary>
    ///     An enumerable extensions.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        ///     An IEnumerable&lt;T&gt; extension method that queries if a null or is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if the null or is t>, false if not.
        /// </returns>
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        ///     An IEnumerable&lt;T&gt; extension method that query if 'source' has any data.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if any data, false if not.
        /// </returns>
        internal static bool HasAnyData<T>(this IEnumerable<T> source)
        {
            return !source.IsNullOrEmpty();
        }
    }
}