using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public class ExporterRegistryTests
    {
        [TearDown]
        public void RestoreRegistry()
        {
            BuiltInExporters.RestoreRegistry();
        }

        [Test]
        public void ExporterRegistry_Default_ContainsExactlyTheSixBuiltIns()
        {
            Assert.That(ExporterRegistry.GetRegisteredFormats(), Is.EquivalentTo(BuiltInExporters.Formats).IgnoreCase);
        }

        [Test]
        public void Register_SameFormatTwice_LastRegistrationWins()
        {
            ExporterRegistry.Register(new MarkerExporter(ExportFormats.Json, "first"));
            ExporterRegistry.Register(new MarkerExporter("json", "second"));

            var output = ExportEmptyViaRegistry(ExportFormats.Json);

            Assert.That(output, Is.EqualTo("second"));
        }

        [Test]
        public void Unregister_DifferentCase_RemovesTheFormat()
        {
            var removed = ExporterRegistry.Unregister("json");

            Assert.Multiple(() =>
            {
                Assert.That(removed, Is.True);
                Assert.That(ExporterRegistry.GetRegisteredFormats(), Does.Not.Contain(ExportFormats.Json).IgnoreCase);
            });
        }

        [Test]
        public void Export_DifferentCaseFormat_UsesTheBuiltInExporter()
        {
            var output = ExportEmptyViaRegistry("json");

            Assert.That(output.TrimStart(), Does.StartWith("["));
        }

        [Test]
        public void Export_NullFormatToStream_ThrowsArgumentNullExceptionBeforeWriting()
        {
            using var stream = new MemoryStream();

            var exception = Assert.Throws<ArgumentNullException>(
                () => ExporterRegistry.Export(null, Array.Empty<CodeSourceObjectsResult>(), stream));

            Assert.Multiple(() =>
            {
                Assert.That(exception.ParamName, Is.EqualTo("format"));
                Assert.That(stream.Length, Is.EqualTo(0), "nothing is written before the guard");
            });
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void ExportToSavePath_NullItems_ThrowsAndLeavesExistingFileUnchanged(string format)
        {
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<ArgumentNullException>(() => ExporterRegistry.Export(format, null, path));
                Assert.That(exception.ParamName, Is.EqualTo("items"));
            });
        }

        [Test]
        public void ExportToSavePath_UnknownFormat_ThrowsAndLeavesExistingFileUnchanged()
        {
            const string unknown = "NO-SUCH-FORMAT";
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<CodeSourceUndefinedExportFormat>(
                    () => ExporterRegistry.Export(unknown, CodeSourceResults.Sample(), path));
                Assert.That(exception.Format, Is.EqualTo(unknown));
            });
        }

        [Test]
        public void ExportToSavePath_NullFormat_ThrowsAndLeavesExistingFileUnchanged()
        {
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<ArgumentNullException>(
                    () => ExporterRegistry.Export(null, CodeSourceResults.Sample(), path));
                Assert.That(exception.ParamName, Is.EqualTo("format"));
            });
        }

        [Test]
        public void ExportToSavePath_NullPath_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => ExporterRegistry.Export(ExportFormats.Json, CodeSourceResults.Sample(), (string)null));

            Assert.That(exception.ParamName, Is.EqualTo("savePath"));
        }

        [TestCase("", TestName = "{m}(empty)")]
        [TestCase("   ", TestName = "{m}(spaces)")]
        [TestCase("\t", TestName = "{m}(tab)")]
        public void ExportToSavePath_EmptyOrWhitespacePath_ThrowsArgumentException(string savePath)
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ExporterRegistry.Export(ExportFormats.Json, CodeSourceResults.Sample(), savePath));

            Assert.That(exception.ParamName, Is.EqualTo("savePath"));
        }

        private static string ExportEmptyViaRegistry(string format)
            => ExportRunner.ExportTextViaRegistry(format, Array.Empty<CodeSourceObjectsResult>());

        private static void WithExistingFile(Action<string> act)
        {
            var path = TempFiles.NewPath("keep");
            var original = Encoding.UTF8.GetBytes("pre-existing content that an invalid call must not truncate\n");
            try
            {
                File.WriteAllBytes(path, original);

                act(path);

                Assert.That(File.ReadAllBytes(path), Is.EqualTo(original),
                    "the pre-existing file must not be truncated or rewritten by an invalid call");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private sealed class MarkerExporter : ICodeSourceExporter
        {
            private readonly byte[] _marker;

            public MarkerExporter(string format, string marker)
            {
                Format = format;
                _marker = Encoding.UTF8.GetBytes(marker);
            }

            public string Format { get; }

            public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
                => outputStream.Write(_marker, 0, _marker.Length);
        }
    }
}
