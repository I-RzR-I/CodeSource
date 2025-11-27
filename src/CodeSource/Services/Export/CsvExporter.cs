// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-18 09:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 09:45
// ***********************************************************************
//  <copyright file="CsvExporter.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeSource.Abstractions;
using CodeSource.Exceptions;
using CodeSource.Extensions.Internal;
using CodeSource.Models;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter" />
    public sealed class CsvExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = "CSV";

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            const string tblHeader =
                "CodePath,URL,Author,Copyright,AppliedOn,Comment,Version,Tags,WorkItemId,IsParent,IsHistory";
            const string emptyTab = ",,,,,,,,,,";
            try
            {
                using (var sw = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    sw.WriteLine(tblHeader);

                    foreach (var it in items)
                    {
                        var parent = it.Parent;
                        sw.WriteLine(emptyTab);
                        sw.WriteLine(string.Join(",",
                            Escape(parent.FullName.IfIsNullThenEmpty()),
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
                        {
                            foreach (var h in parent.History)
                            {
                                sw.WriteLine(string.Join(",",
                                    Escape(h.CodePath.IfIsNullThenEmpty()),
                                    Escape(h.SourceUrl.IfIsNullThenEmpty()),
                                    Escape(h.AuthorName.IfIsNullThenEmpty()),
                                    Escape(h.Copyright.IfIsNullThenEmpty()),
                                    Escape(h.AppliedOn?.ToString("yyyy-MM-dd")),
                                    Escape(h.Comment.IfIsNullThenEmpty()),
                                    Escape($"{h.Version:##.0##}"),
                                    Escape(h.Tags.IfIsNullThenEmpty()),
                                    Escape(h.RelatedTaskId.IfIsNullThenEmpty()),
                                    Escape("1"),
                                    Escape("1")
                                ));
                            }
                        }

                        var children = it.Children ?? new List<CodeSourceObject>();
                        if (children.HasAnyData())
                        {
                            foreach (var c in children)
                            {
                                sw.WriteLine(emptyTab);
                                sw.WriteLine(string.Join(",",
                                    Escape(c.FullName.IfIsNullThenEmpty()),
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
                                {
                                    foreach (var h in c.History)
                                    {
                                        sw.WriteLine(string.Join(",",
                                            Escape(h.CodePath.IfIsNullThenEmpty()),
                                            Escape(h.SourceUrl.IfIsNullThenEmpty()),
                                            Escape(h.AuthorName.IfIsNullThenEmpty()),
                                            Escape(h.Copyright.IfIsNullThenEmpty()),
                                            Escape(h.AppliedOn?.ToString("yyyy-MM-dd")),
                                            Escape(h.Comment.IfIsNullThenEmpty()),
                                            Escape($"{h.Version:##.0##}"),
                                            Escape(h.Tags.IfIsNullThenEmpty()),
                                            Escape(h.RelatedTaskId.IfIsNullThenEmpty()),
                                            Escape("0"),
                                            Escape("1")
                                        ));
                                    }
                                }
                            }

                        }

                        continue;

                        string Escape(string v) => $"\"{v.IfIsNullThenEmpty().Replace("\"", "\"\"")}\"";
                    }
                }
            }
            catch
            {
                throw new CodeSourceExporterException(Format);
            }
        }
    }
}