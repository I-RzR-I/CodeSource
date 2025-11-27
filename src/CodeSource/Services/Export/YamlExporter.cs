// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-18 13:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-18 13:15
// ***********************************************************************
//  <copyright file="YamlExporter.cs" company="RzR SOFT & TECH">
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
    public sealed class YamlExporter : ICodeSourceExporter
    {
        /// <inheritdoc />
        public string Format { get; } = "YAML";

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            try
            {
                using (var sw = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    sw.WriteLine("codeSources:");
                    foreach (var it in items)
                    {
                        var parent = it.Parent;

                        sw.WriteLine("  - fullName: " + parent.FullName);
                        sw.WriteLine("    name: " + parent.Name);
                        
                        if (parent.History.HasAnyData())
                        {
                            sw.WriteLine("    history: " );
                            foreach (var h in parent.History)
                            {
                                sw.WriteLine("      - codePath: " + $"\"{h.CodePath.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        sourceUrl: " + $"\"{h.SourceUrl.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        authorName: " + $"\"{h.AuthorName.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        copyright: " + $"\"{h.Copyright.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        appliedOn: " + $"\"{h.AppliedOn?.ToString("yyyy-MM-dd")}\"");
                                sw.WriteLine("        comment: " + $"\"{h.Comment.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        version: " + $"\"{h.Version:##.0##}\"");
                                sw.WriteLine("        tags: " + $"\"{h.Tags.IfIsNullThenEmpty()}\"");
                                sw.WriteLine("        relatedTaskId: " + $"\"{h.RelatedTaskId.IfIsNullThenEmpty()}\"");
                            }
                        }

                        var children = (it.Children ?? new List<CodeSourceObject>());
                        if (children.HasAnyData())
                        {
                            sw.WriteLine("");
                            foreach (var child in children)
                            {
                                sw.WriteLine("  - fullName: " + child.FullName);
                                sw.WriteLine("    name: " + child.Name);
                                if (child.History.HasAnyData())
                                {
                                    sw.WriteLine("    history: ");
                                    foreach (var h in child.History)
                                    {
                                        sw.WriteLine("      - codePath: " + $"\"{h.CodePath.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        sourceUrl: " + $"\"{h.SourceUrl.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        authorName: " + $"\"{h.AuthorName.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        copyright: " + $"\"{h.Copyright.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        appliedOn: " + $"\"{h.AppliedOn?.ToString("yyyy-MM-dd")}\"");
                                        sw.WriteLine("        comment: " + $"\"{h.Comment.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        version: " + $"\"{h.Version:##.0##}\"");
                                        sw.WriteLine("        tags: " + $"\"{h.Tags.IfIsNullThenEmpty()}\"");
                                        sw.WriteLine("        relatedTaskId: " + $"\"{h.RelatedTaskId.IfIsNullThenEmpty()}\"");
                                    }
                                }
                            }
                        }
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