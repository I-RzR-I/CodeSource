// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="ICodeSourceExporter.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Collections.Generic;
using System.IO;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Abstractions
{
    /// <summary>
    ///     Interface for code source exporter.
    /// </summary>
    public interface ICodeSourceExporter
    {
        /// <summary>
        ///     Gets the format to use.
        /// </summary>
        /// <value>
        ///     The format.
        /// </value>
        string Format { get; }

        /// <summary>
        ///     Exports.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="outputStream">The output stream.</param>
        void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream);
    }
}