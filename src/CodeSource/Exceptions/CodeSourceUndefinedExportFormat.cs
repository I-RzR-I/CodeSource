// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceUndefinedExportFormat.cs" company="RzR SOFT & TECH">
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

namespace RzR.Core.CodeSource.Exceptions
{
    /// <summary>
    ///     A code source undefined export format.
    /// </summary>
    /// <seealso cref="T:Exception" />
    public class CodeSourceUndefinedExportFormat : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceUndefinedExportFormat" /> class.
        /// </summary>
        /// <param name="format">Describes the format to use.</param>
        public CodeSourceUndefinedExportFormat(string format)
            : base(FormatMessage(format))
        {
            Format = format;
        }

        /// <summary>
        ///     Gets the format to use.
        /// </summary>
        /// <value>
        ///     The format.
        /// </value>
        public string Format { get; }

        /// <summary>
        ///     Format message.
        /// </summary>
        /// <param name="format">Describes the format to use.</param>
        /// <returns>
        ///     The formatted message.
        /// </returns>
        private static string FormatMessage(string format)
        {
            return $"Missing exporter for '{format}'";
        }
    }
}