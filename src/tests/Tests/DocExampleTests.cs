using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using TempLib.Fixtures;

namespace Tests
{
    [TestFixture]
    public class DocExampleTests
    {
        private static readonly IReadOnlyDictionary<char, int> Arity = new Dictionary<char, int>
        {
            ['A'] = 0, ['F'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4, ['E'] = 8,
        };

        private static IEnumerable<TestCaseData> SingleApplicationSamples()
        {
            yield return Sample(typeof(DocSampleFixtures.DisposeArticleSample), 'F', new DocValues
            {
                SourceUrl = "https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose",
                AuthorName = "Jane Doe", Version = "1.0",
            });
            yield return Sample(typeof(DocSampleFixtures.AuthorAndCopyrightPropertiesSample), 'A', new DocValues
            {
                AuthorName = "Company User", Copyright = "Company INC",
            });
            yield return Sample(typeof(DocSampleFixtures.ClassAndMethodSample), 'F', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "User", Copyright = "Company INC", Version = "2.0",
            });
            yield return Sample(typeof(DocSampleFixtures.AllPropertiesSample), 'F', new DocValues
            {
                SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", Copyright = "Example Ltd",
                AppliedOn = "2026-09-29", Comment = "Retry policy adapted from the blog post", Version = "1.1",
                Tags = "resilience;http", RelatedTaskId = "#123",
            });
            yield return Sample(typeof(DocSampleFixtures.NullUrlArgumentsSample), 'E', new DocValues
            {
                AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1.0",
            });
            yield return Sample(typeof(DocSampleFixtures.NamedAuthorArgumentSample), 'E', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "1.0",
            });
            yield return Sample(typeof(DocSampleFixtures.AllArgumentsSample), 'E', new DocValues
            {
                SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", Copyright = "\u00A9 Example Ltd",
                AppliedOn = "2026-09-29", Comment = "Retry policy adapted from the blog post", Version = "1.1",
                RelatedTaskId = "#123", Tags = "resilience;http",
            });
            yield return Sample(typeof(DocSampleFixtures.FourNamedArgumentsSample), 'D', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "User", Copyright = "\u00A9 Company INC", Version = "2.0",
            });
            yield return Sample(typeof(DocSampleFixtures.UrlAndVersionPropertySample), 'F', new DocValues
            {
                SourceUrl = "https://example.com", Version = "1.5",
            });
            yield return Sample(typeof(DocSampleFixtures.AuthorAndVersionPropertiesSample), 'F', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0",
            });

            yield return Sample(typeof(LegacyCtorFixtures.AuthorBecomesVersionPitfall), 'B', new DocValues
            {
                SourceUrl = "https://example.com", Version = "John",
            });
            yield return Sample(typeof(LegacyCtorFixtures.LicenseBecomesVersionPitfall), 'C', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "MIT",
            });
            yield return Sample(typeof(LegacyCtorFixtures.NamedVersionArgumentPitfall), 'B', new DocValues
            {
                SourceUrl = "https://example.com", Version = "1.5",
            });
            yield return Sample(typeof(LegacyCtorFixtures.NamedAuthorAndVersionArgumentsPitfall), 'C', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0",
            });
            yield return Sample(typeof(LegacyCtorFixtures.PositionalAuthorNamedVersionPitfall), 'C', new DocValues
            {
                SourceUrl = "https://example.com", AuthorName = "John", Version = "2.0",
            });
            yield return Sample(typeof(LegacyCtorFixtures.VersionAsSecondPositionalPitfall), 'B', new DocValues
            {
                SourceUrl = "https://example.com", Version = "2.0",
            });
        }

        [TestCaseSource(nameof(SingleApplicationSamples))]
        public void DocSample_BindsTheDocumentedConstructor(Type fixture, char signature, DocValues expected)
        {
            var data = fixture.GetCustomAttributesData().Single(a => a.AttributeType == typeof(CodeSourceAttribute));

            Assert.That(data.Constructor.GetParameters().Length, Is.EqualTo(Arity[signature]), $"bound ctor {signature}");
        }

        [TestCaseSource(nameof(SingleApplicationSamples))]
        public void DocSample_HasTheDocumentedValues(Type fixture, char signature, DocValues expected)
        {
            var attribute = fixture.GetCustomAttributes<CodeSourceAttribute>(inherit: false).Single();

            AssertValues(attribute, expected);
        }

        [Test]
        public void DocSample_Method_HasTheDocumentedValues()
        {
            var method = typeof(DocSampleFixtures.ClassAndMethodSample).GetMethod(nameof(DocSampleFixtures.ClassAndMethodSample.RunAsync));

            var data = method.GetCustomAttributesData().Single(a => a.AttributeType == typeof(CodeSourceAttribute));
            var attribute = method.GetCustomAttributes<CodeSourceAttribute>(inherit: false).Single();

            Assert.Multiple(() =>
            {
                Assert.That(data.Constructor.GetParameters().Length, Is.EqualTo(Arity['F']), "bound ctor F");
                AssertValues(attribute, new DocValues
                {
                    SourceUrl = "https://example.com/articles/use-async", AuthorName = "User", Copyright = "Company INC",
                    AppliedOn = "2022-12-12", Comment = "This is how to use async", Version = "1.3",
                });
                Assert.That(ReadInternalAppliedOn(attribute), Is.EqualTo(new DateTime(2022, 12, 12)), "internal parsed date");
            });
        }

        [Test]
        public void DocSample_TwoApplications_HaveTheDocumentedValues()
        {
            var data = typeof(DocSampleFixtures.TwoApplicationsSample).GetCustomAttributesData()
                .Where(a => a.AttributeType == typeof(CodeSourceAttribute)).ToList();
            var byVersion = typeof(DocSampleFixtures.TwoApplicationsSample).GetCustomAttributes<CodeSourceAttribute>(inherit: false)
                .ToDictionary(a => a.Version);

            Assert.Multiple(() =>
            {
                Assert.That(data.Select(d => d.Constructor.GetParameters().Length), Is.All.EqualTo(Arity['F']), "both bind F");
                Assert.That(byVersion.Keys, Is.EquivalentTo(new[] { "1.0", "1.1" }));
                AssertValues(byVersion["1.0"], new DocValues
                {
                    SourceUrl = "https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe",
                    AppliedOn = "2026-01-15", Version = "1.0",
                });
                AssertValues(byVersion["1.1"], new DocValues
                {
                    SourceUrl = "https://example.com/blog/retry-with-jitter", AuthorName = "John Roe",
                    AppliedOn = "2026-09-29", Comment = "Added jitter", Version = "1.1",
                });
            });
        }

        [TestCase(typeof(DocSampleFixtures.AllPropertiesSample), 2026, 9, 29)]
        [TestCase(typeof(DocSampleFixtures.AllArgumentsSample), 2026, 9, 29)]
        public void DocSample_AppliedOn_ParsesToTheDocumentedDate(Type fixture, int year, int month, int day)
        {
            var attribute = fixture.GetCustomAttributes<CodeSourceAttribute>(inherit: false).Single();

            Assert.That(ReadInternalAppliedOn(attribute), Is.EqualTo(new DateTime(year, month, day)));
        }

        [Test]
        public void DocSample_ScanWithOptions_ReportsOnlyValueErrorsAndContinues()
        {
            var assembly = typeof(DocSampleFixtures.DisposeArticleSample).Assembly;

            var errors = new List<CodeSourceScanError>();
            var options = new CodeSourceScanOptions { OnError = error => errors.Add(error) };
            var results = CodeSourceScanner.Instance.FindAnnotations(assembly, options).ToList();

            var invalidAppliedOnTypes = new[]
            {
                typeof(BindingFixtures.AppliedOnNotADate),
                typeof(BindingFixtures.AppliedOnOutOfRange),
                typeof(BindingFixtures.PropertiesInvalidAppliedOn),
                typeof(BindingFixtures.InvalidAppliedOnOnMethod),
            };
            var parents = results.Select(r => r.Parent?.FullName).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(parents, Has.Member(typeof(DocSampleFixtures.DisposeArticleSample).FullName));
                Assert.That(errors.Where(e => e.Stage != CodeSourceScanStage.AttributeValue).Select(e => e.Message), Is.Empty,
                    "no AssemblyLoad/TypeEnumeration/MemberEnumeration/AttributeRead errors");
                Assert.That(errors.Select(e => e.TypeName).Distinct(), Is.EquivalentTo(invalidAppliedOnTypes.Select(t => t.FullName)),
                    "AttributeValue errors (TypeName = full name) only for the fixtures with an invalid AppliedOn");
                Assert.That(parents, Is.SupersetOf(invalidAppliedOnTypes.Select(t => t.FullName)),
                    "the scan continues: every reported type is still in the results");
            });
        }

        [Test]
        public void DocSample_ThrowingOnError_PropagatesOutOfFindAnnotations()
        {
            var assembly = typeof(DocSampleFixtures.DisposeArticleSample).Assembly;
            var options = new CodeSourceScanOptions { OnError = _ => throw new DocSampleAbortException() };

            Assert.Throws<DocSampleAbortException>(() => CodeSourceScanner.Instance.FindAnnotations(assembly, options).ToList());
        }

        public sealed class DocValues
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

        private static TestCaseData Sample(Type fixture, char signature, DocValues expected)
            => new TestCaseData(fixture, signature, expected).SetName($"{{m}}({fixture.Name})");

        private static void AssertValues(CodeSourceAttribute actual, DocValues expected)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual.SourceUrl, Is.EqualTo(expected.SourceUrl), nameof(actual.SourceUrl));
                Assert.That(actual.AuthorName, Is.EqualTo(expected.AuthorName), nameof(actual.AuthorName));
                Assert.That(actual.Copyright, Is.EqualTo(expected.Copyright), nameof(actual.Copyright));
                Assert.That(actual.AppliedOn, Is.EqualTo(expected.AppliedOn), nameof(actual.AppliedOn));
                Assert.That(actual.Comment, Is.EqualTo(expected.Comment), nameof(actual.Comment));
                Assert.That(actual.Version, Is.EqualTo(expected.Version), nameof(actual.Version));
                Assert.That(actual.Tags, Is.EqualTo(expected.Tags), nameof(actual.Tags));
                Assert.That(actual.RelatedTaskId, Is.EqualTo(expected.RelatedTaskId), nameof(actual.RelatedTaskId));
            });
        }

        private static DateTime? ReadInternalAppliedOn(CodeSourceAttribute attribute)
        {
            var property = typeof(CodeSourceAttribute).GetProperty("InternalAppliedOn", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null, "internal InternalAppliedOn must exist");

            return (DateTime?)property.GetValue(attribute);
        }

        private sealed class DocSampleAbortException : Exception
        {
            public DocSampleAbortException()
                : base("OnError asked the scan to stop.")
            {
            }
        }
    }
}
