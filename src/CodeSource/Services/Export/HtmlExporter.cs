// ***********************************************************************
//  Assembly         : RzR.Core.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-18 12:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 12:05
// ***********************************************************************
//  <copyright file="HtmlExporter.cs" company="RzR SOFT & TECH">
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
using System.Net;
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
    public sealed class HtmlExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = ExportFormats.Html;

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            try
            {
                const string emptyTab = "<tr style=\"background-color: #96D4D4 !important;\"><td colspan=\"9\"></td></tr>";
                const string headTab = "<tr><td></td><td colspan=\"8\"><i>Name:</i> <b>{0}</b>; <i>FullName:</i> <b>{1}</b></td></tr>";
                const string historyTab = "<tr><td colspan=\"9\"><i>Code History:</i></td></tr>";
                using (var sw = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    sw.WriteLine("<html>");

                    sw.WriteLine("<header>");
                    sw.WriteLine("<style>table, th, td {\r\n  border: 1px solid black;\r\n  border-collapse: collapse;\r\n}</style>");
                    sw.WriteLine("</header>");

                    sw.WriteLine("<body>");

                    sw.WriteLine("<table>");
                    sw.WriteLine(@"
                        <tr>
                            <th>CodePath</th>
                            <th>URL</th>
                            <th>Author</th>
                            <th>Copyright</th>
                            <th>AppliedOn</th>
                            <th>Comment</th>
                            <th>Version</th>
                            <th>Tags</th>
                            <th>WorkItemId</th>
                        </tr>");

                    foreach (var it in items)
                    {
                        var parent = it.Parent;
                        sw.WriteLine(emptyTab);
                        sw.WriteLine(headTab, WebUtility.HtmlEncode(parent.Name), WebUtility.HtmlEncode(parent.FullName));
                        sw.WriteLine(historyTab);

                        if (parent.History.HasAnyData())
                        {
                            foreach (var h in parent.History)
                            {
                                sw.WriteLine(@$"
                                    <tr>
                                        <td>{WebUtility.HtmlEncode(h.CodePath.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.SourceUrl.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.AuthorName.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.Copyright.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.AppliedOn?.ToString("yyyy-MM-dd"))}</td>
                                        <td>{WebUtility.HtmlEncode(h.Comment.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.Version.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.Tags.IfIsNullThenEmpty())}</td>
                                        <td>{WebUtility.HtmlEncode(h.RelatedTaskId.IfIsNullThenEmpty())}</td>
                                    </tr>");
                            }
                        }
                        var children = (it.Children ?? new List<CodeSourceObject>());
                        if (children.HasAnyData())
                        {
                            foreach (var c in children)
                            {
                                sw.WriteLine(emptyTab);
                                sw.WriteLine(headTab, WebUtility.HtmlEncode(c.Name), WebUtility.HtmlEncode(c.FullName));
                                sw.WriteLine(historyTab);

                                if (c.History.HasAnyData())
                                {
                                    foreach (var h in c.History)
                                    {
                                        sw.WriteLine(@$"
                                            <tr>
                                                <td>{WebUtility.HtmlEncode(h.CodePath.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.SourceUrl.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.AuthorName.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.Copyright.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.AppliedOn?.ToString("yyyy-MM-dd"))}</td>
                                                <td>{WebUtility.HtmlEncode(h.Comment.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.Version.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.Tags.IfIsNullThenEmpty())}</td>
                                                <td>{WebUtility.HtmlEncode(h.RelatedTaskId.IfIsNullThenEmpty())}</td>
                                            </tr>");
                                    }
                                }
                            }
                        }
                        else
                        {
                            sw.WriteLine(emptyTab);
                        }
                    }
                    sw.WriteLine("</table>");

                    sw.WriteLine("</body>");

                    sw.WriteLine("</html>");
                }
            }
            catch (Exception ex)
            {
                throw new CodeSourceExporterException(Format, ex);
            }
        }
    }
}