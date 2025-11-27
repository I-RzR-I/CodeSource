// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-20 21:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-20 21:34
// ***********************************************************************
//  <copyright file="TypeExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

#if NETSTANDARD1_0 || NETSTANDARD1_5
using System.Reflection;
#endif

#endregion

namespace CodeSource.Extensions.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A type extensions.
    /// </summary>
    /// =================================================================================================
    internal static class TypeExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A Type extension method that query if 'baseType' is assignable from portable.
        /// </summary>
        /// <param name="baseType">The baseType to act on.</param>
        /// <param name="derivedType">Type of the derived.</param>
        /// <returns>
        ///     True if assignable from portable, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsAssignableFromPortable(this Type baseType, Type derivedType)
        {
#if NETSTANDARD1_0 || NETSTANDARD1_5
            return baseType.GetTypeInfo().IsAssignableFrom(derivedType.GetTypeInfo());
#else
            return baseType.IsAssignableFrom(derivedType);
#endif
        }
    }
}