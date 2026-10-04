using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using RzR.Core.CodeSource;
using Tests.Support;

namespace Tests.Conditional
{
    [TestFixture]
    public class ConditionalEmissionTests
    {
        private const string TargetTypeName = "Cond.Target";

        private const string TypeofSource =
            "using System;\n" +
            "using RzR.Core.CodeSource;\n" +
            "namespace Cond.Typeof\n" +
            "{\n" +
            "    public class Holder\n" +
            "    {\n" +
            "        public Type Read() => typeof(CodeSourceAttribute);\n" +
            "    }\n" +
            "}\n";

        private const string NameofCrefUsingSource =
            "using RzR.Core.CodeSource;\n" +
            "namespace Cond.Names\n" +
            "{\n" +
            "/// <summary>See <see cref=\"CodeSourceAttribute\"/>.</summary>\n" +
            "    [CodeSource(\"u\")]\n" +
            "    public class Holder\n" +
            "    {\n" +
            "/// <summary>The attribute name.</summary>\n" +
            "        public const string Name = nameof(CodeSourceAttribute);\n" +
            "    }\n" +
            "}\n";

        private const string BadCrefSource =
            "using RzR.Core.CodeSource;\n" +
            "namespace Cond.BadCref\n" +
            "{\n" +
            "/// <summary>See <see cref=\"NoSuchType\"/>.</summary>\n" +
            "    public class Holder { }\n" +
            "}\n";

        private static readonly string[] NetStandardOnly = { TestCompiler.NetStandardAssemblyName };

        private static readonly string[] NetStandardAndCodeSource = { TestCompiler.NetStandardAssemblyName, TestCompiler.CodeSourceAssemblyName };

