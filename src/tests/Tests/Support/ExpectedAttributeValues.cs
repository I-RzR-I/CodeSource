using NUnit.Framework;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    public sealed class ExpectedAttributeValues
    {
        public string SourceUrl { get; init; }

        public string AuthorName { get; init; }

        public string Copyright { get; init; }

        public string AppliedOn { get; init; }

        public string Comment { get; init; }

        public string Version { get; init; }

        public string Tags { get; init; }

        public string RelatedTaskId { get; init; }

        public void AssertMatches(CodeSourceAttribute actual)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual.SourceUrl, Is.EqualTo(SourceUrl), nameof(actual.SourceUrl));
                Assert.That(actual.AuthorName, Is.EqualTo(AuthorName), nameof(actual.AuthorName));
                Assert.That(actual.Copyright, Is.EqualTo(Copyright), nameof(actual.Copyright));
                Assert.That(actual.AppliedOn, Is.EqualTo(AppliedOn), nameof(actual.AppliedOn));
                Assert.That(actual.Comment, Is.EqualTo(Comment), nameof(actual.Comment));
                Assert.That(actual.Version, Is.EqualTo(Version), nameof(actual.Version));
                Assert.That(actual.Tags, Is.EqualTo(Tags), nameof(actual.Tags));
                Assert.That(actual.RelatedTaskId, Is.EqualTo(RelatedTaskId), nameof(actual.RelatedTaskId));
            });
        }
    }
}
