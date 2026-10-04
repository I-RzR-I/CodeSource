using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Attributes
{
    [TestFixture]
    public class DocExampleTests
    {
        private static readonly DocSample[] DocSamples =
        {
            new DocSample(typeof(DocSampleFixtures.DisposeArticleSample), CtorSignature.SourceUrl, new ExpectedAttributeValues
            {
                SourceUrl = "https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose",
                AuthorName = "Jane Doe", Version = "1.0"
            }),
            new DocSample(typeof(DocSampleFixtures.AuthorAndCopyrightPropertiesSample), CtorSignature.Parameterless, new ExpectedAttributeValues
            {
                AuthorName = "Company User", Copyright = "Company INC"
            }),
            new DocSample(typeof(DocSampleFixtures.ClassAndMethodSample), CtorSignature.SourceUrl, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "User", Copyright = "Company INC", Version = "2.0"
            }),
            new DocSample(typeof(DocSampleFixtures.AllPropertiesSample), CtorSignature.SourceUrl, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", Copyright = "Example Ltd",
                AppliedOn = "2026-09-29", Comment = "Retry policy adapted from the blog post", Version = "1.1",
                Tags = "resilience;http", RelatedTaskId = "#123"
            }),
            new DocSample(typeof(DocSampleFixtures.NullUrlArgumentsSample), CtorSignature.Full, new ExpectedAttributeValues
            {
                AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1.0"
            }),
            new DocSample(typeof(DocSampleFixtures.NamedAuthorArgumentSample), CtorSignature.Full, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "1.0"
            }),
            new DocSample(typeof(DocSampleFixtures.AllArgumentsSample), CtorSignature.Full, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", Copyright = "\u00A9 Example Ltd",
                AppliedOn = "2026-09-29", Comment = "Retry policy adapted from the blog post", Version = "1.1",
                RelatedTaskId = "#123", Tags = "resilience;http"
            }),
            new DocSample(typeof(DocSampleFixtures.FourNamedArgumentsSample), CtorSignature.SourceUrlAuthorCopyrightVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "User", Copyright = "\u00A9 Company INC", Version = "2.0"
            }),
            new DocSample(typeof(DocSampleFixtures.UrlAndVersionPropertySample), CtorSignature.SourceUrl, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", Version = "1.5"
            }),
            new DocSample(typeof(DocSampleFixtures.AuthorAndVersionPropertiesSample), CtorSignature.SourceUrl, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0"
            }),

            new DocSample(typeof(LegacyCtorFixtures.AuthorBecomesVersionPitfall), CtorSignature.LegacySourceUrlVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", Version = "John"
            }),
            new DocSample(typeof(LegacyCtorFixtures.LicenseBecomesVersionPitfall), CtorSignature.LegacySourceUrlAuthorVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "MIT"
            }),
            new DocSample(typeof(LegacyCtorFixtures.NamedVersionArgumentPitfall), CtorSignature.LegacySourceUrlVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", Version = "1.5"
            }),
            new DocSample(typeof(LegacyCtorFixtures.NamedAuthorAndVersionArgumentsPitfall), CtorSignature.LegacySourceUrlAuthorVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0"
            }),
            new DocSample(typeof(LegacyCtorFixtures.PositionalAuthorNamedVersionPitfall), CtorSignature.LegacySourceUrlAuthorVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0"
            }),
            new DocSample(typeof(LegacyCtorFixtures.VersionAsSecondPositionalPitfall), CtorSignature.LegacySourceUrlVersion, new ExpectedAttributeValues
            {
                SourceUrl = "https://example.com", Version = "2.0"
            })
        };

        [TestCaseSource(nameof(DocSampleBindingCases))]
        public void DocSample_EachSample_BindsTheDocumentedConstructor(Type fixture, CtorSignature signature)
        {
            var data = CodeSourceAttributeProbe.SingleData(fixture);

            Assert.That(CodeSourceAttributeProbe.BoundParameterNames(data), Is.EqualTo(CtorSignatures.ParameterNames(signature)), $"bound ctor {signature}");
        }

        [TestCaseSource(nameof(DocSampleValueCases))]
        public void DocSample_EachSample_HasTheDocumentedValues(Type fixture, ExpectedAttributeValues expected)
        {
            var attribute = CodeSourceAttributeProbe.Single(fixture);

            expected.AssertMatches(attribute);
        }

        [Test]
        public void DocSample_Method_HasTheDocumentedValues()
        {
            var method = typeof(DocSampleFixtures.ClassAndMethodSample).GetMethod(nameof(DocSampleFixtures.ClassAndMethodSample.RunAsync));

            var data = CodeSourceAttributeProbe.SingleData(method);
            var attribute = CodeSourceAttributeProbe.Single(method);

            Assert.Multiple(() =>
            {
                Assert.That(CodeSourceAttributeProbe.BoundParameterNames(data), Is.EqualTo(CtorSignatures.ParameterNames(CtorSignature.SourceUrl)), "bound ctor SourceUrl");
                new ExpectedAttributeValues
                {
                    SourceUrl = "https://example.com/articles/use-async", AuthorName = "User", Copyright = "Company INC",
                    AppliedOn = "2022-12-12", Comment = "This is how to use async", Version = "1.3"
                }.AssertMatches(attribute);
                Assert.That(attribute.InternalAppliedOn, Is.EqualTo(new DateTime(2022, 12, 12)), "internal parsed date");
            });
        }

        [Test]
        public void DocSample_TwoApplications_HaveTheDocumentedValues()
        {
            var data = CodeSourceAttributeProbe.AllData(typeof(DocSampleFixtures.TwoApplicationsSample));
            var byVersion = CodeSourceAttributeProbe.All(typeof(DocSampleFixtures.TwoApplicationsSample))
                .ToDictionary(attribute => attribute.Version);

            Assert.Multiple(() =>
            {
                Assert.That(data.Select(CodeSourceAttributeProbe.BoundParameterNames), Is.All.EqualTo(CtorSignatures.ParameterNames(CtorSignature.SourceUrl)),
                    "both bind SourceUrl");
                Assert.That(byVersion.Keys, Is.EquivalentTo(new[] { "1.0", "1.1" }));
                new ExpectedAttributeValues
                {
                    SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe",
                    AppliedOn = "2026-01-15", Version = "1.0"
                }.AssertMatches(byVersion["1.0"]);
                new ExpectedAttributeValues
                {
                    SourceUrl = "https://example.com/blog/retry-with-jitter", AuthorName = "John Roe",
                    AppliedOn = "2026-09-29", Comment = "Added jitter", Version = "1.1"
                }.AssertMatches(byVersion["1.1"]);
            });
        }

        [TestCase(typeof(DocSampleFixtures.AllPropertiesSample), 2026, 9, 29)]
        [TestCase(typeof(DocSampleFixtures.AllArgumentsSample), 2026, 9, 29)]
        public void DocSample_AppliedOn_ParsesToTheDocumentedDate(Type fixture, int year, int month, int day)
        {
            var attribute = CodeSourceAttributeProbe.Single(fixture);

            Assert.That(attribute.InternalAppliedOn, Is.EqualTo(new DateTime(year, month, day)));
        }

        [Test]
        public void DocSample_ScanWithOptions_ReportsOnlyValueErrorsAndContinues()
        {
            var errors = new List<CodeSourceScanError>();

            var results = ScanRunner.Scan(FixtureCatalog.FixturesAssembly, errors);

            var parents = ScanRunner.ParentNames(results);
            var invalidAppliedOnNames = FixtureCatalog.InvalidAppliedOnReportingTypes.Select(type => type.FullName).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(parents, Has.Member(typeof(DocSampleFixtures.DisposeArticleSample).FullName));
                Assert.That(errors.Where(error => error.Stage != CodeSourceScanStage.AttributeValue).Select(error => error.Message), Is.Empty,
                    "no AssemblyLoad/TypeEnumeration/MemberEnumeration/AttributeRead errors");
                Assert.That(errors.Select(error => error.TypeName).Distinct(), Is.EquivalentTo(invalidAppliedOnNames),
                    "AttributeValue errors (TypeName = full name) only for the fixtures with an invalid AppliedOn");
                Assert.That(parents, Is.SupersetOf(invalidAppliedOnNames),
                    "the scan continues: every reported type is still in the results");
            });
        }

        [Test]
        public void DocSample_ThrowingOnError_PropagatesOutOfFindAnnotations()
        {
            var options = new CodeSourceScanOptions { OnError = _ => throw new ScanAbortedException() };

            Assert.Throws<ScanAbortedException>(
                () => CodeSourceScanner.Instance.FindAnnotations(FixtureCatalog.FixturesAssembly, options).ToList());
        }

        private static IEnumerable<TestCaseData> DocSampleBindingCases()
            => DocSamples.Select(sample => NamedCase.Of(sample.Fixture.Name, sample.Fixture, sample.Signature));

        private static IEnumerable<TestCaseData> DocSampleValueCases()
            => DocSamples.Select(sample => NamedCase.Of(sample.Fixture.Name, sample.Fixture, sample.Values));

        private sealed record DocSample(Type Fixture, CtorSignature Signature, ExpectedAttributeValues Values);
    }
}
