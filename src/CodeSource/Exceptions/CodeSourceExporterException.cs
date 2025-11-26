// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-24 08:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-24 08:17
// ***********************************************************************
//  <copyright file="CodeSourceExporterException.cs" company="RzR SOFT & TECH">
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
    ///     Exception for signalling code source exporter errors.
    /// </summary>
    /// <seealso cref="T:Exception"/>
    /// =================================================================================================
    public class CodeSourceExporterException : Exception
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the exporter format.
        /// </summary>
        /// <value>
        ///     The exporter format.
        /// </value>
        /// =================================================================================================
        public string ExporterFormat { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceExporterException"/> class.
        /// </summary>
        /// <param name="exporterFormat">The exporter format.</param>
        /// =================================================================================================
        public CodeSourceExporterException(string exporterFormat) 
            : base(FormatMessage(exporterFormat))
        {
            ExporterFormat = exporterFormat;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Format message.
        /// </summary>
        /// <param name="exporterFormat">The exporter format.</param>
        /// <returns>
        ///     The formatted message.
        /// </returns>
        /// =================================================================================================
        private static string FormatMessage(string exporterFormat)
        {
            return $"Unexpected error occurred while trying to export code history in the format '{exporterFormat}'";
        }
    }
}