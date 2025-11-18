// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-17 21:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 07:32
// ***********************************************************************
//  <copyright file="ICodeSourceExporter.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.IO;
using CodeSource.Models;

#endregion

namespace CodeSource.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Interface for code source exporter.
    /// </summary>
    /// =================================================================================================
    public interface ICodeSourceExporter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the format to use.
        /// </summary>
        /// <value>
        ///     The format.
        /// </value>
        /// =================================================================================================
        string Format { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Exports.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="outputStream">The output stream.</param>
        /// =================================================================================================
        void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream);
    }
}