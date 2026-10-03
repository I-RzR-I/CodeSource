using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using RzR.Core.CodeSource.Services.Export;

namespace Tests
{
    internal static class ExportTestData
    {
        internal static readonly string[] AllFormats =
        {
            ExportFormats.Csv, ExportFormats.Html, ExportFormats.Json, ExportFormats.Markdown, ExportFormats.Xml,
            ExportFormats.Yaml
        };

        internal static readonly string[] BomFormats =
        {
            ExportFormats.Csv, ExportFormats.Html, ExportFormats.Json, ExportFormats.Markdown, ExportFormats.Yaml
        };

        internal static ICodeSourceExporter NewExporter(string format)
        {
            switch (format)
            {
                case ExportFormats.Csv: return new CsvExporter();
                case ExportFormats.Html: return new HtmlExporter();
                case ExportFormats.Json: return new JsonExporter();
                case ExportFormats.Markdown: return new MarkdownExporter();
                case ExportFormats.Xml: return new XmlExporter();
                case ExportFormats.Yaml: return new YamlExporter();
                default: throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown built-in format.");
            }
        }

        internal static CodeSourceObjectHistory History(string codePath = null, string sourceUrl = null,
            string authorName = null, string copyright = null, DateTime? appliedOn = null, string comment = null,
            string version = null, string tags = null, string relatedTaskId = null)
        {
            var history = new CodeSourceObjectHistory();
            SetInternal(history, nameof(CodeSourceObjectHistory.CodePath), codePath);
            SetInternal(history, nameof(CodeSourceObjectHistory.SourceUrl), sourceUrl);
            SetInternal(history, nameof(CodeSourceObjectHistory.AuthorName), authorName);
            SetInternal(history, nameof(CodeSourceObjectHistory.Copyright), copyright);
            SetInternal(history, nameof(CodeSourceObjectHistory.AppliedOn), appliedOn);
            SetInternal(history, nameof(CodeSourceObjectHistory.Comment), comment);
            SetInternal(history, nameof(CodeSourceObjectHistory.Version), version);
            SetInternal(history, nameof(CodeSourceObjectHistory.Tags), tags);
            SetInternal(history, nameof(CodeSourceObjectHistory.RelatedTaskId), relatedTaskId);

            return history;
        }

        internal static CodeSourceObjectHistory HistoryAll(string value, DateTime? appliedOn = null)
            => History(value, value, value, value, appliedOn, value, value, value, value);

        internal static CodeSourceObject Object(string name, string fullName,
            params CodeSourceObjectHistory[] history)
            => new CodeSourceObject
            {
                Name = name,
                FullName = fullName,
                History = new List<CodeSourceObjectHistory>(history)
            };

        internal static CodeSourceObjectsResult Result(CodeSourceObject parent, params CodeSourceObject[] children)
            => new CodeSourceObjectsResult { Parent = parent, Children = new List<CodeSourceObject>(children) };

        internal static List<CodeSourceObjectsResult> Uniform(string payload, DateTime? appliedOn = null)
            => new List<CodeSourceObjectsResult>
            {
                Result(
                    Object(payload, payload, HistoryAll(payload, appliedOn)),
                    Object(payload, payload, HistoryAll(payload, appliedOn)))
            };

        internal static List<CodeSourceObjectsResult> Sample()
            => new List<CodeSourceObjectsResult>
            {
                Result(
                    Object("OwnClassData", "TempLib.OwnClassData",
                        History("TempLib.OwnClassData", "https://example.com/a?x=1&y=2", "RzR", "© RzR",
                            new DateTime(2022, 12, 12), "Comment with \"quotes\", commas and <tags>", "1.0",
                            "tag1;tag2", "WI-1"),
                        History(codePath: "TempLib.OwnClassData", version: "1.10")),
                    Object("Method1", "TempLib.OwnClassData.Method1",
                        History("TempLib.OwnClassData.Method1", "http://example.org/", "Someone", null,
                            new DateTime(2023, 1, 31), "a|b `c`", "2.0", null, null)),
                    Object("Method2", "TempLib.OwnClassData.Method2")),
                Result(
                    Object("TempClassData", "TempLib.TempClassData",
                        History(codePath: "TempLib.TempClassData", comment: "line1\nline2", tags: "END-OF-DATA")))
            };

        internal static byte[] Export(string format, IEnumerable<CodeSourceObjectsResult> items)
        {
            using (var stream = new MemoryStream())
            {
                ExporterRegistry.Export(format, items, stream);

                return stream.ToArray();
            }
        }

        internal static string ExportText(string format, IEnumerable<CodeSourceObjectsResult> items)
            => Decode(Export(format, items));

        internal static string Decode(byte[] bytes)
        {
            using (var reader = new StreamReader(new MemoryStream(bytes), new UTF8Encoding(false, true), true))
                return reader.ReadToEnd();
        }

        internal static bool HasBomAt(byte[] bytes, int offset)
            => bytes.Length >= offset + 3 && bytes[offset] == 0xEF && bytes[offset + 1] == 0xBB
               && bytes[offset + 2] == 0xBF;

        private static void SetInternal(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            var setter = property?.GetSetMethod(true);
            if (setter == null)
                throw new InvalidOperationException(
                    $"{target.GetType().Name}.{propertyName} has no setter reachable by reflection.");

            setter.Invoke(target, new[] { value });
        }
    }
}
