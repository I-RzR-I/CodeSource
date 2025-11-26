// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-23 23:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-23 23:15
// ***********************************************************************
//  <copyright file="CodeSourceUndefinedExportFormat.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

#endregion

namespace CodeSource.Exceptions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A code source undefined export format.
    /// </summary>
    /// <seealso cref="T:Exception" />
    /// =================================================================================================
    public class CodeSourceUndefinedExportFormat : Exception
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the format to use.
        /// </summary>
        /// <value>
        ///     The format.
        /// </value>
        /// =================================================================================================
        public string Format { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceUndefinedExportFormat" /> class.
        /// </summary>
        /// <param name="format">Describes the format to use.</param>
        /// =================================================================================================
        public CodeSourceUndefinedExportFormat(string format)
            : base(FormatMessage(format))
        {
            Format = format;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Format message.
        /// </summary>
        /// <param name="format">Describes the format to use.</param>
        /// <returns>
        ///     The formatted message.
        /// </returns>
        /// =================================================================================================
        private static string FormatMessage(string format) => $"Missing exporter for '{format}'";
    }
}