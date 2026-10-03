using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Exceptions;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using YamlDotNet.RepresentationModel;

namespace Tests
{
    [TestFixture]
    public class StreamOwnershipTests
    {
        private static IEnumerable<string> Formats() => ExportTestData.AllFormats;

        private static IEnumerable<TestCaseData> BomPolicyCases()
            => ExportTestData.AllFormats.Select(f =>
                new TestCaseData(f, ExportTestData.BomFormats.Contains(f)).SetName($"{{m}}({f})"));

        [Test]
        public void Export_LeavesCallerStreamOpenAndFlushed(
            [ValueSource(nameof(Formats))] string format,
            [Values] bool viaRegistry)
        {
            var items = ExportTestData.Sample();
            var expected = ExportViaSavePath(format, items);

            using (var stream = new TrackingMemoryStream())
            {
                if (viaRegistry)
                    ExporterRegistry.Export(format, items, stream);
                else
                    ExportTestData.NewExporter(format).Export(items, stream);

                Assert.Multiple(() =>
                {
                    Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                    Assert.That(stream.CanWrite, Is.True, "CanWrite");
                    Assert.That(stream.CanRead, Is.True, "CanRead");
                    Assert.That(stream.ToArray().Length, Is.GreaterThan(0), "Length (ToArray also works on a closed stream)");
                    Assert.That(stream.ToArray(), Is.EqualTo(expected), "complete output, identical to the savePath overload");
                });

                AssertCompleteDocument(format, stream.ToArray());

                var length = stream.Length;
                stream.WriteByte(0x0A);
                Assert.That(stream.Length, Is.EqualTo(length + 1));
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_ToWriteOnlyNonSeekableStream_LeavesItOpenAndComplete(string format)
        {
            var items = ExportTestData.Sample();
            var expected = ExportViaSavePath(format, items);

            using (var stream = new WriteOnlyStream())
            {
                ExportTestData.NewExporter(format).Export(items, stream);

                Assert.Multiple(() =>
                {
                    Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                    Assert.That(stream.CanWrite, Is.True, "CanWrite");
                    Assert.That(stream.Written, Is.EqualTo(expected), "complete output");
                });
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_ToPrefilledStream_WritesNoBomAtOffsetN(string format)
        {
            var items = ExportTestData.Sample();
            var prefix = Encoding.ASCII.GetBytes("PREFIX-0123456789\n");
            var reference = ExportViaSavePath(format, items);
            var referenceBody = ExportTestData.HasBomAt(reference, 0) ? reference.Skip(3).ToArray() : reference;

            using (var stream = new MemoryStream())
            {
                stream.Write(prefix, 0, prefix.Length);

                ExportTestData.NewExporter(format).Export(items, stream);

                var bytes = stream.ToArray();
                Assert.Multiple(() =>
                {
                    Assert.That(bytes.Take(prefix.Length), Is.EqualTo(prefix), "prefix untouched");
                    Assert.That(ExportTestData.HasBomAt(bytes, prefix.Length), Is.False, "no BOM in the middle of the stream");
                    Assert.That(bytes.Skip(prefix.Length), Is.EqualTo(referenceBody), "document appended after the prefix");
                });
            }
        }

        [TestCaseSource(nameof(BomPolicyCases))]
        public void Export_ToEmptyStream_KeepsBomPolicy(string format, bool expectBom)
        {
            using (var stream = new MemoryStream())
            {
                ExportTestData.NewExporter(format).Export(ExportTestData.Sample(), stream);

                Assert.That(ExportTestData.HasBomAt(stream.ToArray(), 0), Is.EqualTo(expectBom));
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_ViaSavePath_ReleasesFileHandle(string format)
        {
            var path = NewTempPath(format);
            try
            {
                ExporterRegistry.Export(format, ExportTestData.Sample(), path);

                Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));
                using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }

                File.Delete(path);
                Assert.That(File.Exists(path), Is.False);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_ViaSavePath_NullItems_ThrowsAndLeavesExistingFileUnchanged(string format)
        {
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<ArgumentNullException>(() => ExporterRegistry.Export(format, null, path));
                Assert.That(exception.ParamName, Is.EqualTo("items"));
            });
        }

        [Test]
        public void Export_ViaSavePath_UnknownFormat_ThrowsAndLeavesExistingFileUnchanged()
        {
            const string unknown = "NO-SUCH-FORMAT";
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<CodeSourceUndefinedExportFormat>(
                    () => ExporterRegistry.Export(unknown, ExportTestData.Sample(), path));
                Assert.That(exception.Format, Is.EqualTo(unknown));
            });
        }

        [Test]
        public void Export_ViaSavePath_NullFormat_ThrowsAndLeavesExistingFileUnchanged()
        {
            WithExistingFile(path =>
            {
                var exception = Assert.Throws<ArgumentNullException>(
                    () => ExporterRegistry.Export(null, ExportTestData.Sample(), path));
                Assert.That(exception.ParamName, Is.EqualTo("format"));
            });
        }

        [Test]
        public void Export_ViaSavePath_NullPath_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => ExporterRegistry.Export(ExportFormats.Json, ExportTestData.Sample(), (string)null));

