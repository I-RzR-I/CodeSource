using System;
using System.IO;

namespace Tests.Support
{
    internal static class TempFiles
    {
        internal static string NewPath(string extension)
            => Path.Combine(Path.GetTempPath(), $"codesource-{Guid.NewGuid():N}.{extension.ToLowerInvariant()}");
    }
}
