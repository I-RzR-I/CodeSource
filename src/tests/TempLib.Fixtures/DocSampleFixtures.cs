using System.Threading.Tasks;
using RzR.Core.CodeSource;

namespace TempLib.Fixtures
{
    public static class DocSampleFixtures
    {
        [CodeSource("https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose",
            AuthorName = "Jane Doe",
            Version = "1.0")]
        public sealed class DisposeArticleSample { }

        [CodeSource(AuthorName = "Company User", Copyright = "Company INC")]
        public sealed class AuthorAndCopyrightPropertiesSample { }

        [CodeSource("https://example.com", AuthorName = "User", Copyright = "Company INC", Version = "2.0")]
        public sealed class ClassAndMethodSample
        {
            [CodeSource("https://example.com/articles/use-async",
                AuthorName = "User",
                Copyright = "Company INC",
                AppliedOn = "2022-12-12",
                Comment = "This is how to use async",
                Version = "1.3")]
            public async Task RunAsync()
            {
                await Task.CompletedTask;
            }
        }

        [CodeSource("https://example.com/blog/retry-with-backoff",
            AuthorName = "Jane Doe",
            Copyright = "Example Ltd",
            AppliedOn = "2026-09-29",
            Comment = "Retry policy adapted from the blog post",
            Version = "1.1",
            Tags = "resilience;http",
            RelatedTaskId = "#123")]
        public sealed class AllPropertiesSample { }

        [CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC")]
        public sealed class NullUrlArgumentsSample { }

        [CodeSource("https://example.com", authorName: "John")]
        public sealed class NamedAuthorArgumentSample { }

        [CodeSource("https://example.com/blog/retry-with-backoff",
            authorName: "Jane Doe",
            copyright: "Example Ltd",
            appliedOn: "2026-09-29",
            comment: "Retry policy adapted from the blog post",
            version: "1.1",
            workItemId: "#123",
            tags: "resilience;http")]
        public sealed class AllArgumentsSample { }

        [CodeSource(sourceUrl: "https://example.com", authorName: "User", copyright: "Company INC", version: "2.0")]
        public sealed class FourNamedArgumentsSample { }

        [CodeSource("https://example.com", Version = "1.5")]
        public sealed class UrlAndVersionPropertySample { }

        [CodeSource("https://example.com", AuthorName = "John", Version = "2.0")]
        public sealed class AuthorAndVersionPropertiesSample { }

        [CodeSource("https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", AppliedOn = "2026-01-15", Version = "1.0")]
        [CodeSource("https://example.com/blog/retry-with-jitter", AuthorName = "John Roe", AppliedOn = "2026-09-29", Comment = "Added jitter", Version = "1.1")]
        public sealed class TwoApplicationsSample { }
    }
}
