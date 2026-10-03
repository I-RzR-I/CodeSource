using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Exceptions;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using RzR.Core.CodeSource.Services.Export;

namespace Tests
{
    [TestFixture]
    [NonParallelizable]
    public class ConcurrencyTests
    {
        private const int ThreadCount = 16;
        private const int Iterations = 200;
        private const string StressFormatPrefix = "STRESS-";

        private static readonly TimeSpan JoinTimeout = TimeSpan.FromMinutes(2);

        private static IEnumerable<string> Formats() => ExportTestData.AllFormats;

        [Test]
        public void Json_16ThreadsX200_EachOutputParsesAndItemCountMatches()
        {
            var failures = new ConcurrentQueue<string>();

            RunConcurrently(failures, thread =>
            {
                for (var i = 0; i < Iterations; i++)
                {
                    var itemCount = 1 + (thread + i) % 5;
                    var childCount = (thread * 7 + i) % 3;
                    var tag = $"T{thread}I{i}";
                    var items = Dataset(tag, itemCount, childCount);

                    try
                    {
                        var text = ExportTestData.ExportText(ExportFormats.Json, items);
                        var problem = CheckJson(text, tag, itemCount, childCount);
                        if (problem != null)
                            failures.Enqueue($"thread {thread} iteration {i}: {problem}");
                    }
                    catch (Exception ex)
                    {
                        failures.Enqueue($"thread {thread} iteration {i}: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            });

            Assert.That(failures, Is.Empty, Summary(failures));
        }

        [TestCaseSource(nameof(Formats))]
        public void AllFormats_ConcurrentOutput_IsByteIdenticalToSingleThreaded(string format)
        {
            var items = ExportTestData.Sample();
            items.AddRange(ExportTestData.Uniform("a\"b|c<d>&e\n=f `g` ,h"));

            var expected = ExportTestData.Export(format, items);
            Assert.That(expected.Length, Is.GreaterThan(0), "single-threaded baseline must not be empty");

            var failures = new ConcurrentQueue<string>();
            RunConcurrently(failures, thread =>
            {
                for (var i = 0; i < Iterations; i++)
                {
                    try
                    {
                        var actual = ExportTestData.Export(format, items);
                        if (!actual.SequenceEqual(expected))
                            failures.Enqueue(
                                $"thread {thread} iteration {i}: {actual.Length} bytes differ from the {expected.Length}-byte baseline");
                    }
                    catch (Exception ex)
                    {
                        failures.Enqueue($"thread {thread} iteration {i}: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            });

            Assert.That(failures, Is.Empty, Summary(failures));
        }

        [Test]
        public void RegisterUnregister_ConcurrentWithExport_RaisesNoUnexpectedException()
        {
            var items = ExportTestData.Sample();
            var failures = new ConcurrentQueue<string>();

            try
            {
                RunConcurrently(failures, thread =>
                {
                    for (var i = 0; i < Iterations; i++)
                    {
                        try
                        {
                            switch (thread % 4)
                            {
                                case 0:
                                    ExporterRegistry.Register(new StubExporter(StressFormatPrefix + thread));
                                    if (!ExporterRegistry.Unregister((StressFormatPrefix + thread).ToLowerInvariant()))
                                        failures.Enqueue($"thread {thread} iteration {i}: Unregister returned false");
                                    break;

                                case 1:
                                    ExporterRegistry.Register(new JsonExporter());
                                    ExporterRegistry.Register(new CsvExporter());
                                    var formats = ExporterRegistry.GetRegisteredFormats().ToList();
                                    if (!formats.Contains(ExportFormats.Json, StringComparer.OrdinalIgnoreCase))
                                        failures.Enqueue($"thread {thread} iteration {i}: JSON missing from {string.Join(",", formats)}");
                                    break;

                                case 2:
                                    if (ExportTestData.Export(ExportFormats.Json, items).Length == 0)
                                        failures.Enqueue($"thread {thread} iteration {i}: empty JSON output");
                                    break;

                                default:
                                    if (ExportTestData.Export(ExportFormats.Csv, items).Length == 0)
                                        failures.Enqueue($"thread {thread} iteration {i}: empty CSV output");
                                    ExportCustomFormatTolerantly(StressFormatPrefix + (thread - 3));
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            failures.Enqueue($"thread {thread} iteration {i}: {ex.GetType().Name}: {ex.Message}");
                        }
                    }
                });
            }
            finally
            {
                for (var thread = 0; thread < ThreadCount; thread++)
                    ExporterRegistry.Unregister(StressFormatPrefix + thread);
                RestoreBuiltInExporters();
            }

            Assert.That(failures, Is.Empty, Summary(failures));

            var registered = ExporterRegistry.GetRegisteredFormats().ToList();
            Assert.That(registered, Is.SupersetOf(ExportTestData.AllFormats), "built-ins restored");
            Assert.That(registered.Where(f => f.StartsWith(StressFormatPrefix, StringComparison.OrdinalIgnoreCase)),
                Is.Empty, "stress formats removed");
        }

        private static void RunConcurrently(ConcurrentQueue<string> failures, Action<int> body)
        {
            using (var start = new Barrier(ThreadCount))
            {
                var threads = new Thread[ThreadCount];
                for (var t = 0; t < ThreadCount; t++)
                {
                    var id = t;
                    threads[t] = new Thread(() =>
                    {
                        try
                        {
                            start.SignalAndWait();
                            body(id);
                        }
                        catch (Exception ex)
                        {
                            failures.Enqueue($"thread {id} crashed: {ex}");
                        }
                    })
                    {
                        IsBackground = true,
                        Name = $"concurrency-{id}"
                    };
                }

                foreach (var thread in threads)
                    thread.Start();

                foreach (var thread in threads)
                {
                    if (!thread.Join(JoinTimeout))
                        failures.Enqueue($"{thread.Name} did not finish within {JoinTimeout}");
                }
            }
        }

        private static List<CodeSourceObjectsResult> Dataset(string tag, int itemCount, int childCount)
        {
            var items = new List<CodeSourceObjectsResult>();
            for (var k = 0; k < itemCount; k++)
            {
                var parentName = $"{tag}P{k}";
                var children = Enumerable.Range(0, childCount)
                    .Select(c => ExportTestData.Object($"M{c}", $"Ns.{parentName}.M{c}",
                        ExportTestData.History(codePath: $"Ns.{parentName}.M{c}", comment: "child \"c\"\n",
                            version: "1.0")))
                    .ToArray();

                items.Add(ExportTestData.Result(
                    ExportTestData.Object(parentName, $"Ns.{parentName}",
                        ExportTestData.History($"Ns.{parentName}", "https://example.com/" + tag, "a\"b", "© x",
                            new DateTime(2022, 12, 12), "line1\nline2", "1.0", "t1;t2", "WI-" + k),
                        ExportTestData.History(codePath: $"Ns.{parentName}", version: "1.10")),
                    children));
            }

            return items;
        }

        private static string CheckJson(string text, string tag, int itemCount, int childCount)
        {
            using (var document = JsonDocument.Parse(text))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Array)
                    return $"root is {root.ValueKind}, expected Array";
                if (root.GetArrayLength() != itemCount)
                    return $"{root.GetArrayLength()} items, expected {itemCount}";

                for (var k = 0; k < itemCount; k++)
                {
                    var item = root[k];
                    var parent = item.GetProperty("parent");
                    var name = parent.GetProperty("name").GetString();
                    if (name != $"{tag}P{k}")
                        return $"item {k} parent name '{name}', expected '{tag}P{k}' (cross-talk between exports)";
                    if (parent.GetProperty("codeChanges").GetArrayLength() != 2)
                        return $"item {k} has {parent.GetProperty("codeChanges").GetArrayLength()} parent changes, expected 2";
                    if (item.GetProperty("children").GetArrayLength() != childCount)
                        return $"item {k} has {item.GetProperty("children").GetArrayLength()} children, expected {childCount}";
                }
            }

            return null;
        }

        private static void ExportCustomFormatTolerantly(string format)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    ExporterRegistry.Export(format, ExportTestData.Sample(), stream);
                }
                catch (CodeSourceUndefinedExportFormat)
                {
                }
            }
        }

        private static void RestoreBuiltInExporters()
        {
            foreach (var exporter in new ICodeSourceExporter[]
                     {
                         new CsvExporter(), new HtmlExporter(), new JsonExporter(), new MarkdownExporter(),
                         new XmlExporter(), new YamlExporter()
                     })
                ExporterRegistry.Register(exporter);
        }

        private static string Summary(ConcurrentQueue<string> failures)
            => $"{failures.Count} failure(s); first: {string.Join(" | ", failures.Take(5))}";

        private sealed class StubExporter : ICodeSourceExporter
        {
            public StubExporter(string format) => Format = format;

            public string Format { get; }

            public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
            {
                if (items == null)
                    throw new ArgumentNullException(nameof(items));
                if (outputStream == null)
                    throw new ArgumentNullException(nameof(outputStream));

                outputStream.WriteByte(0x2A);
            }
        }
    }
}
