using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class CodeSourceBuildOutput
    {
        private const string BinRootKey = "CodeSourceBinRoot";

        private const string ProjectFileName = "CodeSource.csproj";

        private const string TargetFrameworksElement = "TargetFrameworks";

        private const string LoadedTargetFramework = "netstandard2.1";

        private const string BuildHint = "Build src/CodeSource/CodeSource.csproj for all target frameworks.";

        private static readonly string ImageFileName = TestCompiler.CodeSourceAssemblyName + ".dll";

        internal static readonly string BinRoot = typeof(CodeSourceBuildOutput).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == BinRootKey)
            .Value;

        internal static readonly string[] TargetFrameworks =
        {
            "net40",
            "net45",
            "netstandard1.0",
            "netstandard1.5",
            "netstandard2.0",
            "netstandard2.1"
        };

        internal static string ProjectPath => Path.GetFullPath(Path.Combine(BinRoot, "..", "..", ProjectFileName));

        internal static List<string> ProjectTargetFrameworks()
        {
            using var reader = XmlReader.Create(ProjectPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

            return XDocument.Load(reader)
                .Descendants(TargetFrameworksElement)
                .Single()
                .Value
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(targetFramework => targetFramework.Trim())
                .ToList();
        }

        internal static byte[] ReadImage(string targetFramework)
        {
            var path = ImagePath(targetFramework);
            if (!File.Exists(path))
                Assert.Fail($"'{path}' was not found. {BuildHint}");

            return File.ReadAllBytes(path);
        }

        internal static void AssertBinRootIsFresh()
        {
            var loadedMvid = typeof(CodeSourceAttribute).Assembly.ManifestModule.ModuleVersionId;
            using var peReader = new PEReader(new MemoryStream(ReadImage(LoadedTargetFramework), false));
            var metadata = peReader.GetMetadataReader();
            var onDiskMvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid);

            Assert.That(onDiskMvid, Is.EqualTo(loadedMvid),
                $"'{ImagePath(LoadedTargetFramework)}' is not the {TestCompiler.CodeSourceAssemblyName} the tests loaded. {BuildHint}");
        }

        private static string ImagePath(string targetFramework)
            => Path.Combine(BinRoot, targetFramework, ImageFileName);
    }
}
