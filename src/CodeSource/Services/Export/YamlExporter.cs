// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:22
//  ***********************************************************************
//  <copyright file="YamlExporter.cs" company="RzR SOFT & TECH">
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
using System.Globalization;
using System.IO;
using System.Text;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Exceptions;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Models;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace RzR.Core.CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter" />
    public sealed class YamlExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = ExportFormats.Yaml;

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            try
            {
                using (var sw = new StreamWriter(new NonClosingStream(outputStream), Encoding.UTF8))
                {
                    sw.WriteLine("codeSources:");
                    foreach (var it in items)
                    {
                        var parent = it.Parent;

                        sw.WriteLine("  - fullName: " + parent.FullName.IfIsNullThenEmpty().ToYamlDoubleQuoted());
                        sw.WriteLine("    name: " + parent.Name.IfIsNullThenEmpty().ToYamlDoubleQuoted());

                        if (parent.History.HasAnyData())
                        {
                            sw.WriteLine("    history: ");
                            foreach (var h in parent.History) WriteHistoryItem(sw, h);
                        }

                        var children = it.Children ?? new List<CodeSourceObject>();
                        if (children.HasAnyData())
                        {
                            sw.WriteLine("");
                            foreach (var child in children)
                            {
                                sw.WriteLine("  - fullName: " +
                                             child.FullName.IfIsNullThenEmpty().ToYamlDoubleQuoted());
                                sw.WriteLine("    name: " + child.Name.IfIsNullThenEmpty().ToYamlDoubleQuoted());
                                if (child.History.HasAnyData())
                                {
                                    sw.WriteLine("    history: ");
                                    foreach (var h in child.History) WriteHistoryItem(sw, h);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CodeSourceExporterException(Format, ex);
            }
        }

        /// <summary>
        ///     Writes a code history item as a YAML sequence entry; every value is emitted as an escaped
        ///     double-quoted scalar and every key is a constant.
        /// </summary>
        /// <param name="sw">The StreamWriter.</param>
        /// <param name="h">The code history item.</param>
        private static void WriteHistoryItem(StreamWriter sw, CodeSourceObjectHistory h)
        {
            sw.WriteLine("      - codePath: " + h.CodePath.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        sourceUrl: " + h.SourceUrl.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        authorName: " + h.AuthorName.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        copyright: " + h.Copyright.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            // Parenthesised so a null date still reaches the escaper and is emitted as "".
            sw.WriteLine("        appliedOn: " +
                         (h.AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).ToYamlDoubleQuoted());
            sw.WriteLine("        comment: " + h.Comment.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        version: " + h.Version.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        tags: " + h.Tags.IfIsNullThenEmpty().ToYamlDoubleQuoted());
            sw.WriteLine("        relatedTaskId: " + h.RelatedTaskId.IfIsNullThenEmpty().ToYamlDoubleQuoted());
        }
    }
}