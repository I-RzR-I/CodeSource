// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-18 01:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 01:22
// ***********************************************************************
//  <copyright file="EnumerableExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.Linq;

#endregion

namespace CodeSource.Extensions.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An enumerable extensions.
    /// </summary>
    /// =================================================================================================
    internal static class EnumerableExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IEnumerable&lt;T&gt; extension method that queries if a null or is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if the null or is t>, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || source?.Count() == 0;
        }
    }
}