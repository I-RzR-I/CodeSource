using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    [NonParallelizable]
    public class ConcurrencyTests
    {
        private const int ThreadCount = 16;
        private const int Iterations = 200;
        private const string StressFormatPrefix = "STRESS-";
        private const string StableFormat = StressFormatPrefix + "STABLE";

        private static readonly int RoleCount = Enum.GetValues(typeof(Role)).Length;
        private static readonly TimeSpan JoinTimeout = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan CancelGrace = TimeSpan.FromSeconds(10);

        private enum Role
        {
            RegisterUnregister = 0,
            ReRegisterBuiltIns,
            ExportJson,
            ExportCsvAndCustom
        }

        [TearDown]
        public void RestoreRegistry()
        {
            BuiltInExporters.RestoreRegistry();
        }

        [Test]
        public void JsonExport_ConcurrentCalls_EachOutputContainsOnlyItsOwnItems()
        {
            var failures = RunConcurrently((thread, iteration) =>
            {
                var itemCount = 1 + (thread + iteration) % 5;
                var childCount = (thread * 7 + iteration) % 3;
                var tag = $"T{thread}I{iteration}";

                var text = ExportRunner.ExportTextViaRegistry(ExportFormats.Json, Dataset(tag, itemCount, childCount));

                return CheckJson(text, tag, itemCount, childCount);
            });

            Assert.That(failures, Is.Empty, Summary(failures));
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_ConcurrentCalls_OutputIsByteIdenticalToSingleThreaded(string format)
        {
            var items = CodeSourceResults.Sample();
            items.AddRange(CodeSourceResults.Uniform("a\"b|c<d>&e\n=f `g` ,h"));

            var expected = ExportRunner.ExportViaRegistry(format, items);
            Assert.That(expected.Length, Is.GreaterThan(0), "single-threaded baseline must not be empty");

            var failures = RunConcurrently((_, _) =>
            {
                var actual = ExportRunner.ExportViaRegistry(format, items);

                return actual.AsSpan().SequenceEqual(expected)
                    ? null
                    : $"{actual.Length} bytes differ from the {expected.Length}-byte baseline";
            });

            Assert.That(failures, Is.Empty, Summary(failures));
        }

        [Test]
        public void Registry_RegisterUnregisterDuringExport_RaisesNoException()
        {
            var items = CodeSourceResults.Sample();
            ExporterRegistry.Register(new StubExporter(StableFormat));

            var failures = RunConcurrently((thread, _) => (Role)(thread % RoleCount) switch
            {
                Role.RegisterUnregister => RegisterAndUnregister(thread),
                Role.ReRegisterBuiltIns => ReRegisterBuiltIns(),
                Role.ExportJson => ExportNonEmpty(ExportFormats.Json, items),
                Role.ExportCsvAndCustom => ExportNonEmpty(ExportFormats.Csv, items)
                     ?? ExportStable(items)
                     ?? ProbeOwnerFormat(OwnerOf(thread), items),
                _ => throw new ArgumentOutOfRangeException(nameof(thread), thread, "no role is mapped to this thread")
            });

            Assert.That(failures, Is.Empty, Summary(failures));
            Assert.That(ExporterRegistry.Unregister(StableFormat), Is.True, "the stable stub stayed registered for the whole run");

            var registered = ExporterRegistry.GetRegisteredFormats().ToList();
            Assert.That(registered, Is.SupersetOf(BuiltInExporters.Formats).IgnoreCase, "built-ins still registered");
            Assert.That(registered.Where(format => format.StartsWith(StressFormatPrefix, StringComparison.OrdinalIgnoreCase)),
                Is.Empty, "every stress format was unregistered by its owner");
        }

        private static List<string> RunConcurrently(Func<int, int, string> body)
        {
            var failures = new ConcurrentQueue<string>();
            using var stop = new CancellationTokenSource();
            using var start = new Barrier(ThreadCount);

            var threads = Enumerable.Range(0, ThreadCount)
                .Select(id => new Thread(() => RunIterations(id, body, start, stop.Token, failures))
                {
                    IsBackground = true,
                    Name = $"concurrency-{id}"
                })
                .ToList();

            threads.ForEach(thread => thread.Start());

            var clock = Stopwatch.StartNew();
            foreach (var thread in threads)
            {
                if (thread.Join(Remaining(JoinTimeout, clock)))
                    continue;

                failures.Enqueue($"the run did not finish within {JoinTimeout} ({thread.Name} was still running); cancellation requested");
                stop.Cancel();
                break;
            }

            if (!stop.IsCancellationRequested)
                return failures.ToList();

            var grace = Stopwatch.StartNew();
            foreach (var thread in threads)
            {
                if (!thread.Join(Remaining(CancelGrace, grace)))
                    failures.Enqueue($"{thread.Name} still running after cancellation; registry state after this test is unreliable");
            }

            return failures.ToList();
        }

        private static TimeSpan Remaining(TimeSpan budget, Stopwatch clock)
        {
            var left = budget - clock.Elapsed;

            return left > TimeSpan.Zero ? left : TimeSpan.Zero;
        }

        private static void RunIterations(int thread, Func<int, int, string> body, Barrier start,
            CancellationToken stop, ConcurrentQueue<string> failures)
        {
            try
            {
                start.SignalAndWait(stop);
                for (var i = 0; i < Iterations && !stop.IsCancellationRequested; i++)
                {
                    try
                    {
                        var problem = body(thread, i);
                        if (problem != null)
                            failures.Enqueue($"thread {thread} iteration {i}: {problem}");
                    }
                    catch (Exception ex)
                    {
                        failures.Enqueue($"thread {thread} iteration {i}: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                failures.Enqueue($"thread {thread} crashed: {ex}");
            }
        }

        private static int OwnerOf(int thread) => thread - thread % RoleCount;

        private static string StressFormat(int ownerThread) => StressFormatPrefix + ownerThread;

        private static string RegisterAndUnregister(int thread)
        {
            var format = StressFormat(thread);
            ExporterRegistry.Register(new StubExporter(format));

            return ExporterRegistry.Unregister(format.ToLowerInvariant()) ? null : "Unregister returned false";
        }

        private static string ReRegisterBuiltIns()
        {
            ExporterRegistry.Register(BuiltInExporters.Create(ExportFormats.Json));
            ExporterRegistry.Register(BuiltInExporters.Create(ExportFormats.Csv));

            var formats = ExporterRegistry.GetRegisteredFormats().ToList();

            return formats.Contains(ExportFormats.Json, StringComparer.OrdinalIgnoreCase)
                ? null
                : $"JSON missing from {string.Join(",", formats)}";
        }

        private static string ExportNonEmpty(string format, IEnumerable<CodeSourceObjectsResult> items)
            => ExportRunner.ExportViaRegistry(format, items).Length == 0 ? $"empty {format} output" : null;

        private static string ExportStable(IEnumerable<CodeSourceObjectsResult> items)
            => MarkerProblem(StableFormat, ExportRunner.ExportViaRegistry(StableFormat, items));

        private static string ProbeOwnerFormat(int ownerThread, IEnumerable<CodeSourceObjectsResult> items)
        {
            var format = StressFormat(ownerThread);
            byte[] output;
            try
            {
                output = ExportRunner.ExportViaRegistry(format, items);
            }
            catch (CodeSourceUndefinedExportFormat)
            {
                return null;
            }

            return MarkerProblem(format, output);
        }

        private static string MarkerProblem(string format, byte[] output)
            => output.Length == 1 && output[0] == StubExporter.Marker
                ? null
                : $"{format} wrote {output.Length} byte(s) instead of the single 0x{StubExporter.Marker:X2} marker";

        private static List<CodeSourceObjectsResult> Dataset(string tag, int itemCount, int childCount)
        {
            var items = new List<CodeSourceObjectsResult>();
            for (var k = 0; k < itemCount; k++)
            {
                var parentName = $"{tag}P{k}";
                var children = Enumerable.Range(0, childCount)
                    .Select(c => CodeSourceResults.NewObject($"M{c}", $"Ns.{parentName}.M{c}",
                        CodeSourceResults.History(codePath: $"Ns.{parentName}.M{c}", comment: "child \"c\"\n",
                            version: "1.0")))
                    .ToArray();

                items.Add(CodeSourceResults.Result(
                    CodeSourceResults.NewObject(parentName, $"Ns.{parentName}",
                        CodeSourceResults.History(codePath: $"Ns.{parentName}", sourceUrl: "https://example.com/" + tag,
                            authorName: "a\"b", copyright: "\u00A9 x", appliedOn: new DateTime(2022, 12, 12),
                            comment: "line1\nline2", version: "1.0", tags: "t1;t2", relatedTaskId: "WI-" + k),
                        CodeSourceResults.History(codePath: $"Ns.{parentName}", version: "1.10")),
                    children));
            }

            return items;
        }

        private static string CheckJson(string text, string tag, int itemCount, int childCount)
        {
            using var document = JsonDocument.Parse(text);
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

            return null;
        }

        private static string Summary(IReadOnlyCollection<string> failures)
            => $"{failures.Count} failure(s); first: {string.Join(" | ", failures.Take(5))}";

        private sealed class StubExporter : ICodeSourceExporter
        {
            internal const byte Marker = 0x2A;

            public StubExporter(string format) => Format = format;

            public string Format { get; }

            public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
            {
                if (items == null)
                    throw new ArgumentNullException(nameof(items));
                if (outputStream == null)
                    throw new ArgumentNullException(nameof(outputStream));

                outputStream.WriteByte(Marker);
            }
        }
    }
}
