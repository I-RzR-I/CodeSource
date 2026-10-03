using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using RzR.Core.CodeSource.Services.Export;
using TempLib.Fixtures;

namespace Tests
{
    [TestFixture]
    public class ScannerTests
    {
        private const string DynamicAssemblyName = "CodeSourceScannerTests.Dynamic";

        private static readonly string[] TempLibParents = { "TempLib.OwnClassData", "TempLib.TempClassData" };

        private static CodeSourceScanner Scanner => CodeSourceScanner.Instance;

        private static Assembly TempLibAssembly => typeof(TempLib.OwnClassData).Assembly;

        private static Assembly FixturesAssembly => typeof(BindingFixtures).Assembly;

        [Test]
        public void FindAnnotations_DynamicAssemblyInList_ReportsTypeEnumerationAndScansTheOthers()
        {
            var errors = new List<CodeSourceScanError>();
            var options = new CodeSourceScanOptions { OnError = errors.Add };

            var results = Scanner.FindAnnotations(new[] { CreateDynamicAssembly(), TempLibAssembly }, options).ToList();

            var dynamicErrors = errors.Where(e => e.AssemblyName != null && e.AssemblyName.Contains(DynamicAssemblyName)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(dynamicErrors, Has.Count.EqualTo(1), "one error for the dynamic assembly");
                Assert.That(dynamicErrors.Select(e => e.Stage), Is.All.EqualTo(CodeSourceScanStage.TypeEnumeration));
                Assert.That(dynamicErrors.Select(e => e.Exception), Is.All.InstanceOf<NotSupportedException>());
                Assert.That(ParentNames(results), Is.EquivalentTo(TempLibParents), "the other assembly is still scanned");
            });
        }

