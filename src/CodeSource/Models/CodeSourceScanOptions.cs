// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceScanOptions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;

#endregion

namespace RzR.Core.CodeSource.Models
{
    /// <summary>
    ///     Options for an annotation scan.
    /// </summary>
    public sealed class CodeSourceScanOptions
    {
        /// <summary>
        ///     Gets or sets the callback invoked for every recoverable failure met during the scan.
        /// </summary>
        /// <value>
        ///     The error callback, or null to skip failures silently.
        /// </value>
        public Action<CodeSourceScanError> OnError { get; set; }
    }
}