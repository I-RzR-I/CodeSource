using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Attributes
{
    [TestFixture]
    public class AttributeBindingTests
    {
        private static readonly Binding[] Bindings =
        {
            new Binding(typeof(LegacyCtorFixtures.TwoPositional), CtorSignature.LegacySourceUrlVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/b", Version = "John" }),
            new Binding(typeof(LegacyCtorFixtures.ThreePositional), CtorSignature.LegacySourceUrlAuthorVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/c", AuthorName = "John", Version = "MIT" }),
            new Binding(typeof(LegacyCtorFixtures.NamedVersion), CtorSignature.LegacySourceUrlVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/b-named", Version = "2.0" }),
            new Binding(typeof(LegacyCtorFixtures.NamedVersionReordered), CtorSignature.LegacySourceUrlVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/b-reordered", Version = "2.1" }),
            new Binding(typeof(LegacyCtorFixtures.NamedAuthorAndVersion), CtorSignature.LegacySourceUrlAuthorVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/c-named", AuthorName = "Alice", Version = "3.0" }),
            new Binding(typeof(LegacyCtorFixtures.AllNamedAuthorAndVersion), CtorSignature.LegacySourceUrlAuthorVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/legacy/c-all-named", AuthorName = "Bob", Version = "3.1" }),

            new Binding(typeof(BindingFixtures.Parameterless), CtorSignature.Parameterless,
                new ExpectedAttributeValues()),
            new Binding(typeof(BindingFixtures.PropertiesOnly), CtorSignature.Parameterless, new ExpectedAttributeValues
            {
                SourceUrl = "http://fixture.local/a", AuthorName = "Alice", Copyright = "ACME", AppliedOn = "2022-12-12",
                Comment = "via properties", Version = "1.1", Tags = "x;y", RelatedTaskId = "#7"
            }),
            new Binding(typeof(BindingFixtures.PropertiesInvalidAppliedOn), CtorSignature.Parameterless,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/a-invalid-date", AppliedOn = "2022-13-40" }),
            new Binding(typeof(BindingFixtures.SourceUrlOnly), CtorSignature.SourceUrl,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/f", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.SourceUrlWithProperties), CtorSignature.SourceUrl,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/f-props", AuthorName = "Alice", Version = "2.5" }),

            new Binding(typeof(BindingFixtures.FourPositional), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.0" }),
            new Binding(typeof(BindingFixtures.FourNamed), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d-named", AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1" }),
            new Binding(typeof(BindingFixtures.FourPositionalNullCopyright), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d-null-copyright", AuthorName = "Alice", Version = "4.1" }),
            new Binding(typeof(BindingFixtures.FourPositionalPrefixedCopyright), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d-prefixed", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.2" }),
            new Binding(typeof(BindingFixtures.ThreePositionalNamedVersion), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d-mixed", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.3" }),
            new Binding(typeof(BindingFixtures.FourPositionalWithProperty), CtorSignature.SourceUrlAuthorCopyrightVersion,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/d-props", AuthorName = "Alice", Copyright = "\u00A9 ACME", Version = "4.4", Tags = "t" }),

            new Binding(typeof(BindingFixtures.NamedAuthorOnly), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-author", AuthorName = "Alice", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.NamedCopyrightOnly), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-copyright", Copyright = "\u00A9 ACME", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.NamedCommentOnly), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-comment", Comment = "m", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.NamedWorkItemIdOnly), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-workitem", RelatedTaskId = "#9", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.NamedTagsOnly), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-tags", Tags = "t", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.FivePositional), CtorSignature.Full, new ExpectedAttributeValues
            {
                SourceUrl = "http://fixture.local/e5", AuthorName = "Alice", Copyright = "\u00A9 ACME", AppliedOn = "2022-12-12",
                Comment = "five positional", Version = "1.0"
            }),
            new Binding(typeof(BindingFixtures.EightPositional), CtorSignature.Full, new ExpectedAttributeValues
            {
                SourceUrl = "http://fixture.local/e8", AuthorName = "Alice", Copyright = "\u00A9 ACME", AppliedOn = "2022-12-12",
                Comment = "eight positional", Version = "5.0", RelatedTaskId = "#42", Tags = "tag1;tag2"
            }),
            new Binding(typeof(BindingFixtures.NullSourceUrlNamedAuthorAndCopyright), CtorSignature.Full,
                new ExpectedAttributeValues { AuthorName = "Company User", Copyright = "\u00A9 Company INC", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.AppliedOnValid), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-applied", AppliedOn = "2022-12-12", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.AppliedOnPadded), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-applied-padded", AppliedOn = " 2022-12-12 ", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.AppliedOnNotADate), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-applied-text", AppliedOn = "x", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.AppliedOnOutOfRange), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-applied-range", AppliedOn = "2022-13-40", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.WhitespaceCopyright), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-copyright-blank", Version = "1.0" }),
            new Binding(typeof(BindingFixtures.LeadingWhitespacePrefixedCopyright), CtorSignature.Full,
                new ExpectedAttributeValues { SourceUrl = "http://fixture.local/e-copyright-padded", Copyright = "  \u00A9 ACME", Version = "1.0" })
        };

        [TestCaseSource(nameof(BindingCases))]
        public void AttributeApplication_EachFixture_BindsExpectedConstructor(Type fixture, CtorSignature signature)
        {
            var parameterNames = CodeSourceAttributeProbe.BoundParameterNames(CodeSourceAttributeProbe.SingleData(fixture));

            Assert.That(parameterNames, Is.EqualTo(CtorSignatures.ParameterNames(signature)),
                $"parameter names of the bound ctor ({signature})");
        }

        [TestCaseSource(nameof(ValueSnapshotCases))]
        public void AttributeApplication_EachFixture_SetsPropertiesItsParameterNamesImply(Type fixture, ExpectedAttributeValues expected)
        {
            var attribute = CodeSourceAttributeProbe.Single(fixture);

            expected.AssertMatches(attribute);
        }

        private static IEnumerable<TestCaseData> BindingCases()
            => Bindings.Select(binding => NamedCase.Of(binding.Fixture.Name, binding.Fixture, binding.Signature));

        private static IEnumerable<TestCaseData> ValueSnapshotCases()
            => Bindings
                .Where(binding => binding.Values != null)
                .Select(binding => NamedCase.Of(binding.Fixture.Name, binding.Fixture, binding.Values));

        private sealed record Binding(Type Fixture, CtorSignature Signature, ExpectedAttributeValues Values = null);
    }
}
