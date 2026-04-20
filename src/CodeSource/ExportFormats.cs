// ***********************************************************************
//  Assembly         : RzR.Core.CodeSource
//  Author           : RzR
//  Created On       : 2026-04-20 18:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-04-20 18:31
// ***********************************************************************
//  <copyright file="ExportFormats.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace RzR.Core.CodeSource
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Built-in export format constants. Use these instead of raw strings when calling
    ///     <see cref="Services.ExporterRegistry.Export(string, System.Collections.Generic.IEnumerable{Models.CodeSourceObjectsResult}, System.IO.Stream)" />     .
    /// </summary>
    /// =================================================================================================
    public static class ExportFormats
    {
        /// <summary>Comma-Separated Values.</summary>
        public const string Csv = "CSV";

        /// <summary>HyperText Markup Language.</summary>
        public const string Html = "HTML";

        /// <summary>JavaScript Object Notation.</summary>
        public const string Json = "JSON";

        /// <summary>Markdown.</summary>
        public const string Markdown = "MD";

        /// <summary>XML format.</summary>
        public const string Xml = "XML";

        /// <summary>YAML format.</summary>
        public const string Yaml = "YAML";
    }
}