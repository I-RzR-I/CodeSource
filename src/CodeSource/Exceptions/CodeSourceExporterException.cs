// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceExporterException.cs" company="RzR SOFT & TECH">
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
    ///     Exception for signalling code source exporter errors.
    /// </summary>
    /// <seealso cref="T:Exception" />
    public class CodeSourceExporterException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceExporterException" /> class.
        /// </summary>
        /// <param name="exporterFormat">The exporter format.</param>
        public CodeSourceExporterException(string exporterFormat)
            : base(FormatMessage(exporterFormat))
        {
            ExporterFormat = exporterFormat;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceExporterException" /> class.
        /// </summary>
        /// <param name="exporterFormat">The exporter format.</param>
        /// <param name="innerException">The inner exception.</param>
        public CodeSourceExporterException(string exporterFormat, Exception innerException)
            : base(FormatMessage(exporterFormat), innerException)
        {
            ExporterFormat = exporterFormat;
        }

        /// <summary>
        ///     Gets the exporter format.
        /// </summary>
        /// <value>
        ///     The exporter format.
        /// </value>
        public string ExporterFormat { get; }

        /// <summary>
        ///     Format message.
        /// </summary>
        /// <param name="exporterFormat">The exporter format.</param>
        /// <returns>
        ///     The formatted message.
        /// </returns>
        private static string FormatMessage(string exporterFormat)
        {
            return $"Unexpected error occurred while trying to export code history in the format '{exporterFormat}'";
        }
    }
}