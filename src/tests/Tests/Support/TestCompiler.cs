using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class TestCompiler
    {
        internal const string DefaultAssemblyName = "Generated";

        internal const string NetStandardAssemblyName = "netstandard";

        internal static readonly string CodeSourceAssemblyName = typeof(CodeSourceAttribute).Assembly.GetName().Name;

        private const string ReferenceFolderName = "ScanFailRefs";

        private const string NetStandardFileName = NetStandardAssemblyName + ".dll";

        private static readonly Version NetStandardVersion = new Version(2, 1, 0, 0);

        private static readonly CSharpParseOptions WithoutSymbol = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.CSharp9)
            .WithDocumentationMode(DocumentationMode.Diagnose);

        private static readonly CSharpParseOptions WithSymbol = WithoutSymbol
            .WithPreprocessorSymbols(CodeSourceAttribute.ConditionalSymbol);

        private static readonly Lazy<MetadataReference> NetStandardReference = new Lazy<MetadataReference>(
            () => MetadataReference.CreateFromFile(NetStandardPath()),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<MetadataReference> CodeSourceReference = new Lazy<MetadataReference>(
            () => MetadataReference.CreateFromFile(typeof(CodeSourceAttribute).Assembly.Location),
            LazyThreadSafetyMode.ExecutionAndPublication);

        internal static MetadataReference NetStandard => NetStandardReference.Value;

        internal static MetadataReference CodeSource => CodeSourceReference.Value;

        internal static CSharpCompilation Compile(bool defineCodeSource, params string[] sources)
            => Compile(DefaultAssemblyName, defineCodeSource, new[] { NetStandard, CodeSource }, sources);

        internal static CSharpCompilation Compile(string assemblyName, bool defineCodeSource,
            IEnumerable<MetadataReference> references, params string[] sources)
        {
            var parseOptions = defineCodeSource ? WithSymbol : WithoutSymbol;

            return CSharpCompilation.Create(
                assemblyName,
                sources.Select(source => CSharpSyntaxTree.ParseText(source, parseOptions)),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));
        }

        internal static byte[] Emit(CSharpCompilation compilation)
        {
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            if (!result.Success)
                throw new InvalidOperationException($"{compilation.AssemblyName} failed to compile:{Describe(result.Diagnostics)}");

            return image.ToArray();
        }

        internal static List<string> DiagnosticIds(IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
            => diagnostics.Where(diagnostic => diagnostic.Severity == severity).Select(diagnostic => diagnostic.Id).Distinct().ToList();

        internal static string Describe(IEnumerable<Diagnostic> diagnostics)
            => Environment.NewLine + string.Join(Environment.NewLine, diagnostics.Select(diagnostic => diagnostic.ToString()));

        private static string NetStandardPath()
        {
            var path = Path.Combine(AppContext.BaseDirectory, ReferenceFolderName, NetStandardFileName);
            if (!File.Exists(path))
                throw new InvalidOperationException(
                    $"The netstandard 2.1 reference assembly was not found at '{path}'. Tests.csproj copies it from the NETStandard.Library.Ref 2.1.0 package; rebuild the test project.");

            var version = AssemblyName.GetAssemblyName(path).Version;
            if (version != NetStandardVersion)
                throw new InvalidOperationException(
                    $"'{path}' is netstandard {version}; the tests compile against netstandard {NetStandardVersion}. Rebuild the test project.");

            return path;
        }
    }
}