        [Test]
        public void FindAnnotations_DynamicAssemblyWithoutOptions_IsSkippedSilently()
        {
            var assemblies = new[] { CreateDynamicAssembly(), TempLibAssembly };

            var results = Scanner.FindAnnotations(assemblies).ToList();

            Assert.That(ParentNames(results), Is.EquivalentTo(TempLibParents));
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOn_ReportsAttributeValueErrorsWithoutException()
        {
            var errors = new List<CodeSourceScanError>();

            Scanner.FindAnnotations(new[] { FixturesAssembly }, new CodeSourceScanOptions { OnError = errors.Add }).ToList();

            var valueErrors = errors.Where(e => e.Stage == CodeSourceScanStage.AttributeValue).ToList();
            Assert.Multiple(() =>
            {
                foreach (var invalid in new[]
                         {
                             typeof(BindingFixtures.AppliedOnNotADate),
                             typeof(BindingFixtures.AppliedOnOutOfRange),
                             typeof(BindingFixtures.PropertiesInvalidAppliedOn),
                         })
                {
                    Assert.That(valueErrors.Count(e => IsFor(e, invalid)), Is.EqualTo(1), invalid.Name);
                }

                foreach (var valid in new[]
                         {
                             typeof(BindingFixtures.AppliedOnValid),
                             typeof(BindingFixtures.AppliedOnPadded),
                             typeof(BindingFixtures.PropertiesOnly),
                         })
                {
                    Assert.That(valueErrors.Where(e => IsFor(e, valid)), Is.Empty, valid.Name);
                }

                Assert.That(valueErrors.Select(e => e.Exception), Is.All.Null, "AttributeValue carries no exception");
            });
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOnOnMethod_ReportsTheMemberName()
        {
            var errors = new List<CodeSourceScanError>();

            Scanner.FindAnnotations(new[] { FixturesAssembly }, new CodeSourceScanOptions { OnError = errors.Add }).ToList();

            var memberError = errors.Single(e => e.Stage == CodeSourceScanStage.AttributeValue
                                                 && IsFor(e, typeof(BindingFixtures.InvalidAppliedOnOnMethod)));
            Assert.That(memberError.MemberName, Is.EqualTo(nameof(BindingFixtures.InvalidAppliedOnOnMethod.Run)));
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOn_KeepsTheHistoryWithoutADate()
        {
            var results = Scanner.FindAnnotations(new[] { FixturesAssembly }).ToList();

            var history = SingleParentHistory(results, typeof(BindingFixtures.AppliedOnNotADate));
            Assert.Multiple(() =>
            {
                Assert.That(history.AppliedOn, Is.Null);
                Assert.That(history.SourceUrl, Is.EqualTo("http://fixture.local/e-applied-text"));
            });
        }

        [Test]
        public void FindAnnotations_PaddedAppliedOn_ExportsTheTrimmedDate()
        {
            var results = Scanner.FindAnnotations(new[] { FixturesAssembly }).ToList();

            var history = SingleParentHistory(results, typeof(BindingFixtures.AppliedOnPadded));

            Assert.That(history.AppliedOn, Is.EqualTo(new DateTime(2022, 12, 12)));
        }

        [Test]
        public void FindAnnotations_ParameterlessAttribute_ExportsDefaultVersion()
        {
            var results = Scanner.FindAnnotations(new[] { FixturesAssembly }).ToList();

            var history = SingleParentHistory(results, typeof(BindingFixtures.Parameterless));

            Assert.That(history.Version, Is.EqualTo("1.0"));
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
                    throw new ScanAbortedByCallbackException();
                },
            };

            Assert.Throws<ScanAbortedByCallbackException>(
                () => Scanner.FindAnnotations(new[] { CreateDynamicAssembly(), FixturesAssembly }, options).ToList());
            Assert.That(calls, Is.EqualTo(1), "the scan stops at the first reported error");
        }

        [Test]
        public void ScanErrorMessage_HasNoStackTraceOrFileLocation()
        {
            var errors = new List<CodeSourceScanError>();
            var options = new CodeSourceScanOptions { OnError = errors.Add };

            Scanner.FindAnnotations(new[] { CreateDynamicAssembly(), FixturesAssembly }, options).ToList();

            var fixturesFolder = Path.GetDirectoryName(FixturesAssembly.Location);
            Assume.That(errors, Is.Not.Empty, "the scan must report errors for this test to be meaningful");
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
        public void FindAnnotations_AssemblyLoadedFromBytes_ReturnsItsAnnotations()
        {
            var fromBytes = Assembly.Load(File.ReadAllBytes(TempLibAssembly.Location));
            Assume.That(fromBytes, Is.Not.SameAs(TempLibAssembly));

            var results = Scanner.FindAnnotations(fromBytes).ToList();

            Assert.That(ParentNames(results), Is.EquivalentTo(TempLibParents));
        }

        [Test]
        public void FindAnnotations_ReferencesResolvingToAlreadySeenAssemblies_AreScannedOnceInOneScan()
        {
            var root = new ReferenceDuplicatingAssembly(
                TempLibAssembly,
                TempLibAssembly.GetName(),
                FixturesAssembly.GetName(),
                FixturesAssembly.GetName());

            var names = ParentNames(Scanner.FindAnnotations(root).ToList());

            Assert.Multiple(() =>
            {
                Assert.That(root.ReferencesWereRead, Is.True, "the scan used the root's own (duplicated) reference list");
                Assert.That(names, Is.Unique, "each assembly is scanned once within a single scan");
                Assert.That(names, Is.SupersetOf(TempLibParents), "the root is scanned");
                Assert.That(names, Has.Member(typeof(BindingFixtures.SourceUrlOnly).FullName), "the duplicated reference is scanned");
            });
        }

        [Test]
        public void FindAnnotations_ByName_ReturnsDistinctFullNames()
        {
            var results = Scanner.FindAnnotations(TempLibAssembly.GetName().Name).ToList();

            Assert.That(ParentNames(results), Is.Unique.And.EquivalentTo(TempLibParents));
        }

        [Test]
        public void FindAnnotations_NullElementInList_IsSkipped()
        {
            var results = Scanner.FindAnnotations(new Assembly[] { null, TempLibAssembly }).ToList();

            Assert.That(ParentNames(results), Is.EquivalentTo(TempLibParents));
        }

        [Test]
        public void FindAnnotations_NullOptionsOrNullOnError_SkipRecoverableErrorsSilently()
        {
            var assemblies = new[] { CreateDynamicAssembly(), TempLibAssembly };
            List<string> withNullOptions = null;
            List<string> withNullOnError = null;

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => withNullOptions = ParentNames(Scanner.FindAnnotations(assemblies, null).ToList()), "null options");
                Assert.DoesNotThrow(() => withNullOnError = ParentNames(Scanner.FindAnnotations(assemblies, new CodeSourceScanOptions()).ToList()),
                    "options without OnError");
            });
            Assert.Multiple(() =>
            {
                Assert.That(withNullOptions, Is.EquivalentTo(TempLibParents), "null options");
                Assert.That(withNullOnError, Is.EquivalentTo(TempLibParents), "options without OnError");
            });
        }

        [Test]
        public void FindAnnotations_NullAssembly_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((Assembly)null).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assembly"));
                Assert.That(() => Scanner.FindAnnotations((Assembly)null, new CodeSourceScanOptions()).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assembly"));
            });
        }

        [Test]
        public void FindAnnotations_NullAssemblyList_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((IEnumerable<Assembly>)null).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblies"));
                Assert.That(() => Scanner.FindAnnotations((IEnumerable<Assembly>)null, new CodeSourceScanOptions()).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblies"));
            });
        }

        [Test]
        public void FindAnnotations_NullAssemblyName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((string)null).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblyName"));
                Assert.That(() => Scanner.FindAnnotations((string)null, new CodeSourceScanOptions()).ToList(),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblyName"));
            });
        }

        [TestCase("")]
        [TestCase("   ")]
        public void FindAnnotations_EmptyOrWhitespaceAssemblyName_ThrowsArgumentException(string assemblyName)
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations(assemblyName).ToList(),
                    Throws.TypeOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblyName"));
                Assert.That(() => Scanner.FindAnnotations(assemblyName, new CodeSourceScanOptions()).ToList(),
                    Throws.TypeOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("assemblyName"));
            });
        }

        public enum PathForm
        {
            AbsoluteBackslash,
            AbsoluteForwardSlash,
            Relative,
        }

        [TestCase(PathForm.AbsoluteBackslash)]
        [TestCase(PathForm.AbsoluteForwardSlash)]
        [TestCase(PathForm.Relative)]
        public void FindAnnotations_PathLikeName_ThrowsAndNeverOpensTheFile(PathForm form)
        {
            var folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "cs-pathprobe-" + Guid.NewGuid().ToString("N")));
            var probe = Path.Combine(folder.FullName, "PathProbe.dll");
            File.WriteAllText(probe, "not an assembly");
            try
            {
                var name = form switch
                {
                    PathForm.AbsoluteBackslash => probe,
                    PathForm.AbsoluteForwardSlash => probe.Replace('\\', '/'),
                    _ => Path.GetRelativePath(Environment.CurrentDirectory, probe),
                };

                var thrown = Assert.Catch(() => Scanner.FindAnnotations(name).ToList());

                Assert.Multiple(() =>
                {
                    Assert.That(thrown, Is.InstanceOf<ArgumentException>()
                        .Or.InstanceOf<FileLoadException>()
                        .Or.InstanceOf<FileNotFoundException>(), thrown?.ToString());
                    Assert.That(LoadedAssemblyLocations(), Has.None.EqualTo(probe).IgnoreCase, "the probe must never be loaded");
                });
            }
            finally
            {
                folder.Delete(recursive: true);
            }
        }

        [Test]
        public void FindAnnotations_MemberWithUnresolvableAttribute_SkipsOnlyThatMember()
        {
            var codeSourceAssembly = typeof(CodeSourceAttribute).Assembly;
            Assembly ResolveOnlyCodeSource(object sender, ResolveEventArgs args)
                => string.Equals(new AssemblyName(args.Name).Name, codeSourceAssembly.GetName().Name, StringComparison.Ordinal)
                    ? codeSourceAssembly
                    : null;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveOnlyCodeSource;
            try
            {
                var target = Assembly.Load(ScanFailFixture.TargetImage);
                var errors = new List<CodeSourceScanError>();

                var results = Scanner.FindAnnotations(target, new CodeSourceScanOptions { OnError = errors.Add }).ToList();

                var dump = DescribeErrors(errors);
                var mixedResults = results.Where(r => r.Parent?.FullName == "ScanFailTarget.MixedMembers").ToList();
                var mixed = mixedResults.FirstOrDefault();
                Assert.Multiple(() =>
                {
                    Assert.That(errors.Count(e => e.Stage == CodeSourceScanStage.AttributeRead
                                                  && e.TypeName == "ScanFailTarget.MixedMembers"
                                                  && e.MemberName == "Broken"), Is.EqualTo(1), "AttributeRead for the [MissingAttr] member" + dump);
                    Assert.That(errors.Any(e => e.Stage == CodeSourceScanStage.AssemblyLoad
                                                && e.AssemblyName != null && e.AssemblyName.Contains("ScanFailStub")), Is.True,
                        "the unresolvable reference is reported" + dump);
                    Assert.That(errors.Select(e => SimpleAssemblyName(e.AssemblyName))
                            .Where(name => name != ScanFailFixture.TargetAssemblyName && name != ScanFailFixture.StubAssemblyName),
                        Is.Empty, "every error belongs to the fixture" + dump);
                    Assert.That(mixedResults, Has.Count.EqualTo(1), "one result for the type with healthy members" + dump);
                    Assert.That(mixed, Is.Not.Null, "the type with healthy members is still returned");
                    Assert.That(mixed?.Parent?.History, Is.Not.Null.And.Count.EqualTo(1), "Parent is complete (class-level history)");
                    Assert.That(mixed?.Children?.Select(c => c.Name) ?? Enumerable.Empty<string>(),
                        Is.EquivalentTo(new[] { "Healthy", "AlsoHealthy" }));
                    Assert.That(results.Where(r => r.Parent?.FullName == "ScanFailTarget.OnlyBrokenMember"), Is.Empty,
                        "no partially built Parent for a type whose only annotated member failed");
                });
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveOnlyCodeSource;
            }
        }

        [Test]
        public void Instance_IsAStaticReadOnlyField()
        {
            var field = typeof(CodeSourceScanner).GetField(nameof(CodeSourceScanner.Instance), BindingFlags.Public | BindingFlags.Static);

            Assert.Multiple(() =>
            {
                Assert.That(field, Is.Not.Null, "Instance stays a field (a property would break compiled readers)");
                Assert.That(field?.IsInitOnly, Is.True, "Instance must not be assignable");
                Assert.That(field?.FieldType, Is.EqualTo(typeof(CodeSourceScanner)));
            });
        }

        [Test]
        [NonParallelizable]
        public void GetRegisteredFormats_ContainsExactlyTheSixBuiltIns()
        {
            var formats = ExporterRegistry.GetRegisteredFormats().Select(f => f.ToUpperInvariant());

            Assert.That(formats, Is.EquivalentTo(new[]
            {
                ExportFormats.Csv, ExportFormats.Html, ExportFormats.Json,
                ExportFormats.Markdown, ExportFormats.Xml, ExportFormats.Yaml,
            }));
        }

        [Test]
        [NonParallelizable]
        public void Register_SameFormatTwice_LastRegistrationWins()
        {
            try
            {
                ExporterRegistry.Register(new MarkerExporter(ExportFormats.Json, "first"));
                ExporterRegistry.Register(new MarkerExporter("json", "second"));

                var output = ExportToString(ExportFormats.Json);

                Assert.That(output, Is.EqualTo("second"));
            }
            finally
            {
                ExporterRegistry.Register(new JsonExporter());
            }
        }

        [Test]
        [NonParallelizable]
        public void Unregister_DifferentCase_RemovesTheFormat()
        {
            try
            {
                var removed = ExporterRegistry.Unregister("json");

                Assert.Multiple(() =>
                {
                    Assert.That(removed, Is.True);
                    Assert.That(ExporterRegistry.GetRegisteredFormats()
                        .Any(f => string.Equals(f, ExportFormats.Json, StringComparison.OrdinalIgnoreCase)), Is.False);
                });
            }
            finally
            {
                ExporterRegistry.Register(new JsonExporter());
            }
        }

        [Test]
        [NonParallelizable]
        public void Export_DifferentCaseFormat_UsesTheBuiltInExporter()
        {
            var output = ExportToString("json");

            Assert.That(output.TrimStart('\uFEFF').TrimStart(), Does.StartWith("["));
        }

        [Test]
        public void Export_NullFormatToStream_ThrowsArgumentNullExceptionBeforeWriting()
        {
            using var stream = new MemoryStream();

            Assert.Multiple(() =>
            {
                Assert.That(() => ExporterRegistry.Export(null, new List<CodeSourceObjectsResult>(), stream),
                    Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("format"));
                Assert.That(stream.Length, Is.EqualTo(0), "nothing is written before the guard");
            });
        }

        private static AssemblyBuilder CreateDynamicAssembly()
        {
            var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(DynamicAssemblyName), AssemblyBuilderAccess.Run);
            builder.DefineDynamicModule("Main").DefineType("DynamicType", TypeAttributes.Public).CreateType();

            return builder;
        }

        private static List<string> ParentNames(IEnumerable<CodeSourceObjectsResult> results)
            => results.Select(r => r.Parent?.FullName).ToList();

        private static bool IsFor(CodeSourceScanError error, Type type)
            => error.TypeName == type.FullName;

        private static string SimpleAssemblyName(string displayName)
            => displayName?.Split(',')[0].Trim();

        private static string DescribeErrors(IEnumerable<CodeSourceScanError> errors)
            => Environment.NewLine + "Reported errors:" + string.Concat(errors.Select(e =>
                $"{Environment.NewLine}  {e.Stage} | {e.AssemblyName} | {e.TypeName} | {e.MemberName} | {e.Exception?.GetType().FullName}"));

        private static CodeSourceObjectHistory SingleParentHistory(IEnumerable<CodeSourceObjectsResult> results, Type fixture)
            => results.Single(r => r.Parent?.FullName == fixture.FullName).Parent.History.Single();

        private static IEnumerable<string> LoadedAssemblyLocations()
            => AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);

        private static string ExportToString(string format)
        {
            using var stream = new MemoryStream();
            ExporterRegistry.Export(format, new List<CodeSourceObjectsResult>(), stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private sealed class ReferenceDuplicatingAssembly : Assembly
        {
            private readonly Assembly _inner;
            private readonly AssemblyName[] _extraReferences;

            public ReferenceDuplicatingAssembly(Assembly inner, params AssemblyName[] extraReferences)
            {
                _inner = inner;
                _extraReferences = extraReferences;
            }

            public override string FullName => _inner.FullName;

            public override string Location => _inner.Location;

            public override Module ManifestModule => _inner.ManifestModule;

            public override bool IsDynamic => _inner.IsDynamic;

            public override bool ReflectionOnly => _inner.ReflectionOnly;

            public override IEnumerable<Type> ExportedTypes => _inner.ExportedTypes;

            public override IEnumerable<TypeInfo> DefinedTypes => _inner.DefinedTypes;

            public override AssemblyName GetName() => _inner.GetName();

            public override AssemblyName GetName(bool copiedName) => _inner.GetName(copiedName);

            public bool ReferencesWereRead { get; private set; }

            public override AssemblyName[] GetReferencedAssemblies()
            {
                ReferencesWereRead = true;
                return _inner.GetReferencedAssemblies().Concat(_extraReferences).ToArray();
            }

            public override Type[] GetExportedTypes() => _inner.GetExportedTypes();

            public override Type[] GetTypes() => _inner.GetTypes();

            public override Type GetType(string name, bool throwOnError, bool ignoreCase) => _inner.GetType(name, throwOnError, ignoreCase);

            public override object[] GetCustomAttributes(bool inherit) => _inner.GetCustomAttributes(inherit);

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _inner.GetCustomAttributes(attributeType, inherit);

            public override IList<CustomAttributeData> GetCustomAttributesData() => _inner.GetCustomAttributesData();

            public override bool IsDefined(Type attributeType, bool inherit) => _inner.IsDefined(attributeType, inherit);

            public override string ToString() => _inner.ToString();

            public override bool Equals(object o) => ReferenceEquals(this, o);

            public override int GetHashCode() => _inner.GetHashCode();
        }

        private sealed class MarkerExporter : ICodeSourceExporter
        {
            private readonly byte[] _marker;

            public MarkerExporter(string format, string marker)
            {
                Format = format;
                _marker = Encoding.UTF8.GetBytes(marker);
            }

            public string Format { get; }

            public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
                => outputStream.Write(_marker, 0, _marker.Length);
        }

        private sealed class ScanAbortedByCallbackException : Exception
        {
            public ScanAbortedByCallbackException()
                : base("OnError asked the scan to stop.")
            {
            }
        }
    }
}
