// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:22
//  ***********************************************************************
//  <copyright file="ExporterRegistry.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Exceptions;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services.Export;

#endregion

namespace RzR.Core.CodeSource.Services
{
    /// <summary>
    ///     An exporter registry.
    /// </summary>
    public static class ExporterRegistry
    {
        /// <summary>
        ///     (Immutable) the exporters.
        /// </summary>
        private static readonly Dictionary<string, ICodeSourceExporter> Exporters;

        /// <summary>
        ///     (Immutable) the synchronization lock.
        /// </summary>
        private static readonly object SyncLock = new();

        /// <summary>
        ///     Initializes static members of the <see cref="ExporterRegistry" /> class.
        /// </summary>
        /// <remarks>
        ///     The six built-in exporters are registered explicitly, with no assembly loading or type
        ///     discovery. Format names are case-insensitive. If two exporters share a format, the last
        ///     one registered wins.
        /// </remarks>
        static ExporterRegistry()
        {
            Exporters = new Dictionary<string, ICodeSourceExporter>(StringComparer.OrdinalIgnoreCase);

            var builtInExporters = new ICodeSourceExporter[]
            {
                new CsvExporter(),
                new HtmlExporter(),
                new JsonExporter(),
                new MarkdownExporter(),
                new XmlExporter(),
                new YamlExporter()
            };

            foreach (var exporter in builtInExporters)
                Exporters[exporter.Format] = exporter;
        }

        /// <summary>
        ///     Registers a custom exporter. If an exporter for the same format already exists, it will
        ///     be replaced.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="exporter" /> is null.
        /// </exception>
        /// <param name="exporter">The exporter to register.</param>
        public static void Register(ICodeSourceExporter exporter)
        {
            if (exporter.IsNull())
                throw new ArgumentNullException(nameof(exporter));

            lock (SyncLock)
            {
                Exporters[exporter.Format] = exporter;
            }
        }

        /// <summary>
        ///     Removes a registered exporter by format name.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="format">The format name to unregister (case-insensitive).</param>
        /// <returns>
        ///     True if the exporter was found and removed, false otherwise.
        /// </returns>
        public static bool Unregister(string format)
        {
            if (format.IsMissing())
                throw new ArgumentNullException(nameof(format));

            lock (SyncLock)
            {
                return Exporters.Remove(format);
            }
        }

        /// <summary>
        ///     Gets the registered format names.
        /// </summary>
        /// <returns>
        ///     A registered format names enumerable.
        /// </returns>
        public static IEnumerable<string> GetRegisteredFormats()
        {
            lock (SyncLock)
            {
                return Exporters.Keys.ToArray();
            }
        }

# if !NETSTANDARD1_0

        /// <summary>
        ///     Exports.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" />, <paramref name="items" /> or
        ///     <paramref name="savePath" /> is null.
        /// </exception>
        /// <exception cref="CodeSourceUndefinedExportFormat">
        ///     Thrown when no exporter is registered for <paramref name="format" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="savePath" /> is empty or whitespace.
        /// </exception>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="items">The items.</param>
        /// <param name="savePath">Full pathname of the file.</param>
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, string savePath)
        {
            if (format.IsNull())
                throw new ArgumentNullException(nameof(format));

            ICodeSourceExporter exporter;
            lock (SyncLock)
            {
                if (!Exporters.TryGetValue(format, out exporter))
                    throw new CodeSourceUndefinedExportFormat(format);
            }

            if (items.IsNull())
                throw new ArgumentNullException(nameof(items));

            if (savePath.IsNull())
                throw new ArgumentNullException(nameof(savePath));

            if (savePath.IsMissing())
                throw new ArgumentException("The save path must not be empty or whitespace.", nameof(savePath));

            using var stream = new FileStream(savePath, FileMode.Create);
            exporter.Export(items, stream);
        }
#endif

        /// <summary>
        ///     Exports.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is null.
        /// </exception>
        /// <exception cref="CodeSourceUndefinedExportFormat">
        ///     Thrown when no exporter is registered for <paramref name="format" />.
        /// </exception>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="items">The items.</param>
        /// <param name="stream">The stream.</param>
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, Stream stream)
        {
            if (format.IsNull())
                throw new ArgumentNullException(nameof(format));

            ICodeSourceExporter exporter;
            lock (SyncLock)
            {
                if (!Exporters.TryGetValue(format, out exporter))
                    throw new CodeSourceUndefinedExportFormat(format);
            }

            exporter.Export(items, stream);
        }
    }
}