        [Test]
        public void AllTargets_SymbolDefined_EmitsEveryRowAndTheAssemblyReference()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(true, ConditionalSources.AllTargetsSource));

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.EquivalentTo(ConditionalSources.AllTargetsUrls));
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardAndCodeSource));
            });
        }

        [Test]
        public void AllTargets_SymbolUndefined_OmitsEveryRowAndTheAssemblyReference()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(false, ConditionalSources.AllTargetsSource));

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.Empty);
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardOnly));
            });
        }

        [Test]
        public void StackedUsages_SymbolDefined_EmitsEveryApplicationWithItsValues()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(true, ClassWith(
                "[CodeSource(\"u1\", Version = \"1\")]",
                "[CodeSource(\"u2\", Version = \"2\")]",
                "[CodeSource(\"u3\", Version = \"3\")]")));

            var instances = Assembly.Load(image).GetType(TargetTypeName, true)
                .GetCustomAttributes<CodeSourceAttribute>()
                .Select(instance => $"{instance.SourceUrl}|{instance.Version}");

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.EquivalentTo(new[] { "u1", "u2", "u3" }));
                Assert.That(instances, Is.EquivalentTo(new[] { "u1|1", "u2|2", "u3|3" }));
            });
        }

        [Test]
        public void PerFileDefine_ProjectUndefined_EmitsOnlyThatFile()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(false,
                FileWith("#define CODESOURCE", "FileA"),
                FileWith(string.Empty, "FileB")));

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.EquivalentTo(new[] { "u/FileA" }));
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardAndCodeSource));
            });
        }

        [Test]
        public void PerFileUndef_ProjectDefined_SuppressesOnlyThatFile()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(true,
                FileWith(string.Empty, "FileA"),
                FileWith("#undef CODESOURCE", "FileB")));

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.EquivalentTo(new[] { "u/FileA" }));
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardAndCodeSource));
            });
        }

        [TestCase("codesource")]
        [TestCase("CodeSource")]
        [TestCase("Codesource")]
        public void OtherCasingSymbol_Defined_EmitsNoRowAndNoAssemblyReference(string symbol)
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(false, FileWith("#define " + symbol, "Target")));

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.Empty);
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardOnly));
            });
        }

        [TestCase("[CodeSource(\"u\", \"v\")]", TestName = "{m}(TwoPositional)")]
        [TestCase("[CodeSource(\"u\", \"a\", \"v\")]", TestName = "{m}(ThreePositional)")]
        public void ObsoleteConstructor_SymbolUndefined_StillWarnsCS0618(string usage)
        {
            var compilation = TestCompiler.Compile(false, ClassWith(usage));

            var image = TestCompiler.Emit(compilation);
            var diagnostics = compilation.GetDiagnostics();

            Assert.Multiple(() =>
            {
                Assert.That(TestCompiler.DiagnosticIds(diagnostics, DiagnosticSeverity.Warning), Has.Member("CS0618"),
                    TestCompiler.Describe(diagnostics));
                Assert.That(PeImage.CodeSourceUrls(image), Is.Empty);
            });
        }

        [TestCase("[CodeSource(\"1\", \"2\", \"3\", \"4\", \"5\", \"6\", \"7\", \"8\", \"9\")]", "CS1729", TestName = "{m}(NineArguments)")]
        [TestCase("[CodeSource(42)]", "CS1503", TestName = "{m}(WrongArgumentType)")]
        [TestCase("[CodeSource(\"u\", NoSuchProp = \"x\")]", "CS0246", TestName = "{m}(UnknownNamedArgument)")]
        public void OmittedUsage_InvalidApplication_StillReportsCompilerError(string usage, string expectedErrorId)
        {
            var diagnostics = TestCompiler.Compile(false, ClassWith(usage)).GetDiagnostics();

            Assert.That(TestCompiler.DiagnosticIds(diagnostics, DiagnosticSeverity.Error), Is.EquivalentTo(new[] { expectedErrorId }),
                TestCompiler.Describe(diagnostics));
        }

        [Test]
        public void OmittedUsage_WithoutCodeSourceReference_ReportsCS0246()
        {
            var diagnostics = TestCompiler.Compile(TestCompiler.DefaultAssemblyName, false, new[] { TestCompiler.NetStandard },
                ClassWith("[CodeSource(\"u\")]")).GetDiagnostics();

            Assert.That(TestCompiler.DiagnosticIds(diagnostics, DiagnosticSeverity.Error), Is.EquivalentTo(new[] { "CS0246" }),
                TestCompiler.Describe(diagnostics));
        }

        [Test]
        public void TypeofCodeSourceAttribute_SymbolUndefined_KeepsTheAssemblyReference()
        {
            var image = TestCompiler.Emit(TestCompiler.Compile(false, TypeofSource));

            Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardAndCodeSource));
        }

        [Test]
        public void NameofCrefAndUsing_SymbolUndefined_AddNoAssemblyReference()
        {
            var compilation = TestCompiler.Compile(false, NameofCrefUsingSource);

            var image = TestCompiler.Emit(compilation);
            var holder = Assembly.Load(image).GetType("Cond.Names.Holder", true);
            var diagnostics = compilation.GetDiagnostics();

            Assert.Multiple(() =>
            {
                Assert.That(TestCompiler.DiagnosticIds(diagnostics, DiagnosticSeverity.Warning), Has.No.Member("CS1574"),
                    TestCompiler.Describe(diagnostics));
                Assert.That(holder.GetField("Name")?.GetRawConstantValue(), Is.EqualTo(nameof(CodeSourceAttribute)));
                Assert.That(PeImage.CodeSourceUrls(image), Is.Empty);
                Assert.That(PeImage.AssemblyReferenceNames(image), Is.EquivalentTo(NetStandardOnly));
            });
        }

        [Test]
        public void UnresolvedCref_DiagnoseMode_IsReportedAsCS1574()
        {
            var diagnostics = TestCompiler.Compile(false, BadCrefSource).GetDiagnostics();

            Assert.That(TestCompiler.DiagnosticIds(diagnostics, DiagnosticSeverity.Warning), Has.Member("CS1574"),
                TestCompiler.Describe(diagnostics));
        }

        private static string ClassWith(params string[] attributeUsages)
            => "using RzR.Core.CodeSource; namespace Cond { " + string.Join(" ", attributeUsages) + " public class Target { } }";

        private static string FileWith(string directive, string className)
            => directive + "\nusing RzR.Core.CodeSource; namespace Cond { [CodeSource(\"u/" + className + "\")] public class " + className + " { } }";
    }
}
