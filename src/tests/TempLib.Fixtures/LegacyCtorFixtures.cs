using RzR.Core.CodeSource;

namespace TempLib.Fixtures
{
    public static class LegacyCtorFixtures
    {
        [CodeSource("http://fixture.local/legacy/b", "John")]
        public sealed class TwoPositional { }

        [CodeSource("http://fixture.local/legacy/c", "John", "MIT")]
        public sealed class ThreePositional { }

        [CodeSource("http://fixture.local/legacy/b-named", version: "2.0")]
        public sealed class NamedVersion { }

        [CodeSource(version: "2.1", sourceUrl: "http://fixture.local/legacy/b-reordered")]
        public sealed class NamedVersionReordered { }

        [CodeSource("http://fixture.local/legacy/c-named", authorName: "Alice", version: "3.0")]
        public sealed class NamedAuthorAndVersion { }

        [CodeSource(sourceUrl: "http://fixture.local/legacy/c-all-named", authorName: "Bob", version: "3.1")]
        public sealed class AllNamedAuthorAndVersion { }

        [CodeSource("https://example.com", "John")]
        public sealed class AuthorBecomesVersionPitfall { }

        [CodeSource("https://example.com", "John", "MIT")]
        public sealed class LicenseBecomesVersionPitfall { }

        [CodeSource("https://example.com", version: "1.5")]
        public sealed class NamedVersionArgumentPitfall { }

        [CodeSource("https://example.com", authorName: "John", version: "2.0")]
        public sealed class NamedAuthorAndVersionArgumentsPitfall { }

        [CodeSource("https://example.com", "John", version: "2.0")]
        public sealed class PositionalAuthorNamedVersionPitfall { }

        [CodeSource("https://example.com", "2.0")]
        public sealed class VersionAsSecondPositionalPitfall { }
    }
}
