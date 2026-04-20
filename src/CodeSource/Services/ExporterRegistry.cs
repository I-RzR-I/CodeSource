// ***********************************************************************
//  Assembly         : RzR.Core.CodeSource
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Exceptions;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Services
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
        ///     (Immutable) the synchronization lock.
        /// </summary>
        /// =================================================================================================
        private static readonly object SyncLock = new object();

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a custom exporter. If an exporter for the same format already exists, it will
        ///     be replaced.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="exporter"/> is null.
        /// </exception>
        /// <param name="exporter">The exporter to register.</param>
        /// =================================================================================================
        public static void Register(ICodeSourceExporter exporter)
        {
            if (exporter.IsNull())
                throw new ArgumentNullException(nameof(exporter));

            lock (SyncLock)
            {
                Exporters[exporter.Format] = exporter;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Removes a registered exporter by format name.
        /// </summary>
        /// <param name="format">The format name to unregister (case-insensitive).</param>
        /// <returns>
        ///     True if the exporter was found and removed, false otherwise.
        /// </returns>
        /// =================================================================================================
        public static bool Unregister(string format)
        {
            if (format.IsMissing())
                throw new ArgumentNullException(nameof(format));

            lock (SyncLock)
            {
                return Exporters.Remove(format);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the registered format names.
        /// </summary>
        /// <returns>
        ///     A registered format names enumerable.
        /// </returns>
        /// =================================================================================================
        public static IEnumerable<string> GetRegisteredFormats()
        {
            lock (SyncLock)
            {
                return Exporters.Keys.ToArray();
            }
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
            ICodeSourceExporter exporter;
            lock (SyncLock)
            {
                if (!Exporters.TryGetValue(format, out exporter))
                    throw new CodeSourceUndefinedExportFormat(format);
            }

            using var stream = new FileStream(savePath, FileMode.Create);
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
            ICodeSourceExporter exporter;
            lock (SyncLock)
            {
                if (!Exporters.TryGetValue(format, out exporter))
                    throw new CodeSourceUndefinedExportFormat(format);
            }

            exporter.Export(items, stream);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the local types in this collection.
        /// </summary>
        /// <param name="assemblyName">
        ///     (Optional) Name of the assembly.
        ///     Default value is 'CodeSource(RzR.Core.CodeSource)'/
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