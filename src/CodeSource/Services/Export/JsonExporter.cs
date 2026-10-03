// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="JsonExporter.cs" company="RzR SOFT & TECH">
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
using RzR.Core.CodeSource.Extensions.Internal.Builder;
using RzR.Core.CodeSource.Models;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace RzR.Core.CodeSource.Services.Export
{
    /// <inheritdoc cref="ICodeSourceExporter" />
    public sealed class JsonExporter : ICodeSourceExporter
    {
        /// <summary>
        ///     (Immutable) the indent. 1 TAB (4 spaces) value.
        /// </summary>
        private const string Indent = "\t";

        /// <inheritdoc />
        public string Format { get; } = ExportFormats.Json;

        /// <inheritdoc />
        public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            try
            {
                // Per-call state: the registry shares one exporter instance across threads.
                var isFirst = true;
                using (var sw = new StreamWriter(new NonClosingStream(outputStream), Encoding.UTF8))
                {
                    sw.WriteJsonOpenArray(Indent.IndentMultiply(-1)); // start of array (codeSources) [ 
                    foreach (var item in items) WriteItem(sw, item, ref isFirst);
                    sw.WriteJsonCloseArray(Indent.IndentMultiply(-1), true, false); // end of array (codeSources) ]
                }
            }
            catch (Exception ex)
            {
                throw new CodeSourceExporterException(Format, ex);
            }
        }

        /// <summary>
        ///     Writes an item.
        /// </summary>
        /// <param name="sw">The software.</param>
        /// <param name="item">The item.</param>
        /// <param name="isFirst">
        ///     [in,out] True while no item has been written in the current export; set to false once an
        ///     item is written.
        /// </param>
        private static void WriteItem(StreamWriter sw, CodeSourceObjectsResult item, ref bool isFirst)
        {
            if (item.IsNull())
                return;

            if (!isFirst)
                sw.WriteJsonObjDelimiter();

            isFirst = false;

            sw.WriteJsonIndent(Indent.IndentMultiply())
                .WriteJsonOpenObject(); // start of object (codeSource) {

            // Parent
            var parent = item.Parent;
            WriteJsonParent(sw, parent);

            sw.WriteJsonObjDelimiter();

            // Children
            var children = item.Children;
            WriteJsonChildren(sw, children);

            sw.WriteJsonNewLine()
                .WriteJsonIndent(Indent.IndentMultiply())
                .WriteJsonCloseObject(); // end of object (codeSource) }
        }

        /// <summary>
        ///     Writes the parent of this item.
        /// </summary>
        /// <param name="sw">The software.</param>
        /// <param name="parent">The parent.</param>
        private static void WriteJsonParent(StreamWriter sw, CodeSourceObject parent)
        {
            sw.WriteJsonIndent(Indent.IndentMultiply(1))
                .WriteProp("parent", null, false, false, false)
                .WriteJsonOpenObject(); // start of object (parent) {

            sw.WriteJsonIndent(Indent.IndentMultiply(2))
                .WriteProp("name", parent.Name);
            sw.WriteJsonIndent(Indent.IndentMultiply(2))
                .WriteProp("fullName", parent.FullName);

            sw.WriteJsonIndent(Indent.IndentMultiply(2))
                .WriteProp("codeChanges", null, false, false, false)
                .WriteJsonOpenArray(); // start of object (codeChanges) [

            if (parent.History.HasAnyData())
            {
                var historyCount = parent.History.Count();
                for (var i = 0; i < historyCount; i++)
                {
                    var h = parent.History.ElementAt(i);

                    sw.WriteJsonIndent(Indent.IndentMultiply(3))
                        .WriteJsonOpenObject(); // start of object (codeChange) {

                    WriteJsonHistoryItem(sw, h, Indent.IndentMultiply(4));

                    sw.WriteJsonNewLine()
                        .WriteJsonCloseObject(Indent.IndentMultiply(3)); // end of object (codeChange) }

                    if (i != historyCount - 1)
                        sw.WriteJsonObjDelimiter();
                }
            }

            sw.WriteJsonNewLine()
                .WriteJsonIndent(Indent.IndentMultiply(2))
                .WriteJsonCloseArray(null, false); // end of object (codeChanges) ]

            sw.WriteJsonIndent(Indent.IndentMultiply(1))
                .WriteJsonCloseObject(); // end of object (parent) }
        }

        /// <summary>
        ///     Writes the children of this item.
        /// </summary>
        /// <param name="sw">The software.</param>
        /// <param name="children">The children.</param>
        private static void WriteJsonChildren(StreamWriter sw, IEnumerable<CodeSourceObject> children)
        {
            sw.WriteJsonIndent(Indent.IndentMultiply(1))
                .WriteProp("children", null, false, false, false)
                .WriteJsonOpenArray(); // start of array (children) [

            var childrenCount = children.Count();
            for (var i = 0; i < childrenCount; i++)
            {
                var child = children.ElementAt(i);

                sw.WriteJsonIndent(Indent.IndentMultiply(2))
                    .WriteJsonOpenObject(); // start of object (child) {

                sw.WriteJsonIndent(Indent.IndentMultiply(3))
                    .WriteProp("name", child.Name);
                sw.WriteJsonIndent(Indent.IndentMultiply(3))
                    .WriteProp("fullName", child.FullName);

                sw.WriteJsonIndent(Indent.IndentMultiply(3))
                    .WriteProp("codeChanges", null, false, false, false)
                    .WriteJsonOpenArray(); // start of object (codeChanges) [
                if (child.History.HasAnyData())
                {
                    var historyCount = child.History.Count();
                    for (var j = 0; j < historyCount; j++)
                    {
                        var h = child.History.ElementAt(j);

                        sw.WriteJsonIndent(Indent.IndentMultiply(4))
                            .WriteJsonOpenObject(); // start of object (codeChange) {

                        WriteJsonHistoryItem(sw, h, Indent.IndentMultiply(5));

                        sw.WriteJsonNewLine()
                            .WriteJsonCloseObject(Indent.IndentMultiply(4)); // end of object (codeChange) }

                        if (j != historyCount - 1)
                            sw.WriteJsonObjDelimiter();
                    }
                }

                sw.WriteJsonNewLine()
                    .WriteJsonIndent(Indent.IndentMultiply(3))
                    .WriteJsonCloseArray(null, false); // end of object (codeChanges) ]

                sw.WriteJsonIndent(Indent.IndentMultiply(2))
                    .WriteJsonCloseObject(); // end of object (child) }

                if (i != childrenCount - 1)
                    sw.WriteJsonObjDelimiter();
            }

            sw.WriteJsonNewLine().WriteJsonIndent(Indent.IndentMultiply(1))
                .WriteJsonCloseArray(null, false, false); // end of array (children) ]
        }

        /// <summary>
        ///     Writes a JSON history item.
        /// </summary>
        /// <param name="sw">The software.</param>
        /// <param name="history">The history.</param>
        /// <param name="indent">(Immutable) the indent. 1 TAB (4 spaces) value.</param>
        private static void WriteJsonHistoryItem(StreamWriter sw, CodeSourceObjectHistory history, string indent)
        {
            sw.WriteJsonIndent(indent)
                .WriteProp("codePath", history.CodePath.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("sourceUrl", history.SourceUrl.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("authorName", history.AuthorName.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("copyright", history.Copyright.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("appliedOn", history.AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            sw.WriteJsonIndent(indent)
                .WriteProp("comment", history.Comment.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("version", history.Version.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("tags", history.Tags.IfIsNullThenEmpty());

            sw.WriteJsonIndent(indent)
                .WriteProp("relatedTaskId", history.RelatedTaskId.IfIsNullThenEmpty(), false, true, false);
        }
    }
}