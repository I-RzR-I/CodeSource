using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Scanning
{
    [TestFixture]
    [NonParallelizable]
    public class ScannerErrorReportingTests
    {
        private static CodeSourceScanner Scanner => CodeSourceScanner.Instance;

        [Test]
        public void FindAnnotations_DynamicAssemblyInList_ReportsTypeEnumerationAndScansTheOthers()
        {
            var errors = new List<CodeSourceScanError>();

            var results = ScanRunner.Scan(new Assembly[] { DynamicAssemblies.ScannerProbe, FixtureCatalog.TempLibAssembly }, errors);

            var dynamicErrors = errors.Where(error => error.AssemblyName != null && error.AssemblyName.Contains(DynamicAssemblies.ScannerProbeName)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(dynamicErrors, Has.Count.EqualTo(1), "one error for the dynamic assembly");
                Assert.That(dynamicErrors.Select(error => error.Stage), Is.All.EqualTo(CodeSourceScanStage.TypeEnumeration));
                Assert.That(dynamicErrors.Select(error => error.Exception), Is.All.InstanceOf<NotSupportedException>());
                Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(FixtureCatalog.TempLibParents), "the other assembly is still scanned");
            });
        }

        [TestCaseSource(nameof(ScanWithoutOnErrorCases))]
        public void FindAnnotations_DynamicAssemblyWithoutOnError_IsSkippedSilently(
            Func<IEnumerable<Assembly>, IEnumerable<CodeSourceObjectsResult>> scan)
        {
            var results = scan(new Assembly[] { DynamicAssemblies.ScannerProbe, FixtureCatalog.TempLibAssembly }).ToList();

            Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(FixtureCatalog.TempLibParents));
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOn_ReportsAttributeValueErrorsWithoutException()
        {
            var errors = new List<CodeSourceScanError>();

            ScanRunner.Scan(new[] { FixtureCatalog.FixturesAssembly }, errors);

            var valueErrors = errors.Where(error => error.Stage == CodeSourceScanStage.AttributeValue).ToList();
            Assert.Multiple(() =>
            {
                foreach (var invalid in FixtureCatalog.InvalidAppliedOnTypes)
                    Assert.That(valueErrors.Count(error => IsFor(error, invalid)), Is.EqualTo(1), invalid.Name);

                foreach (var valid in FixtureCatalog.ValidAppliedOnTypes)
                    Assert.That(valueErrors.Where(error => IsFor(error, valid)), Is.Empty, valid.Name);

                Assert.That(valueErrors.Select(error => error.Exception), Is.All.Null, "AttributeValue carries no exception");
            });
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOnOnMethod_ReportsTheMemberName()
        {
            var errors = new List<CodeSourceScanError>();

            ScanRunner.Scan(new[] { FixtureCatalog.FixturesAssembly }, errors);

            var memberError = errors.Single(error => error.Stage == CodeSourceScanStage.AttributeValue
                                                     && IsFor(error, typeof(BindingFixtures.InvalidAppliedOnOnMethod)));
            Assert.That(memberError.MemberName, Is.EqualTo(nameof(BindingFixtures.InvalidAppliedOnOnMethod.Run)));
        }

        [Test]
        public void FindAnnotations_OnErrorThrows_PropagatesAndAbortsTheScan()
        {
            var calls = 0;
            var options = new CodeSourceScanOptions
            {
                OnError = _ =>
                {
                    calls++;
                    throw new ScanAbortedException();
                }
            };

            Assert.Throws<ScanAbortedException>(
                () => Scanner.FindAnnotations(new Assembly[] { DynamicAssemblies.ScannerProbe, FixtureCatalog.FixturesAssembly }, options).ToList());
            Assert.That(calls, Is.EqualTo(1), "the scan stops at the first reported error");
        }

        [Test]
        public void ScanError_Message_HasNoStackTraceOrFileLocation()
        {
            var errors = new List<CodeSourceScanError>();

            ScanRunner.Scan(new Assembly[] { DynamicAssemblies.ScannerProbe, FixtureCatalog.FixturesAssembly }, errors);

            var fixturesFolder = Path.GetDirectoryName(FixtureCatalog.FixturesAssembly.Location);
            Assert.That(errors, Is.Not.Empty, "the scan must report errors for this test to be meaningful");
            Assert.Multiple(() =>
            {
                foreach (var error in errors)
                {
                    Assert.That(error.Message, Is.Not.Null.And.Not.Empty, error.Stage.ToString());
                    Assert.That(error.Message, Does.Not.Contain("   at "), "no stack frames");
                    Assert.That(error.Message, Does.Not.Contain("--- End of"), "no stack frames");
                    Assert.That(error.Message, Does.Not.Contain(fixturesFolder).IgnoreCase, "no Location");
                    Assert.That(error.Message, Does.Not.Contain("file:").IgnoreCase, "no CodeBase");
                    if (error.Exception != null)
                        Assert.That(error.Message, Does.Contain(error.Exception.GetType().Name), "names the exception type");
                }
            });
        }

        [Test]
        public void FindAnnotations_MemberWithUnresolvableAttribute_SkipsOnlyThatMember()
        {
            Assembly ResolveOnlyCodeSource(object sender, ResolveEventArgs args)
                => string.Equals(new AssemblyName(args.Name).Name, TestCompiler.CodeSourceAssemblyName, StringComparison.Ordinal)
                    ? typeof(CodeSourceAttribute).Assembly
                    : null;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveOnlyCodeSource;
            try
            {
                var target = Assembly.Load(ScanFailFixture.TargetImage);
                var errors = new List<CodeSourceScanError>();

                var results = ScanRunner.Scan(target, errors);

                var dump = ScanRunner.DescribeErrors(errors);
                var mixedResults = results.Where(result => result.Parent?.FullName == ScanFailFixture.MixedMembersTypeName).ToList();
                Assert.Multiple(() =>
                {
                    Assert.That(errors.Count(error => error.Stage == CodeSourceScanStage.AttributeRead
                                                      && error.TypeName == ScanFailFixture.MixedMembersTypeName
                                                      && error.MemberName == "Broken"), Is.EqualTo(1), "AttributeRead for the [MissingAttr] member" + dump);
                    Assert.That(errors.Any(error => error.Stage == CodeSourceScanStage.AssemblyLoad
                                                    && error.AssemblyName != null && error.AssemblyName.Contains(ScanFailFixture.StubAssemblyName)), Is.True,
                        "the unresolvable reference is reported" + dump);
                    Assert.That(errors.Select(error => SimpleAssemblyName(error.AssemblyName))
                            .Where(name => name != ScanFailFixture.TargetAssemblyName && name != ScanFailFixture.StubAssemblyName),
                        Is.Empty, "every error belongs to the fixture" + dump);
                    Assert.That(results.Where(result => result.Parent?.FullName == ScanFailFixture.OnlyBrokenMemberTypeName), Is.Empty,
                        "no partially built Parent for a type whose only annotated member failed");
                    Assert.That(mixedResults, Has.Count.EqualTo(1), "one result for the type with healthy members" + dump);
                });

                var mixed = mixedResults[0];
                Assert.Multiple(() =>
                {
                    Assert.That(mixed.Parent?.History, Is.Not.Null.And.Count.EqualTo(1), "Parent is complete (class-level history)");
                    Assert.That(mixed.Children?.Select(child => child.Name) ?? Enumerable.Empty<string>(),
                        Is.EquivalentTo(new[] { "Healthy", "AlsoHealthy" }));
                });
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveOnlyCodeSource;
            }
        }

        private static IEnumerable<TestCaseData> ScanWithoutOnErrorCases()
        {
            yield return NamedCase.Of("NoOptions",
                (Func<IEnumerable<Assembly>, IEnumerable<CodeSourceObjectsResult>>)(assemblies => Scanner.FindAnnotations(assemblies)));
            yield return NamedCase.Of("NullOptions",
                (Func<IEnumerable<Assembly>, IEnumerable<CodeSourceObjectsResult>>)(assemblies => Scanner.FindAnnotations(assemblies, null)));
            yield return NamedCase.Of("OptionsWithoutOnError",
                (Func<IEnumerable<Assembly>, IEnumerable<CodeSourceObjectsResult>>)(assemblies => Scanner.FindAnnotations(assemblies, new CodeSourceScanOptions())));
        }

        private static bool IsFor(CodeSourceScanError error, Type type)
            => error.TypeName == type.FullName;

        private static string SimpleAssemblyName(string displayName)
            => displayName?.Split(',')[0].Trim();

        private static class ScanFailFixture
        {
            internal const string TargetAssemblyName = "ScanFailTarget";

            internal const string StubAssemblyName = "ScanFailStub";

            internal const string MixedMembersTypeName = TargetAssemblyName + ".MixedMembers";

            internal const string OnlyBrokenMemberTypeName = TargetAssemblyName + ".OnlyBrokenMember";

            private const string StubSource =
                "using System;\n" +
                "namespace ScanFailStub\n" +
                "{\n" +
                "    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]\n" +
                "    public sealed class MissingAttr : Attribute\n" +
                "    {\n" +
                "    }\n" +
                "}\n";

            private const string MixedMembersSource =
                "using RzR.Core.CodeSource;\n" +
                "using ScanFailStub;\n" +
                "namespace ScanFailTarget\n" +
                "{\n" +
                "    [CodeSource(\"http://fixture.local/mixed\", \"fixture\", null, \"1.0\")]\n" +
                "    public class MixedMembers\n" +
                "    {\n" +
                "        [CodeSource(\"http://fixture.local/healthy\", \"fixture\", null, \"1.0\")]\n" +
                "        public void Healthy() { }\n" +
                "\n" +
                "        [CodeSource(\"http://fixture.local/also-healthy\", \"fixture\", null, \"1.0\")]\n" +
                "        public void AlsoHealthy() { }\n" +
                "\n" +
                "        [MissingAttr]\n" +
                "        [CodeSource(\"http://fixture.local/broken\", \"fixture\", null, \"1.0\")]\n" +
                "        public void Broken() { }\n" +
                "    }\n" +
                "}\n";

            private const string OnlyBrokenMemberSource =
                "using RzR.Core.CodeSource;\n" +
                "using ScanFailStub;\n" +
                "namespace ScanFailTarget\n" +
                "{\n" +
                "    public class OnlyBrokenMember\n" +
                "    {\n" +
                "        [MissingAttr]\n" +
                "        [CodeSource(\"http://fixture.local/only-broken\", \"fixture\", null, \"1.0\")]\n" +
                "        public void Broken() { }\n" +
                "    }\n" +
                "}\n";

            private static readonly Lazy<byte[]> Image = new Lazy<byte[]>(BuildTargetImage, LazyThreadSafetyMode.ExecutionAndPublication);

            internal static byte[] TargetImage => Image.Value;

            private static byte[] BuildTargetImage()
            {
                var stub = MetadataReference.CreateFromImage(TestCompiler.Emit(
                    TestCompiler.Compile(StubAssemblyName, false, new[] { TestCompiler.NetStandard }, StubSource)));

                var image = TestCompiler.Emit(TestCompiler.Compile(
                    TargetAssemblyName,
                    true,
                    new[] { TestCompiler.NetStandard, TestCompiler.CodeSource, stub },
                    MixedMembersSource,
                    OnlyBrokenMemberSource));
                EnsureExpectedReferences(image);

                return image;
            }

            private static void EnsureExpectedReferences(byte[] image)
            {
                var actual = PeImage.AssemblyReferenceNames(image).OrderBy(name => name, StringComparer.Ordinal).ToList();
                var expected = new[] { TestCompiler.NetStandardAssemblyName, TestCompiler.CodeSourceAssemblyName, StubAssemblyName }
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList();

                if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                    throw new InvalidOperationException(
                        $"{TargetAssemblyName} fixture references [{string.Join(", ", actual)}]; expected [{string.Join(", ", expected)}].");
            }
        }
    }
}
