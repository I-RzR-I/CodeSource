using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RzR.Core.CodeSource;

namespace Tests
{
    internal static class ScanFailFixture
    {
        internal const string TargetAssemblyName = "ScanFailTarget";

        internal const string StubAssemblyName = "ScanFailStub";

        private const string ReferenceFolderName = "ScanFailRefs";

        private const string NetStandardFileName = "netstandard.dll";

        private const string NetStandardAssemblyName = "netstandard";

        private const string StubSource = @"using System;

namespace ScanFailStub
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class MissingAttr : Attribute
    {
    }
}
";

        private const string MixedMembersSource = @"using RzR.Core.CodeSource;
using ScanFailStub;

namespace ScanFailTarget
{
    [CodeSource(""http://fixture.local/mixed"", ""fixture"", null, ""1.0"")]
    public class MixedMembers
    {
        [CodeSource(""http://fixture.local/healthy"", ""fixture"", null, ""1.0"")]
        public void Healthy() { }

        [CodeSource(""http://fixture.local/also-healthy"", ""fixture"", null, ""1.0"")]
        public void AlsoHealthy() { }

        [MissingAttr]
        [CodeSource(""http://fixture.local/broken"", ""fixture"", null, ""1.0"")]
        public void Broken() { }
    }
}
";

        private const string OnlyBrokenMemberSource = @"using RzR.Core.CodeSource;
using ScanFailStub;

namespace ScanFailTarget
{
    public class OnlyBrokenMember
    {
        [MissingAttr]
        [CodeSource(""http://fixture.local/only-broken"", ""fixture"", null, ""1.0"")]
        public void Broken() { }
    }
}
";

        private static readonly Lazy<byte[]> Image = new Lazy<byte[]>(BuildTargetImage, LazyThreadSafetyMode.ExecutionAndPublication);

        internal static byte[] TargetImage => Image.Value;

        private static byte[] BuildTargetImage()
        {
            var netStandard = MetadataReference.CreateFromFile(NetStandardPath());
            var codeSource = MetadataReference.CreateFromFile(typeof(CodeSourceAttribute).Assembly.Location);

            var stubCompilation = CreateCompilation(StubAssemblyName, new[] { StubSource }, netStandard);
            EnsureNoErrors(StubAssemblyName, stubCompilation.GetDiagnostics());
            var stub = stubCompilation.ToMetadataReference();

            var targetCompilation = CreateCompilation(
                TargetAssemblyName,
                new[] { MixedMembersSource, OnlyBrokenMemberSource },
                netStandard,
                codeSource,
                stub);

            using var image = new MemoryStream();
            var emitResult = targetCompilation.Emit(image);
            if (!emitResult.Success)
                throw new InvalidOperationException($"{TargetAssemblyName} fixture failed to compile:{Environment.NewLine}{Describe(emitResult.Diagnostics)}");

            var bytes = image.ToArray();
            EnsureExpectedReferences(bytes, typeof(CodeSourceAttribute).Assembly.GetName().Name);

            return bytes;
        }

        private static CSharpCompilation CreateCompilation(string assemblyName, IEnumerable<string> sources, params MetadataReference[] references)
            => CSharpCompilation.Create(
                assemblyName,
                sources.Select(source => CSharpSyntaxTree.ParseText(source)),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

        private static string NetStandardPath()
        {
            var path = Path.Combine(AppContext.BaseDirectory, ReferenceFolderName, NetStandardFileName);
            if (!File.Exists(path))
                throw new InvalidOperationException(
                    $"The netstandard 2.1 reference assembly was not found at '{path}'. Tests.csproj copies it from the NETStandard.Library.Ref 2.1.0 package; rebuild the test project.");

            return path;
        }

        private static void EnsureNoErrors(string assemblyName, IEnumerable<Diagnostic> diagnostics)
        {
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
                throw new InvalidOperationException($"{assemblyName} fixture failed to compile:{Environment.NewLine}{Describe(errors)}");
        }

        private static void EnsureExpectedReferences(byte[] image, string codeSourceName)
        {
            using var peReader = new PEReader(new MemoryStream(image));
            var metadata = peReader.GetMetadataReader();
            var actual = metadata.AssemblyReferences
                .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
            var expected = new[] { NetStandardAssemblyName, codeSourceName, StubAssemblyName }
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidOperationException(
                    $"{TargetAssemblyName} fixture references [{string.Join(", ", actual)}]; expected [{string.Join(", ", expected)}].");
        }

        private static string Describe(IEnumerable<Diagnostic> diagnostics)
            => string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));
    }
}
