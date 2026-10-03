using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using TempLib.Fixtures;

namespace Tests
{
    [TestFixture]
    public class AttributeBindingTests
    {
        private const string SignatureA = "A";
        private const string SignatureB = "B";
        private const string SignatureC = "C";
        private const string SignatureD = "D";
        private const string SignatureE = "E";
        private const string SignatureF = "F";

        private static readonly IReadOnlyDictionary<string, string[]> ParameterNames = new Dictionary<string, string[]>
        {
            [SignatureA] = new string[0],
            [SignatureF] = new[] { "sourceUrl" },
            [SignatureB] = new[] { "sourceUrl", "version" },
            [SignatureC] = new[] { "sourceUrl", "authorName", "version" },
            [SignatureD] = new[] { "sourceUrl", "authorName", "copyright", "version" },
            [SignatureE] = new[] { "sourceUrl", "authorName", "copyright", "appliedOn", "comment", "version", "workItemId", "tags" },
        };

        private static IEnumerable<TestCaseData> BindingMatrixCases()
        {
            yield return Binding(typeof(LegacyCtorFixtures.TwoPositional), SignatureB);
            yield return Binding(typeof(LegacyCtorFixtures.ThreePositional), SignatureC);
            yield return Binding(typeof(LegacyCtorFixtures.NamedVersion), SignatureB);
            yield return Binding(typeof(LegacyCtorFixtures.NamedVersionReordered), SignatureB);
            yield return Binding(typeof(LegacyCtorFixtures.NamedAuthorAndVersion), SignatureC);
            yield return Binding(typeof(LegacyCtorFixtures.AllNamedAuthorAndVersion), SignatureC);

            yield return Binding(typeof(BindingFixtures.Parameterless), SignatureA);
            yield return Binding(typeof(BindingFixtures.PropertiesOnly), SignatureA);
            yield return Binding(typeof(BindingFixtures.PropertiesInvalidAppliedOn), SignatureA);
            yield return Binding(typeof(BindingFixtures.SourceUrlOnly), SignatureF);
            yield return Binding(typeof(BindingFixtures.SourceUrlWithProperties), SignatureF);
            yield return Binding(typeof(BindingFixtures.FourPositional), SignatureD);
            yield return Binding(typeof(BindingFixtures.FourNamed), SignatureD);
            yield return Binding(typeof(BindingFixtures.FourPositionalNullCopyright), SignatureD);
            yield return Binding(typeof(BindingFixtures.FourPositionalPrefixedCopyright), SignatureD);
            yield return Binding(typeof(BindingFixtures.NamedAuthorOnly), SignatureE);
            yield return Binding(typeof(BindingFixtures.NamedCopyrightOnly), SignatureE);
            yield return Binding(typeof(BindingFixtures.FivePositional), SignatureE);
            yield return Binding(typeof(BindingFixtures.EightPositional), SignatureE);
            yield return Binding(typeof(BindingFixtures.NullSourceUrlNamedAuthorAndCopyright), SignatureE);
            yield return Binding(typeof(BindingFixtures.AppliedOnValid), SignatureE);
            yield return Binding(typeof(BindingFixtures.AppliedOnPadded), SignatureE);
            yield return Binding(typeof(BindingFixtures.AppliedOnNotADate), SignatureE);
            yield return Binding(typeof(BindingFixtures.AppliedOnOutOfRange), SignatureE);
            yield return Binding(typeof(BindingFixtures.WhitespaceCopyright), SignatureE);
            yield return Binding(typeof(BindingFixtures.LeadingWhitespacePrefixedCopyright), SignatureE);
        }

