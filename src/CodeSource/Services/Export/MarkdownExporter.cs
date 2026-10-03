// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="MarkdownExporter.cs" company="RzR SOFT & TECH">
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
using System.Linq;
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
    public sealed class MarkdownExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = ExportFormats.Markdown;

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            const string breakLine = "---";
            const string nl = "<br />";
            // {0} receives a complete code span (fence included), see ToMarkdownCodeSpan.
            const string name = "Name: {0}";
            const string fullName = "FullName: {0}";
            const string history = "##### Code History: ";
            const string tblHeader =
                "| CodePath | URL | Author | Copyright | AppliedOn | Comment | Version | Tags | WorkItemId |";
            const string tblHeaderTab =
                "|----------|-----|--------|-----------|-----------|---------|---------|------|------------|";
            const string emptyRow = "|  |  |  |  |  |  |  |  |  |";

            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            try
            {
                using (var sw = new StreamWriter(new NonClosingStream(outputStream), Encoding.UTF8))
                {
                    for (var i = 1; i <= items.Count(); i++)
                    {
                        var it = items.ElementAt(i - 1);
                        var parent = it.Parent;
                        sw.WriteLine(string.Empty);
                        sw.WriteLine($"> #### {i}. Parent (Class)");
                        sw.Write(">>");
                        sw.Write(name, parent.Name.ToMarkdownCodeSpan());
                        sw.Write(nl);
                        sw.Write(fullName, parent.FullName.ToMarkdownCodeSpan());
                        sw.WriteLine(string.Empty);
                        sw.WriteLine(string.Empty);

                        sw.WriteLine(history);
                        sw.WriteLine(string.Empty);
                        sw.WriteLine(tblHeader);
                        sw.WriteLine(tblHeaderTab);
                        if (parent.History.HasAnyData())
                        {
                            foreach (var h in parent.History) sw.WriteLine(FormatHistoryRow(h));
                        }
                        else
                        {
                            sw.WriteLine(emptyRow);
                            sw.WriteLine(string.Empty);
                        }

                        var children = it.Children ?? new List<CodeSourceObject>();
                        if (children.HasAnyData())
                            for (var j = 1; j <= children.Count(); j++)
                            {
                                var c = it.Children.ElementAt(j - 1);
                                sw.WriteLine(string.Empty);
                                sw.WriteLine($"> {i}.{j} Child(Method)");
                                sw.Write(nl);
                                sw.Write(name, c.Name.ToMarkdownCodeSpan());
                                sw.Write(nl);
                                sw.Write(fullName, c.FullName.ToMarkdownCodeSpan());
                                sw.WriteLine(string.Empty);
                                sw.WriteLine(string.Empty);

                                sw.WriteLine(history);
                                sw.WriteLine(string.Empty);
                                sw.WriteLine(tblHeader);
                                sw.WriteLine(tblHeaderTab);
                                if (c.History.HasAnyData())
                                {
                                    foreach (var h in c.History) sw.WriteLine(FormatHistoryRow(h));
                                }
                                else
                                {
                                    sw.WriteLine(emptyRow);
                                    sw.WriteLine(string.Empty);
                                }
                            }

                        sw.WriteLine(string.Empty);
                        sw.WriteLine(breakLine);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CodeSourceExporterException(Format, ex);
            }
        }

        /// <summary>
        ///     Formats a code history item as a Markdown table row; every data value is escaped for a
        ///     table cell and the source URL is rendered as a link only for http/https.
        /// </summary>
        /// <param name="h">The code history item.</param>
        /// <returns>
        ///     The formatted table row.
        /// </returns>
        private static string FormatHistoryRow(CodeSourceObjectHistory h)
        {
            return $"| {h.CodePath.ToMarkdownTableCell()} " +
                   $"| {h.SourceUrl.ToMarkdownLinkCell()} " +
                   $"| {h.AuthorName.ToMarkdownTableCell()} " +
                   $"| {h.Copyright.ToMarkdownTableCell()} " +
                   $"| {h.AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} " +
                   $"| {h.Comment.ToMarkdownTableCell()} " +
                   $"| {h.Version.ToMarkdownTableCell()} " +
                   $"| {h.Tags.ToMarkdownTableCell()} " +
                   $"| {h.RelatedTaskId.ToMarkdownTableCell()} |";
        }
    }
}