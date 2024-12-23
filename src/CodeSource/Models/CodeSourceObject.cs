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
        public IEnumerable<CodeSourceObjectHistory> History { get; set; }
    }
}