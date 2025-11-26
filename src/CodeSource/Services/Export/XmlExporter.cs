// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-19 00:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-19 17:35
// ***********************************************************************
//  <copyright file="XmlExporter.cs" company="RzR SOFT & TECH">
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
using CodeSource.Extensions.Internal.Builder;
using CodeSource.Models;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter" />
    public sealed class XmlExporter : ICodeSourceExporter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the indent. 1 TAB value.
        /// </summary>
        /// =================================================================================================
        private const string Indent = "    ";

        /// <inheritdoc />
        public string Format { get; } = "XML";

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            try
            {
                using (var sw = new StreamWriter(outputStream, new UTF8Encoding(false)))
                {
                    sw.WriteXmlRoot("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteXmlOpenElement("codeSources", Indent.IndentMultiply(-1));

                    foreach (var item in items) 
                        WriteItem(sw, item);

                    sw.WriteXmlCloseElement("codeSources", Indent.IndentMultiply(-1));
                }
            }
            catch
            {
                throw new CodeSourceExporterException(Format);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Writes an item.
        /// </summary>
        /// <param name="sw">The StreamWriter.</param>
        /// <param name="item">The code source object result item.</param>
        /// =================================================================================================
        private static void WriteItem(StreamWriter sw, CodeSourceObjectsResult item)
        {
            if (item.IsNull()) 
                return;

            sw.WriteXmlOpenElement("codeSource", Indent.IndentMultiply());

            // Parent
            sw.WriteXmlOpenElement("parent", Indent.IndentMultiply(1));

            var parent = item.Parent;
            sw.WriteXmlElement(Indent.IndentMultiply(2), "name", parent.Name);
            sw.WriteXmlElement(Indent.IndentMultiply(2), "fullName", parent.FullName);

            sw.WriteXmlOpenElement("codeChanges", Indent.IndentMultiply(2));
            if (parent.History.HasAnyData())
            {
                foreach (var h in parent.History)
                {
                    sw.WriteXmlOpenElement("codeChange", Indent.IndentMultiply(3));

                    WriteXmlHistoryItem(sw, h, Indent.IndentMultiply(4));

                    sw.WriteXmlCloseElement("codeChange", Indent.IndentMultiply(3));
                }
            }

            sw.WriteXmlCloseElement("codeChanges", Indent.IndentMultiply(2));
            sw.WriteXmlCloseElement("parent", Indent.IndentMultiply(1));

            // Children
            var children = item.Children;
            sw.WriteXmlOpenElement("children", Indent.IndentMultiply(1));

            if (children.HasAnyData())
            {
                foreach (var child in children)
                {
                    sw.WriteXmlOpenElement("child", Indent.IndentMultiply(2));
                    sw.WriteXmlElement(Indent.IndentMultiply(3), "name", child.Name);
                    sw.WriteXmlElement(Indent.IndentMultiply(3), "fullName", child.FullName);
                    sw.WriteXmlOpenElement("codeChanges", Indent.IndentMultiply(3));

                    if (child.History.HasAnyData())
                    {
                        foreach (var h in child.History)
                        {
                            sw.WriteXmlOpenElement("codeChange", Indent.IndentMultiply(4));

                            WriteXmlHistoryItem(sw, h, Indent.IndentMultiply(5));

                            sw.WriteXmlCloseElement("codeChange", Indent.IndentMultiply(4));
                        }
                    }

                    sw.WriteXmlCloseElement("codeChanges", Indent.IndentMultiply(3));
                    sw.WriteXmlCloseElement("child", Indent.IndentMultiply(2));
                }

            }

            sw.WriteXmlCloseElement("children", Indent.IndentMultiply(1));

            sw.WriteXmlCloseElement("codeSource", Indent.IndentMultiply());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Writes an XML history item.
        /// </summary>
        /// <param name="sw">The StreamWriter.</param>
        /// <param name="history">The history.</param>
        /// <param name="indent">(Immutable) the indent. 1 TAB value.</param>
        /// =================================================================================================
        private static void WriteXmlHistoryItem(StreamWriter sw, CodeSourceObjectHistory history, string indent)
        {
            sw.WriteXmlElement(indent, "codePath", $"{history.CodePath.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "sourceUrl", $"{history.SourceUrl.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "authorName", $"{history.AuthorName.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "copyright", $"{history.Copyright.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "appliedOn", $"{history.AppliedOn?.ToString("yyyy-MM-dd")}");
            sw.WriteXmlElement(indent, "comment", $"{history.Comment.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "version", $"{history.Version:##.0##}");
            sw.WriteXmlElement(indent, "tags", $"{history.Tags.IfIsNullThenEmpty()}");
            sw.WriteXmlElement(indent, "relatedTaskId", $"{history.RelatedTaskId.IfIsNullThenEmpty()}");
        }
    }
}