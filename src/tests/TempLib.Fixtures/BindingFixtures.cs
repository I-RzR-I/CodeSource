using RzR.Core.CodeSource;

namespace TempLib.Fixtures
{
    public static class BindingFixtures
    {
        [CodeSource]
        public sealed class Parameterless { }

        [CodeSource(SourceUrl = "http://fixture.local/a", AuthorName = "Alice", Copyright = "ACME",
            AppliedOn = "2022-12-12", Comment = "via properties", Version = "1.1", Tags = "x;y", RelatedTaskId = "#7")]
        public sealed class PropertiesOnly { }

        [CodeSource(SourceUrl = "http://fixture.local/a-invalid-date", AppliedOn = "2022-13-40")]
        public sealed class PropertiesInvalidAppliedOn { }

        [CodeSource("http://fixture.local/f")]
        public sealed class SourceUrlOnly { }

        [CodeSource("http://fixture.local/f-props", AuthorName = "Alice", Version = "2.5")]
        public sealed class SourceUrlWithProperties { }

        [CodeSource("http://fixture.local/d", "Alice", "ACME", "4.0")]
        public sealed class FourPositional { }

        [CodeSource(sourceUrl: "http://fixture.local/d-named", authorName: "Company User", copyright: "Company INC", version: "1")]
        public sealed class FourNamed { }

        [CodeSource("http://fixture.local/d-null-copyright", "Alice", null, "4.1")]
        public sealed class FourPositionalNullCopyright { }

        [CodeSource("http://fixture.local/d-prefixed", "Alice", "\u00A9 ACME", "4.2")]
        public sealed class FourPositionalPrefixedCopyright { }

        [CodeSource("http://fixture.local/e-author", authorName: "Alice")]
        public sealed class NamedAuthorOnly { }

        [CodeSource("http://fixture.local/e-copyright", copyright: "ACME")]
        public sealed class NamedCopyrightOnly { }

        [CodeSource("http://fixture.local/e5", "Alice", "ACME", "2022-12-12", "five positional")]
        public sealed class FivePositional { }

        [CodeSource("http://fixture.local/e8", "Alice", "ACME", "2022-12-12", "eight positional", "5.0", "#42", "tag1;tag2")]
        public sealed class EightPositional { }

        [CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC")]
        public sealed class NullSourceUrlNamedAuthorAndCopyright { }

        [CodeSource("http://fixture.local/e-applied", appliedOn: "2022-12-12")]
        public sealed class AppliedOnValid { }

        [CodeSource("http://fixture.local/e-applied-padded", appliedOn: " 2022-12-12 ")]
        public sealed class AppliedOnPadded { }

        [CodeSource("http://fixture.local/e-applied-text", appliedOn: "x")]
        public sealed class AppliedOnNotADate { }

        [CodeSource("http://fixture.local/e-applied-range", appliedOn: "2022-13-40")]
        public sealed class AppliedOnOutOfRange { }

        [CodeSource("http://fixture.local/e-copyright-blank", copyright: "   ")]
        public sealed class WhitespaceCopyright { }

        [CodeSource("http://fixture.local/e-copyright-padded", copyright: "  \u00A9 ACME")]
        public sealed class LeadingWhitespacePrefixedCopyright { }

        public sealed class InvalidAppliedOnOnMethod
        {
            [CodeSource("http://fixture.local/e-member-invalid-date", appliedOn: "not-a-date")]
            public void Run() { }
        }
    }
}
