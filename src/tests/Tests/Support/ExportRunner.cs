using System.Collections.Generic;
using System.IO;
using System.Text;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;

namespace Tests.Support
{
    internal static class ExportRunner
    {
        internal static byte[] Export(string format, IEnumerable<CodeSourceObjectsResult> items)
        {
            using var stream = new MemoryStream();
            BuiltInExporters.Create(format).Export(items, stream);

            return stream.ToArray();
        }

        internal static byte[] ExportViaRegistry(string format, IEnumerable<CodeSourceObjectsResult> items)
        {
            using var stream = new MemoryStream();
            ExporterRegistry.Export(format, items, stream);

            return stream.ToArray();
        }

        internal static string ExportText(string format, IEnumerable<CodeSourceObjectsResult> items)
            => Decode(Export(format, items));

        internal static string ExportTextViaRegistry(string format, IEnumerable<CodeSourceObjectsResult> items)
            => Decode(ExportViaRegistry(format, items));

        internal static string Decode(byte[] bytes)
        {
            using var reader = new StreamReader(new MemoryStream(bytes), new UTF8Encoding(false, true), true);

            return reader.ReadToEnd();
        }

        internal static bool HasBomAt(byte[] bytes, int offset)
            => bytes.Length >= offset + 3 && bytes[offset] == 0xEF && bytes[offset + 1] == 0xBB
               && bytes[offset + 2] == 0xBF;
    }
}
