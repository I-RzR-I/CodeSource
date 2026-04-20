// ***********************************************************************
//  Assembly         : RzR.Core.CodeSource
//  Author           : RzR
//  Created On       : 2024-12-23 16:20
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-12-23 19:46
// ***********************************************************************
//  <copyright file="CodeSourceObjectsResult.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace RzR.Core.CodeSource.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Encapsulates the result of a code source objects.
    /// </summary>
    /// =================================================================================================
    public class CodeSourceObjectsResult
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the parent.
        /// </summary>
        /// <value>
        ///     The parent.
        /// </value>
        /// =================================================================================================
        public CodeSourceObject Parent { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the children.
        /// </summary>
        /// <value>
        ///     The children.
        /// </value>
        /// =================================================================================================
#if NET45_OR_GREATER || NETSTANDARD || NET
        public IReadOnlyList<CodeSourceObject> Children { get; set; }
#else
        public IList<CodeSourceObject> Children { get; set; }
#endif
    }
}