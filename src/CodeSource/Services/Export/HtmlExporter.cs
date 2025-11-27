// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
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

using CodeSource.Abstractions;
using CodeSource.Extensions.Internal;
using CodeSource.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeSource.Exceptions;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter" />
    public sealed class HtmlExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = "HTML";

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
                        sw.WriteLine(headTab, parent.Name, parent.FullName);
                        sw.WriteLine(historyTab);

                        if (parent.History.HasAnyData())
                        {
                            foreach (var h in parent.History)
                            {
                                sw.WriteLine(@$"
                                    <tr>
                                        <td>{h.CodePath.IfIsNullThenEmpty()}</td>
                                        <td>{h.SourceUrl.IfIsNullThenEmpty()}</td>
                                        <td>{h.AuthorName.IfIsNullThenEmpty()}</td>
                                        <td>{h.Copyright.IfIsNullThenEmpty()}</td>
                                        <td>{h.AppliedOn?.ToString("yyyy-MM-dd")}</td>
                                        <td>{h.Comment.IfIsNullThenEmpty()}</td>
                                        <td>{$"{h.Version:##.0##}"}</td>
                                        <td>{h.Tags.IfIsNullThenEmpty()}</td>
                                        <td>{h.RelatedTaskId.IfIsNullThenEmpty()}</td>
                                    </tr>");
                            }
                        }
                        var children = (it.Children ?? new List<CodeSourceObject>());
                        if (children.HasAnyData())
                        {
                            foreach (var c in children)
                            {
                                sw.WriteLine(emptyTab);
                                sw.WriteLine(headTab, c.Name, c.FullName);
                                sw.WriteLine(historyTab);

                                if (c.History.HasAnyData())
                                {
                                    foreach (var h in c.History)
                                    {
                                        sw.WriteLine(@$"
                                            <tr>
                                                <td>{h.CodePath.IfIsNullThenEmpty()}</td>
                                                <td>{h.SourceUrl.IfIsNullThenEmpty()}</td>
                                                <td>{h.AuthorName.IfIsNullThenEmpty()}</td>
                                                <td>{h.Copyright.IfIsNullThenEmpty()}</td>
                                                <td>{h.AppliedOn?.ToString("yyyy-MM-dd")}</td>
                                                <td>{h.Comment.IfIsNullThenEmpty()}</td>
                                                <td>{$"{h.Version:##.0##}"}</td>
                                                <td>{h.Tags.IfIsNullThenEmpty()}</td>
                                                <td>{h.RelatedTaskId.IfIsNullThenEmpty()}</td>
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
            catch
            {
                throw new CodeSourceExporterException(Format);
            }
        }
    }
}