        [TestCaseSource(nameof(BindingMatrixCases))]
        public void AttributeApplication_BindsExpectedConstructor(Type fixture, string signature)
        {
            var expected = ParameterNames[signature];

            var parameters = SingleCodeSourceData(fixture).Constructor.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters.Length, Is.EqualTo(expected.Length), $"arity of the bound ctor ({signature})");
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(expected), $"parameter names of the bound ctor ({signature})");
            });
        }

        private static IEnumerable<TestCaseData> ValueSnapshotCases()
        {
            yield return Values(typeof(LegacyCtorFixtures.TwoPositional),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/b", Version = "John" });
            yield return Values(typeof(LegacyCtorFixtures.ThreePositional),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/c", AuthorName = "John", Version = "MIT" });
            yield return Values(typeof(LegacyCtorFixtures.NamedVersion),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/b-named", Version = "2.0" });
            yield return Values(typeof(LegacyCtorFixtures.NamedVersionReordered),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/b-reordered", Version = "2.1" });
            yield return Values(typeof(LegacyCtorFixtures.NamedAuthorAndVersion),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/c-named", AuthorName = "Alice", Version = "3.0" });
            yield return Values(typeof(LegacyCtorFixtures.AllNamedAuthorAndVersion),
                new ExpectedValues { SourceUrl = "http://fixture.local/legacy/c-all-named", AuthorName = "Bob", Version = "3.1" });

            yield return Values(typeof(BindingFixtures.Parameterless), new ExpectedValues());
            yield return Values(typeof(BindingFixtures.PropertiesOnly), new ExpectedValues
            {
                SourceUrl = "http://fixture.local/a", AuthorName = "Alice", Copyright = "ACME", AppliedOn = "2022-12-12",
                Comment = "via properties", Version = "1.1", Tags = "x;y", RelatedTaskId = "#7",
            });

            yield return Values(typeof(BindingFixtures.SourceUrlOnly),
                new ExpectedValues { SourceUrl = "http://fixture.local/f", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.SourceUrlWithProperties),
                new ExpectedValues { SourceUrl = "http://fixture.local/f-props", AuthorName = "Alice", Version = "2.5" });

            yield return Values(typeof(BindingFixtures.FourPositional),
                new ExpectedValues { SourceUrl = "http://fixture.local/d", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.0" });
            yield return Values(typeof(BindingFixtures.FourNamed),
                new ExpectedValues { SourceUrl = "http://fixture.local/d-named", AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1" });
            yield return Values(typeof(BindingFixtures.FourPositionalNullCopyright),
                new ExpectedValues { SourceUrl = "http://fixture.local/d-null-copyright", AuthorName = "Alice", Version = "4.1" });
            yield return Values(typeof(BindingFixtures.FourPositionalPrefixedCopyright),
                new ExpectedValues { SourceUrl = "http://fixture.local/d-prefixed", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.2" });

            yield return Values(typeof(BindingFixtures.NamedAuthorOnly),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-author", AuthorName = "Alice", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.NamedCopyrightOnly),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-copyright", Copyright = "\u00A9 ACME", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.FivePositional), new ExpectedValues
            {
                SourceUrl = "http://fixture.local/e5", AuthorName = "Alice", Copyright = "\u00A9 ACME", AppliedOn = "2022-12-12",
                Comment = "five positional", Version = "1.0",
            });
            yield return Values(typeof(BindingFixtures.EightPositional), new ExpectedValues
            {
                SourceUrl = "http://fixture.local/e8", AuthorName = "Alice", Copyright = "\u00A9 ACME", AppliedOn = "2022-12-12",
                Comment = "eight positional", Version = "5.0", RelatedTaskId = "#42", Tags = "tag1;tag2",
            });
            yield return Values(typeof(BindingFixtures.NullSourceUrlNamedAuthorAndCopyright),
                new ExpectedValues { AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.WhitespaceCopyright),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-copyright-blank", Version = "1.0" });

            yield return Values(typeof(BindingFixtures.AppliedOnValid),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-applied", AppliedOn = "2022-12-12", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.AppliedOnPadded),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-applied-padded", AppliedOn = " 2022-12-12 ", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.AppliedOnNotADate),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-applied-text", AppliedOn = "x", Version = "1.0" });
            yield return Values(typeof(BindingFixtures.AppliedOnOutOfRange),
                new ExpectedValues { SourceUrl = "http://fixture.local/e-applied-range", AppliedOn = "2022-13-40", Version = "1.0" });
        }

        [TestCaseSource(nameof(ValueSnapshotCases))]
        public void AttributeApplication_SetsPropertiesItsParameterNamesImply(Type fixture, ExpectedValues expected)
        {
            var attribute = SingleCodeSourceAttribute(fixture);

            Assert.Multiple(() =>
            {
                Assert.That(attribute.SourceUrl, Is.EqualTo(expected.SourceUrl), nameof(attribute.SourceUrl));
                Assert.That(attribute.AuthorName, Is.EqualTo(expected.AuthorName), nameof(attribute.AuthorName));
                Assert.That(attribute.Copyright, Is.EqualTo(expected.Copyright), nameof(attribute.Copyright));
                Assert.That(attribute.AppliedOn, Is.EqualTo(expected.AppliedOn), nameof(attribute.AppliedOn));
                Assert.That(attribute.Comment, Is.EqualTo(expected.Comment), nameof(attribute.Comment));
                Assert.That(attribute.Version, Is.EqualTo(expected.Version), nameof(attribute.Version));
                Assert.That(attribute.Tags, Is.EqualTo(expected.Tags), nameof(attribute.Tags));
                Assert.That(attribute.RelatedTaskId, Is.EqualTo(expected.RelatedTaskId), nameof(attribute.RelatedTaskId));
            });
        }

        [TestCase(SignatureB)]
        [TestCase(SignatureC)]
        [TestCase(SignatureD)]
        public void PositionalConstructor_HasNoOptionalParameters(string signature)
        {
            var constructor = GetConstructor(signature);

            var optional = constructor.GetParameters().Where(p => p.HasDefaultValue || p.IsOptional).Select(p => p.Name);

            Assert.That(optional, Is.Empty, $"ctor {signature} must not declare default values");
        }

        [Test]
        public void SourceUrlConstructor_ExistsWithOneRequiredParameter()
        {
            var constructor = GetConstructor(SignatureF);

            Assert.That(constructor.GetParameters().Single().HasDefaultValue, Is.False);
        }

        [Test]
        public void FullConstructor_KeepsItsSignatureAndDefaults()
        {
            var defaults = GetConstructor(SignatureE).GetParameters()
                .Skip(1)
                .Select(p => p.HasDefaultValue ? p.DefaultValue : "<no default>");

            Assert.That(defaults, Is.EqualTo(new object[] { null, null, null, null, "1.0", null, null }));
        }

        [TestCase(SignatureB)]
        [TestCase(SignatureC)]
        public void LegacyPositionalConstructor_IsObsoleteWarning(string signature)
        {
            var obsolete = GetConstructor(signature).GetCustomAttribute<ObsoleteAttribute>();

            Assert.Multiple(() =>
            {
                Assert.That(obsolete, Is.Not.Null, $"ctor {signature} must carry [Obsolete]");
                Assert.That(obsolete?.IsError, Is.False, "warning, not error");
                Assert.That(obsolete?.Message, Does.Contain("CodeSource(sourceUrl)"));
                Assert.That(obsolete?.Message, Does.Contain("named properties"));
                Assert.That(obsolete?.Message, Does.Contain("Positional arguments bind by position"));
            });
        }

        [TestCase(SignatureA)]
        [TestCase(SignatureD)]
        [TestCase(SignatureE)]
        [TestCase(SignatureF)]
        public void NonLegacyConstructor_IsNotObsolete(string signature)
        {
            var obsolete = GetConstructor(signature).GetCustomAttribute<ObsoleteAttribute>();

            Assert.That(obsolete, Is.Null);
        }

        [TestCase(typeof(BindingFixtures.AppliedOnValid))]
        [TestCase(typeof(BindingFixtures.AppliedOnPadded))]
        [TestCase(typeof(BindingFixtures.FivePositional))]
        [TestCase(typeof(BindingFixtures.EightPositional))]
        [TestCase(typeof(BindingFixtures.PropertiesOnly))]
        public void AppliedOn_ValidDate_ParsesToInternalDate(Type fixture)
        {
            var attribute = SingleCodeSourceAttribute(fixture);

            var parsed = ReadInternalAppliedOn(attribute);

            Assert.That(parsed, Is.EqualTo(new DateTime(2022, 12, 12)));
        }

        [TestCase(typeof(BindingFixtures.AppliedOnNotADate))]
        [TestCase(typeof(BindingFixtures.AppliedOnOutOfRange))]
        [TestCase(typeof(BindingFixtures.PropertiesInvalidAppliedOn))]
        [TestCase(typeof(BindingFixtures.SourceUrlOnly))]
        public void AppliedOn_InvalidOrMissing_GivesNullInternalDateWithoutThrowing(Type fixture)
        {
            CodeSourceAttribute attribute = null;

            Assert.DoesNotThrow(() => attribute = SingleCodeSourceAttribute(fixture));
            Assert.That(ReadInternalAppliedOn(attribute), Is.Null);
        }

        [TestCase("x")]
        [TestCase("2022-13-40")]
        [TestCase("12/12/2022")]
        [TestCase("   ")]
        public void AppliedOn_InvalidValueThroughFullConstructor_DoesNotThrowAndKeepsRawValue(string appliedOn)
        {
            CodeSourceAttribute attribute = null;

            Assert.DoesNotThrow(() => attribute = new CodeSourceAttribute("http://fixture.local/direct", appliedOn: appliedOn));
            Assert.Multiple(() =>
            {
                Assert.That(ReadInternalAppliedOn(attribute), Is.Null);
                Assert.That(attribute.AppliedOn, Is.EqualTo(appliedOn));
            });
        }

        [TestCase(typeof(BindingFixtures.FourPositionalNullCopyright))]
        [TestCase(typeof(BindingFixtures.WhitespaceCopyright))]
        [TestCase(typeof(BindingFixtures.NamedAuthorOnly))]
        public void Copyright_MissingOnConstructorPath_IsNull(Type fixture)
        {
            var attribute = SingleCodeSourceAttribute(fixture);

            Assert.That(attribute.Copyright, Is.Null);
        }

        [Test]
        public void Copyright_AppliedTwiceThroughConstructor_IsPrefixedOnce()
        {
            var first = new CodeSourceAttribute("http://fixture.local/direct", "Alice", "ACME", "1.0");

            var second = new CodeSourceAttribute("http://fixture.local/direct", "Alice", first.Copyright, "1.0");

            Assert.Multiple(() =>
            {
                Assert.That(first.Copyright, Is.EqualTo("\u00A9 ACME"));
                Assert.That(second.Copyright, Is.EqualTo(first.Copyright));
            });
        }

        [Test]
        public void Copyright_PrefixedValueWithLeadingWhitespace_IsNotPrefixedAgain()
        {
            var attribute = SingleCodeSourceAttribute(typeof(BindingFixtures.LeadingWhitespacePrefixedCopyright));

            Assert.Multiple(() =>
            {
                Assert.That(attribute.Copyright.Count(c => c == '\u00A9'), Is.EqualTo(1), attribute.Copyright);
                Assert.That(attribute.Copyright.TrimStart(), Does.StartWith("\u00A9 ACME"));
            });
        }

        public sealed class ExpectedValues
        {
            public string SourceUrl { get; set; }

            public string AuthorName { get; set; }

            public string Copyright { get; set; }

            public string AppliedOn { get; set; }

            public string Comment { get; set; }

            public string Version { get; set; }

            public string Tags { get; set; }

            public string RelatedTaskId { get; set; }
        }

        private static TestCaseData Binding(Type fixture, string signature)
            => new TestCaseData(fixture, signature).SetName($"Binds_{signature}_{fixture.Name}");

        private static TestCaseData Values(Type fixture, ExpectedValues expected)
            => new TestCaseData(fixture, expected).SetName($"Values_{fixture.DeclaringType?.Name}_{fixture.Name}");

        private static ConstructorInfo GetConstructor(string signature)
        {
            var names = ParameterNames[signature];
            var constructor = typeof(CodeSourceAttribute).GetConstructor(Enumerable.Repeat(typeof(string), names.Length).ToArray());

            Assert.That(constructor, Is.Not.Null, $"ctor {signature} ({string.Join(", ", names)}) must exist");
            Assert.That(constructor.GetParameters().Select(p => p.Name), Is.EqualTo(names), $"ctor {signature} parameter names");

            return constructor;
        }

        private static CustomAttributeData SingleCodeSourceData(Type fixture)
            => fixture.GetCustomAttributesData().Single(a => a.AttributeType == typeof(CodeSourceAttribute));

        private static CodeSourceAttribute SingleCodeSourceAttribute(Type fixture)
            => fixture.GetCustomAttributes<CodeSourceAttribute>(inherit: false).Single();

        private static DateTime? ReadInternalAppliedOn(CodeSourceAttribute attribute)
        {
            var property = typeof(CodeSourceAttribute).GetProperty("InternalAppliedOn", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null, "internal InternalAppliedOn must exist");
            Assert.That(property.PropertyType, Is.EqualTo(typeof(DateTime?)));

            return (DateTime?)property.GetValue(attribute);
        }
    }
}
