using System.Linq;
using NUnit.Framework;
using RzR.Core.CodeSource;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Attributes
{
    [TestFixture]
    public class CopyrightPrefixTests
    {
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
            var attribute = CodeSourceAttributeProbe.Single(typeof(BindingFixtures.LeadingWhitespacePrefixedCopyright));

            Assert.Multiple(() =>
            {
                Assert.That(attribute.Copyright.Count(character => character == '\u00A9'), Is.EqualTo(1), attribute.Copyright);
                Assert.That(attribute.Copyright.TrimStart(), Does.StartWith("\u00A9 ACME"));
            });
        }
    }
}
