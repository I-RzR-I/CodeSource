using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using TempLib.NoSymbol;
using Tests.Support;

namespace Tests.Conditional
{
    [TestFixture]
    public class ConditionalScannerTests
    {
        private static readonly Assembly NoSymbolAssembly = typeof(NoSymbolAnnotated).Assembly;

        private static readonly string[] ReportedEntries =
        {
            "Cond.AllTargets.AnnotatedClass|u/class",
            "Cond.AllTargets.AnnotatedClass/.ctor|u/ctor",
            "Cond.AllTargets.AnnotatedClass/Method|u/method",
            "Cond.AllTargets.AnnotatedStruct|u/struct",
            "Cond.AllTargets.IAnnotated|u/interface",
            "Cond.AllTargets.AnnotatedEnum|u/enum",
            "Cond.AllTargets.AnnotatedDelegate|u/delegate"
        };

        private static readonly string[] ReportedParents =
        {
            "Cond.AllTargets.AnnotatedClass",
            "Cond.AllTargets.AnnotatedStruct",
            "Cond.AllTargets.IAnnotated",
            "Cond.AllTargets.AnnotatedEnum",
            "Cond.AllTargets.AnnotatedDelegate"
        };

        private Assembly _withSymbol;

        [OneTimeSetUp]
        public void CompileAllTargets()
        {
            _withSymbol = Assembly.Load(TestCompiler.Emit(TestCompiler.Compile(true, ConditionalSources.AllTargetsSource)));
        }

        [Test]
        public void Scanner_SymbolDefinedBuild_ReportsPublicTypesConstructorsAndMethods()
        {
            var errors = new List<CodeSourceScanError>();

            var results = ScanRunner.Scan(_withSymbol, errors);

            Assert.Multiple(() =>
            {
                Assert.That(errors, Is.Empty, ScanRunner.DescribeErrors(errors));
                Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(ReportedParents));
                Assert.That(Flatten(results), Is.EquivalentTo(ReportedEntries));
            });
        }

        [Test]
        public void Scanner_NoSymbolBuild_ReturnsNoResultsAndNoErrors()
        {
            var errors = new List<CodeSourceScanError>();

            var results = ScanRunner.Scan(NoSymbolAssembly, errors);

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.Empty);
                Assert.That(errors, Is.Empty, ScanRunner.DescribeErrors(errors));
            });
        }

        [Test]
        public void Scanner_AssemblyListWithAndWithoutSymbol_ReportsOnlySymbolEntries()
        {
            var errors = new List<CodeSourceScanError>();

            var results = ScanRunner.Scan(new[] { NoSymbolAssembly, _withSymbol }, errors);

            Assert.Multiple(() =>
            {
                Assert.That(errors, Is.Empty, ScanRunner.DescribeErrors(errors));
                Assert.That(Flatten(results), Is.EquivalentTo(ReportedEntries));
            });
        }

        [Test]
        public void ReflectionEmit_CustomAttributeBuilder_IgnoresConditional()
        {
            var type = DynamicAssemblies.Define("Cond_ReflectionEmit", "Cond.Emitted.Target", "u/emit").Type;

            var sourceUrls = type.GetCustomAttributes<CodeSourceAttribute>().Select(instance => instance.SourceUrl);

            Assert.That(sourceUrls, Is.EqualTo(new[] { "u/emit" }));
        }

        private static List<string> Flatten(IEnumerable<CodeSourceObjectsResult> results)
            => results
                .SelectMany(result => result.Parent.History.Select(history => $"{result.Parent.FullName}|{history.SourceUrl}")
                    .Concat((result.Children ?? Enumerable.Empty<CodeSourceObject>()).SelectMany(child =>
                        child.History.Select(history => $"{result.Parent.FullName}/{child.Name}|{history.SourceUrl}"))))
                .ToList();
    }
}
