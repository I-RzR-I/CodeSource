// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-20 19:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-20 19:39
// ***********************************************************************
//  <copyright file="ExporterRegistry.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using CodeSource.Abstractions;
using CodeSource.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeSource.Exceptions;
using CodeSource.Extensions.Internal;

#endregion

namespace CodeSource.Services
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An exporter registry.
    /// </summary>
    /// =================================================================================================
    public static class ExporterRegistry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the exporters.
        /// </summary>
        /// =================================================================================================
        private static readonly Dictionary<string, ICodeSourceExporter> Exporters;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes static members of the <see cref="ExporterRegistry"/> class.
        /// </summary>
        /// =================================================================================================
        static ExporterRegistry()
        {
            var exporterTypes = GetLocalTypes()
                .Where(x => typeof(ICodeSourceExporter).IsAssignableFromPortable(x)
                            && x != typeof(ICodeSourceExporter));
            Exporters = exporterTypes
                .Select(t => (ICodeSourceExporter)Activator.CreateInstance(t)!)
                .ToDictionary(e => e.Format, StringComparer.OrdinalIgnoreCase);
        }

# if !NETSTANDARD1_0
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Exports.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="items">The items.</param>
        /// <param name="savePath">Full pathname of the file.</param>
        /// =================================================================================================
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, string savePath)
        {
            if (!Exporters.TryGetValue(format, out var exporter))
                throw new CodeSourceUndefinedExportFormat(format);

            using var stream = new FileStream(savePath, FileMode.CreateNew);
            exporter.Export(items, stream);
        }
#endif

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Exports.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="items">The items.</param>
        /// <param name="stream">The stream.</param>
        /// =================================================================================================
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, Stream stream)
        {
            if (!Exporters.TryGetValue(format, out var exporter))
                throw new CodeSourceUndefinedExportFormat(format);

            exporter.Export(items, stream);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the local types in this collection.
        /// </summary>
        /// <param name="assemblyName">
        ///     (Optional) Name of the assembly.
        ///     Default value is 'CodeSource'/
        /// </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the local types in this
        ///     collection.
        /// </returns>
        /// =================================================================================================
        private static IEnumerable<Type> GetLocalTypes(string assemblyName = "CodeSource")
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));

            IEnumerable<Type> types;
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
            types = assembly.GetExportedTypes();
#elif NETSTANDARD1_0
            types = assembly.ExportedTypes;
#else
                    types = assembly.GetExportedTypes();
#endif

            return types;
        }
    }
}