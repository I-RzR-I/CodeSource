// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CsvExporter.cs" company="RzR SOFT & TECH">
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
    public sealed class CsvExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = ExportFormats.Csv;

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            const string tblHeader =
                "CodePath,URL,Author,Copyright,AppliedOn,Comment,Version,Tags,WorkItemId,IsParent,IsHistory";
            const string emptyTab = ",,,,,,,,,,";

            if (items.IsNull())
                throw new ArgumentNullException(nameof(items));
            if (outputStream.IsNull())
                throw new ArgumentNullException(nameof(outputStream));

            try
            {
                using (var sw = new StreamWriter(new NonClosingStream(outputStream), Encoding.UTF8))
                {
                    sw.WriteLine(tblHeader);

                    foreach (var it in items)
                    {
                        var parent = it.Parent;
                        sw.WriteLine(emptyTab);
                        sw.WriteLine(string.Join(",",
                            EscapeData(parent.FullName),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape(string.Empty),
                            Escape("1"),
                            Escape("0")
                        ));

                        if (parent.History.HasAnyData())
                            foreach (var h in parent.History)
                                sw.WriteLine(string.Join(",",
                                    EscapeData(h.CodePath),
                                    EscapeData(h.SourceUrl),
                                    EscapeData(h.AuthorName),
                                    EscapeData(h.Copyright),
                                    EscapeData(h.AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                                    EscapeData(h.Comment),
                                    EscapeData(h.Version),
                                    EscapeData(h.Tags),
                                    EscapeData(h.RelatedTaskId),
                                    Escape("1"),
                                    Escape("1")
                                ));

                        var children = it.Children ?? new List<CodeSourceObject>();
                        if (children.HasAnyData())
                            foreach (var c in children)
                            {
                                sw.WriteLine(emptyTab);
                                sw.WriteLine(string.Join(",",
                                    EscapeData(c.FullName),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape(string.Empty),
                                    Escape("0"),
                                    Escape("0")
                                ));

                                if (c.History.HasAnyData())
                                    foreach (var h in c.History)
                                        sw.WriteLine(string.Join(",",
                                            EscapeData(h.CodePath),
                                            EscapeData(h.SourceUrl),
                                            EscapeData(h.AuthorName),
                                            EscapeData(h.Copyright),
                                            EscapeData(
                                                h.AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                                            EscapeData(h.Comment),
                                            EscapeData(h.Version),
                                            EscapeData(h.Tags),
                                            EscapeData(h.RelatedTaskId),
                                            Escape("0"),
                                            Escape("1")
                                        ));
                            }

                        continue;

                        // Fixed flag and placeholder cells: quoted only, never formula-neutralised.
                        string Escape(string v)
                        {
                            return v.ToCsvField();
                        }

                        // Data-derived cells: trimmed, formula-neutralised, then quoted.
                        string EscapeData(string v)
                        {
                            return v.ToCsvDataField();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CodeSourceExporterException(Format, ex);
            }
        }
    }
}