            Assert.That(exception.ParamName, Is.EqualTo("savePath"));
        }

        [TestCase("", TestName = "{m}(empty)")]
        [TestCase("   ", TestName = "{m}(spaces)")]
        [TestCase("\t", TestName = "{m}(tab)")]
        public void Export_ViaSavePath_EmptyOrWhitespacePath_ThrowsArgumentException(string savePath)
        {
            var exception = Assert.Throws<ArgumentException>(
                () => ExporterRegistry.Export(ExportFormats.Json, ExportTestData.Sample(), savePath));

            Assert.That(exception.ParamName, Is.EqualTo("savePath"));
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_WhenEnumerationFails_CallerStreamStaysOpen(string format)
        {
            using (var stream = new TrackingMemoryStream())
            {
                Assert.Catch(() => ExportTestData.NewExporter(format).Export(ThrowingItems(), stream));

                Assert.Multiple(() =>
                {
                    Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                    Assert.That(stream.CanWrite, Is.True, "CanWrite");
                });
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_NullStream_ThrowsArgumentNullException(string format)
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => ExportTestData.NewExporter(format).Export(ExportTestData.Sample(), null));

            Assert.That(exception.ParamName, Is.EqualTo("outputStream"));
        }

        [TestCaseSource(nameof(Formats))]
        public void Export_NullItems_ThrowsArgumentNullExceptionAndWritesNothing(string format)
        {
            using (var stream = new TrackingMemoryStream())
            {
                var exception = Assert.Throws<ArgumentNullException>(
                    () => ExportTestData.NewExporter(format).Export(null, stream));

                Assert.Multiple(() =>
                {
                    Assert.That(exception.ParamName, Is.EqualTo("items"));
                    Assert.That(stream.DisposeCalled, Is.False, "stream not disposed");
                    Assert.That(stream.Length, Is.EqualTo(0), "nothing written");
                });
            }
        }

        private static byte[] ExportViaSavePath(string format, IEnumerable<CodeSourceObjectsResult> items)
        {
            var path = NewTempPath(format);
            try
            {
                ExporterRegistry.Export(format, items, path);
                return File.ReadAllBytes(path);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static void WithExistingFile(Action<string> act)
        {
            var path = NewTempPath("keep");
            var original = Encoding.UTF8.GetBytes("pre-existing content that an invalid call must not truncate\n");
            File.WriteAllBytes(path, original);
            try
            {
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

        private static string NewTempPath(string format)
            => Path.Combine(Path.GetTempPath(), $"codesource-{Guid.NewGuid():N}.{format.ToLowerInvariant()}");

        private static IEnumerable<CodeSourceObjectsResult> ThrowingItems()
        {
            yield return ExportTestData.Sample()[0];
            throw new InvalidOperationException("enumeration failure injected by the test");
        }

        private static void AssertCompleteDocument(string format, byte[] bytes)
        {
            var text = ExportTestData.Decode(bytes);
            Assert.That(text, Does.Contain("END-OF-DATA"), "the last value of the dataset must be present");

            switch (format)
            {
                case ExportFormats.Json:
                    using (var document = JsonDocument.Parse(text))
                        Assert.That(document.RootElement.GetArrayLength(), Is.EqualTo(2));
                    break;
                case ExportFormats.Xml:
                    var xml = new XmlDocument { XmlResolver = null };
                    using (var reader = XmlReader.Create(new StringReader(text),
                               new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                        xml.Load(reader);
                    Assert.That(xml.SelectNodes("/codeSources/codeSource")?.Count, Is.EqualTo(2));
                    break;
                case ExportFormats.Yaml:
                    var yaml = new YamlStream();
                    yaml.Load(new StringReader(text));
                    Assert.That(yaml.Documents.Count, Is.EqualTo(1));
                    break;
                case ExportFormats.Html:
                    Assert.That(text.TrimEnd(), Does.EndWith("</html>"));
                    break;
                case ExportFormats.Markdown:
                    Assert.That(text.TrimEnd(), Does.EndWith("---"));
                    break;
                case ExportFormats.Csv:
                    Assert.That(text, Does.EndWith(Environment.NewLine));
                    Assert.That(text.TrimEnd(), Does.EndWith("\""));
                    break;
            }
        }

        private sealed class TrackingMemoryStream : MemoryStream
        {
            public bool DisposeCalled { get; private set; }

            protected override void Dispose(bool disposing)
            {
                DisposeCalled = true;
                base.Dispose(disposing);
            }
        }

        private sealed class WriteOnlyStream : Stream
        {
            private readonly MemoryStream _buffer = new MemoryStream();

            public bool DisposeCalled { get; private set; }

            public byte[] Written => _buffer.ToArray();

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => !DisposeCalled;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (DisposeCalled)
                    throw new ObjectDisposedException(nameof(WriteOnlyStream));

                _buffer.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                DisposeCalled = true;
                base.Dispose(disposing);
            }
        }
    }
}
