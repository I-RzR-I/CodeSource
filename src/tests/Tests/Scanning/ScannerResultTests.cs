using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Scanning
{
    [TestFixture]
    public class ScannerResultTests
    {
        private static CodeSourceScanner Scanner => CodeSourceScanner.Instance;

        [Test]
        public void FindAnnotations_LoadedAssembly_ReturnsItsAnnotatedParents()
        {
            var results = Scanner.FindAnnotations(FixtureCatalog.TempLibAssembly).ToList();

            Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(FixtureCatalog.TempLibParents));
        }

        [Test]
        public void FindAnnotations_ByName_ReturnsDistinctFullNames()
        {
            var results = Scanner.FindAnnotations(FixtureCatalog.TempLibAssembly.GetName().Name).ToList();

            Assert.That(ScanRunner.ParentNames(results), Is.Unique.And.EquivalentTo(FixtureCatalog.TempLibParents));
        }

        [Test]
        public void FindAnnotations_AssemblyLoadedFromBytes_ReturnsItsAnnotations()
        {
            var fromBytes = Assembly.Load(PeImage.Read(FixtureCatalog.TempLibAssembly));
            Assert.That(fromBytes, Is.Not.SameAs(FixtureCatalog.TempLibAssembly), "the bytes must load as a separate assembly");

            var results = Scanner.FindAnnotations(fromBytes).ToList();

            Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(FixtureCatalog.TempLibParents));
        }

        [Test]
        public void FindAnnotations_NullElementInList_IsSkipped()
        {
            var results = Scanner.FindAnnotations(new Assembly[] { null, FixtureCatalog.TempLibAssembly }).ToList();

            Assert.That(ScanRunner.ParentNames(results), Is.EquivalentTo(FixtureCatalog.TempLibParents));
        }

        [Test]
        public void FindAnnotations_ReferencesResolvingToAlreadySeenAssemblies_AreScannedOnceInOneScan()
        {
            var root = new ReferenceDuplicatingAssembly(
                FixtureCatalog.TempLibAssembly,
                FixtureCatalog.TempLibAssembly.GetName(),
                FixtureCatalog.FixturesAssembly.GetName(),
                FixtureCatalog.FixturesAssembly.GetName());

            var names = ScanRunner.ParentNames(Scanner.FindAnnotations(root).ToList());

            Assert.Multiple(() =>
            {
                Assert.That(root.ReferencesWereRead, Is.True, "the scan used the root's own (duplicated) reference list");
                Assert.That(names, Is.Unique, "each assembly is scanned once within a single scan");
                Assert.That(names, Is.SupersetOf(FixtureCatalog.TempLibParents), "the root is scanned");
                Assert.That(names, Has.Member(typeof(BindingFixtures.SourceUrlOnly).FullName), "the duplicated reference is scanned");
            });
        }

        [Test]
        public void FindAnnotations_InvalidAppliedOn_KeepsTheHistoryWithoutADate()
        {
            var history = FixtureHistory(typeof(BindingFixtures.AppliedOnNotADate));
            Assert.Multiple(() =>
            {
                Assert.That(history.AppliedOn, Is.Null);
                Assert.That(history.SourceUrl, Is.EqualTo("http://fixture.local/e-applied-text"));
            });
        }

        [Test]
        public void FindAnnotations_PaddedAppliedOn_ExportsTheTrimmedDate()
        {
            var history = FixtureHistory(typeof(BindingFixtures.AppliedOnPadded));

            Assert.That(history.AppliedOn, Is.EqualTo(new DateTime(2022, 12, 12)));
        }

        [Test]
        public void FindAnnotations_ParameterlessAttribute_ExportsDefaultVersion()
        {
            var history = FixtureHistory(typeof(BindingFixtures.Parameterless));

            Assert.That(history.Version, Is.EqualTo("1.0"));
        }

        [Test]
        public void Instance_Field_IsStaticAndReadOnly()
        {
            var field = typeof(CodeSourceScanner).GetField(nameof(CodeSourceScanner.Instance), BindingFlags.Public | BindingFlags.Static);

            Assert.Multiple(() =>
            {
                Assert.That(field, Is.Not.Null, "Instance stays a field (a property would break compiled readers)");
                Assert.That(field?.IsInitOnly, Is.True, "Instance must not be assignable");
                Assert.That(field?.FieldType, Is.EqualTo(typeof(CodeSourceScanner)));
            });
        }

        private static CodeSourceObjectHistory FixtureHistory(Type fixture)
            => Scanner.FindAnnotations(new[] { FixtureCatalog.FixturesAssembly })
                .Single(result => result.Parent?.FullName == fixture.FullName).Parent.History.Single();

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

            public bool ReferencesWereRead { get; private set; }

            public override AssemblyName GetName() => _inner.GetName();

            public override AssemblyName GetName(bool copiedName) => _inner.GetName(copiedName);

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
    }
}
