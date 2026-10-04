using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class StreamOwnershipTests
    {
        [Test]
        public void Export_ToCallerStream_LeavesItOpenAndWritable(
            [ValueSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))] string format,
            [Values] bool viaRegistry)
        {
            using var stream = new TrackingMemoryStream();
            ExportToCallerStream(format, CodeSourceResults.Sample(), stream, viaRegistry);

            Assert.Multiple(() =>
            {
                Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                Assert.That(stream.CanWrite, Is.True, "CanWrite");
                Assert.That(stream.CanRead, Is.True, "CanRead");
            });

            var length = stream.Length;
            stream.WriteByte(0x0A);
            Assert.That(stream.Length, Is.EqualTo(length + 1));
        }

        [Test]
        public void Export_ToCallerStream_MatchesSavePathOutput(
            [ValueSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))] string format,
            [Values] bool viaRegistry)
        {
            var items = CodeSourceResults.Sample();
            var expected = ExportViaSavePath(format, items);

            using var stream = new MemoryStream();
            ExportToCallerStream(format, items, stream, viaRegistry);

            Assert.That(stream.ToArray(), Is.EqualTo(expected), "complete output, identical to the savePath overload");
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_SavePathOutput_IsACompleteDocument(string format)
        {
            var items = CodeSourceResults.Sample();
            var bytes = ExportViaSavePath(format, items);
            var text = ExportRunner.Decode(bytes);

            Assert.That(text, Does.Contain(CodeSourceResults.SampleLastValue), "the last value of the dataset must be present");

            switch (format)
            {
                case ExportFormats.Json:
                    using (var document = JsonDocument.Parse(text))
                        Assert.That(document.RootElement.GetArrayLength(), Is.EqualTo(items.Count));
                    break;
                case ExportFormats.Xml:
                    Assert.That(StrictXml.Load(bytes).SelectNodes("/codeSources/codeSource")?.Count, Is.EqualTo(items.Count));
                    break;
                case ExportFormats.Yaml:
                    Assert.That(YamlDocumentReader.LoadEntries(text), Has.Count.EqualTo(items.Sum(item => 1 + item.Children.Count)));
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
                default:
                    Assert.Fail($"No completeness check for {format}.");
                    break;
            }
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_ToWriteOnlyNonSeekableStream_LeavesItOpenAndComplete(string format)
        {
            var items = CodeSourceResults.Sample();
            var expected = ExportRunner.Export(format, items);

            using var stream = new WriteOnlyStream();
            BuiltInExporters.Create(format).Export(items, stream);

            Assert.Multiple(() =>
            {
                Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                Assert.That(stream.CanWrite, Is.True, "CanWrite");
                Assert.That(stream.Written, Is.EqualTo(expected), "complete output");
            });
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_ToPrefilledStream_WritesNoBomAtOffsetN(string format)
        {
            var items = CodeSourceResults.Sample();
            var prefix = Encoding.ASCII.GetBytes("PREFIX-0123456789\n");
            var reference = ExportRunner.Export(format, items);
            var referenceBody = ExportRunner.HasBomAt(reference, 0) ? reference.Skip(3).ToArray() : reference;

            using var stream = new MemoryStream();
            stream.Write(prefix, 0, prefix.Length);

            BuiltInExporters.Create(format).Export(items, stream);

            var bytes = stream.ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(bytes.Take(prefix.Length), Is.EqualTo(prefix), "prefix untouched");
                Assert.That(ExportRunner.HasBomAt(bytes, prefix.Length), Is.False, "no BOM in the middle of the stream");
                Assert.That(bytes.Skip(prefix.Length), Is.EqualTo(referenceBody), "document appended after the prefix");
            });
        }

        [TestCaseSource(nameof(BomPolicyCases))]
        public void Export_ToEmptyStream_KeepsBomPolicy(string format, bool expectBom)
        {
            var bytes = ExportRunner.Export(format, CodeSourceResults.Sample());

            Assert.That(ExportRunner.HasBomAt(bytes, 0), Is.EqualTo(expectBom));
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_ViaSavePath_ReleasesFileHandle(string format)
        {
            var path = TempFiles.NewPath(format);
            try
            {
                ExporterRegistry.Export(format, CodeSourceResults.Sample(), path);

                Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));
                Assert.That(() => new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose(),
                    Throws.Nothing, "the savePath overload must release its file handle");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_WhenEnumerationFails_CallerStreamStaysOpen(string format)
        {
            using var stream = new TrackingMemoryStream();
            Assert.Catch(() => BuiltInExporters.Create(format).Export(ThrowingItems(), stream));

            Assert.Multiple(() =>
            {
                Assert.That(stream.DisposeCalled, Is.False, "the exporter must not close or dispose the caller's stream");
                Assert.That(stream.CanWrite, Is.True, "CanWrite");
            });
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_NullStream_ThrowsArgumentNullException(string format)
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => BuiltInExporters.Create(format).Export(CodeSourceResults.Sample(), null));

            Assert.That(exception.ParamName, Is.EqualTo("outputStream"));
        }

        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_NullItems_ThrowsArgumentNullExceptionAndWritesNothing(string format)
        {
            using var stream = new TrackingMemoryStream();
            var exception = Assert.Throws<ArgumentNullException>(
                () => BuiltInExporters.Create(format).Export(null, stream));

            Assert.Multiple(() =>
            {
                Assert.That(exception.ParamName, Is.EqualTo("items"));
                Assert.That(stream.DisposeCalled, Is.False, "stream not disposed");
                Assert.That(stream.Length, Is.EqualTo(0), "nothing written");
            });
        }

        private static void ExportToCallerStream(string format, IEnumerable<CodeSourceObjectsResult> items,
            Stream stream, bool viaRegistry)
        {
            if (viaRegistry)
                ExporterRegistry.Export(format, items, stream);
            else
                BuiltInExporters.Create(format).Export(items, stream);
        }

        private static byte[] ExportViaSavePath(string format, IEnumerable<CodeSourceObjectsResult> items)
        {
            var path = TempFiles.NewPath(format);
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

        private static IEnumerable<CodeSourceObjectsResult> ThrowingItems()
        {
            yield return CodeSourceResults.Sample()[0];
            throw new InvalidOperationException("enumeration failure injected by the test");
        }

        private static IEnumerable<TestCaseData> BomPolicyCases()
            => BuiltInExporters.Formats.Select(format =>
                NamedCase.Of(format, format, BuiltInExporters.BomFormats.Contains(format)));

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
