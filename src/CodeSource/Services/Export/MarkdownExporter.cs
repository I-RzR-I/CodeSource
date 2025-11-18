// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-17 21:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 07:31
// ***********************************************************************
//  <copyright file="MarkdownExporter.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeSource.Abstractions;
using CodeSource.Extensions.Internal;
using CodeSource.Models;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter"/>
    public class MarkdownExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; private set; } = "MD";

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            const string breakLine = "---";
            const string nl = "<br />";
            const string name = "Name: `{0}`";
            const string fullName = "FullName: `{0}`";
            const string history = "##### Code History: ";
            const string tblHeader =
                "| CodePath | URL | Author | Copyright | AppliedOn | Comment | Version | Tags | WorkItemId |";
            const string tblHeaderTab =
                "|----------|-----|--------|-----------|-----------|---------|---------|------|------------|";
            const string emptyRow = "|  |  |  |  |  |  |  |  |  |";

            try
            {
                using (var sw = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    for (var i = 1; i <= items.Count(); i++)
                    {
                        var it = items.ElementAt(i - 1);
                        var parent = it.Parent;
                        sw.WriteLine(string.Empty);
                        sw.WriteLine($"> #### {i}. Parent (Class)");
                        sw.Write(">>");
                        sw.Write(name, parent.Name.IfIsNullThenEmpty());
                        sw.Write(nl);
                        sw.Write(fullName, parent.FullName.IfIsNullThenEmpty());
                        sw.WriteLine(string.Empty);
                        sw.WriteLine(string.Empty);

                        sw.WriteLine(history);
                        sw.WriteLine(string.Empty);
                        sw.WriteLine(tblHeader);
                        sw.WriteLine(tblHeaderTab);
                        if (parent.History.Any())
                        {
                            foreach (var h in parent.History)
                            {
                                sw.WriteLine($"| {h.CodePath.IfIsNullThenEmpty()} " +
                                             $"| [{h.SourceUrl.IfIsNullThenEmpty()}]({h.SourceUrl.IfIsNullThenEmpty()}) " +
                                             $"| {h.AuthorName.IfIsNullThenEmpty()} " +
                                             $"| {h.Copyright.IfIsNullThenEmpty()} " +
                                             $"| {h.AppliedOn?.ToString("yyyy-MM-dd")} " +
                                             $"| {h.Comment.IfIsNullThenEmpty()} " +
                                             $"| {h.Version:##.0##} " +
                                             $"| {h.Tags.IfIsNullThenEmpty()} " +
                                             $"| {h.RelatedTaskId.IfIsNullThenEmpty()} |");
                            }
                        }
                        else
                        {
                            sw.WriteLine(emptyRow);
                            sw.WriteLine(string.Empty);
                        }

                        var children = (it.Children ?? new List<CodeSourceObject>());
                        if (children.HasAnyData())
                        {
                            for (var j = 1; j <= it.Children.Count(); j++)
                            {
                                var c = it.Children.ElementAt(j - 1);
                                sw.WriteLine(string.Empty);
                                sw.WriteLine($"> {i}.{j} Child(Method)");
                                sw.Write(nl);
                                sw.Write(name, c.Name.IfIsNullThenEmpty());
                                sw.Write(nl);
                                sw.Write(fullName, c.FullName.IfIsNullThenEmpty());
                                sw.WriteLine(string.Empty);
                                sw.WriteLine(string.Empty);

                                sw.WriteLine(history);
                                sw.WriteLine(string.Empty);
                                sw.WriteLine(tblHeader);
                                sw.WriteLine(tblHeaderTab);
                                if (c.History.HasAnyData())
                                {
                                    foreach (var h in c.History)
                                    {
                                        sw.WriteLine($"| {h.CodePath.IfIsNullThenEmpty()} " +
                                                     $"| [{h.SourceUrl.IfIsNullThenEmpty()}]({h.SourceUrl.IfIsNullThenEmpty()}) " +
                                                     $"| {h.AuthorName.IfIsNullThenEmpty()} " +
                                                     $"| {h.Copyright.IfIsNullThenEmpty()} " +
                                                     $"| {h.AppliedOn?.ToString("yyyy-MM-dd")} " +
                                                     $"| {h.Comment.IfIsNullThenEmpty()} " +
                                                     $"| {h.Version:##.0##} " +
                                                     $"| {h.Tags.IfIsNullThenEmpty()} " +
                                                     $"| {h.RelatedTaskId.IfIsNullThenEmpty()} |");
                                    }
                                }
                                else
                                {
                                    sw.WriteLine(emptyRow);
                                    sw.WriteLine(string.Empty);
                                }
                            }
                        }

                        sw.WriteLine(string.Empty);
                        sw.WriteLine(breakLine);
                    }
                }
            }
            catch
            {
                /*ignored*/
            }
        }
    }
}