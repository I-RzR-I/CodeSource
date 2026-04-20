// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2024-12-23 15:30
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-12-23 19:46
// ***********************************************************************
//  <copyright file="CodeSourceObject.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace CodeSource.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A code source object.
    /// </summary>
    /// =================================================================================================
    public class CodeSourceObject
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// =================================================================================================
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the name of the full.
        /// </summary>
        /// <value>
        ///     The name of the full.
        /// </value>
        /// =================================================================================================
        public string FullName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the history.
        /// </summary>
        /// <value>
        ///     The history.
        /// </value>
        /// =================================================================================================
#if NET45_OR_GREATER || NETSTANDARD || NET
        public IReadOnlyList<CodeSourceObjectHistory> History { get; set; }
#else
        public IList<CodeSourceObjectHistory> History { get; set; }
#endif

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a string that represents the current code source object.
        /// </summary>
        /// <returns>A string that represents the current code source object.</returns>
        /// =================================================================================================
        public override string ToString()
            => string.Format("{0} ({1})", Name, FullName);
    }
}