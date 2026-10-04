using System;
using System.Collections.Generic;
using System.Linq;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Services;
using RzR.Core.CodeSource.Services.Export;

namespace Tests.Support
{
    internal static class BuiltInExporters
    {
        internal static readonly string[] Formats =
        {
            ExportFormats.Csv, ExportFormats.Html, ExportFormats.Json, ExportFormats.Markdown, ExportFormats.Xml,
            ExportFormats.Yaml
        };

        internal static readonly string[] BomFormats =
        {
            ExportFormats.Csv, ExportFormats.Html, ExportFormats.Json, ExportFormats.Markdown, ExportFormats.Yaml
        };

        internal static ICodeSourceExporter Create(string format)
            => format switch
            {
                ExportFormats.Csv => new CsvExporter(),
                ExportFormats.Html => new HtmlExporter(),
                ExportFormats.Json => new JsonExporter(),
                ExportFormats.Markdown => new MarkdownExporter(),
                ExportFormats.Xml => new XmlExporter(),
                ExportFormats.Yaml => new YamlExporter(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown built-in format.")
            };

        internal static IReadOnlyList<ICodeSourceExporter> CreateAll()
            => Formats.Select(Create).ToList();

        internal static void RestoreRegistry()
        {
            var foreignFormats = ExporterRegistry.GetRegisteredFormats()
                .Where(format => !Formats.Contains(format, StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var format in foreignFormats)
                ExporterRegistry.Unregister(format);

            foreach (var exporter in CreateAll())
                ExporterRegistry.Register(exporter);
        }
    }
}
