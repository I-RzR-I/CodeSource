// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-19 18:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-19 18:11
// ***********************************************************************
//  <copyright file="ObjectExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace CodeSource.Extensions.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An object extensions.
    /// </summary>
    /// =================================================================================================
    internal static class ObjectExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An object extension method that query if 'source' is null.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if null, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsNull(this object source) => source == null;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An object extension method that query if 'source' is not null.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        ///     True if not null, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsNotNull(this object source) => source != null;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An object extension method that if not null.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        ///     An object.
        /// </returns>
        /// =================================================================================================
        internal static object IfNotNull(this object source, object newValue)
            => source.IsNull() ? null : newValue;
    }
}