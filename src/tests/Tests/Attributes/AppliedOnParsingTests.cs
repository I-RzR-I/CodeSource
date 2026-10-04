using System;
using NUnit.Framework;
using RzR.Core.CodeSource;
using TempLib.Fixtures;
using Tests.Support;

namespace Tests.Attributes
{
    [TestFixture]
    public class AppliedOnParsingTests
    {
        [TestCase(typeof(BindingFixtures.AppliedOnValid))]
        [TestCase(typeof(BindingFixtures.AppliedOnPadded))]
        [TestCase(typeof(BindingFixtures.FivePositional))]
        [TestCase(typeof(BindingFixtures.EightPositional))]
        [TestCase(typeof(BindingFixtures.PropertiesOnly))]
        public void AppliedOn_ValidDate_ParsesToInternalDate(Type fixture)
        {
            var attribute = CodeSourceAttributeProbe.Single(fixture);

            Assert.That(attribute.InternalAppliedOn, Is.EqualTo(new DateTime(2022, 12, 12)));
        }

        [TestCase(typeof(BindingFixtures.AppliedOnNotADate))]
        [TestCase(typeof(BindingFixtures.AppliedOnOutOfRange))]
        [TestCase(typeof(BindingFixtures.PropertiesInvalidAppliedOn))]
        [TestCase(typeof(BindingFixtures.SourceUrlOnly))]
        public void AppliedOn_InvalidOrMissing_GivesNullInternalDateWithoutThrowing(Type fixture)
        {
            CodeSourceAttribute attribute = null;

            Assert.DoesNotThrow(() => attribute = CodeSourceAttributeProbe.Single(fixture));
            Assert.That(attribute.InternalAppliedOn, Is.Null);
        }

        [TestCase("x")]
        [TestCase("2022-13-40")]
        [TestCase("12/12/2022")]
        [TestCase("   ", TestName = "{m}(spaces)")]
        public void AppliedOn_InvalidValueThroughFullConstructor_DoesNotThrowAndKeepsRawValue(string appliedOn)
        {
            CodeSourceAttribute attribute = null;

            Assert.DoesNotThrow(() => attribute = new CodeSourceAttribute("http://fixture.local/direct", appliedOn: appliedOn));
            Assert.Multiple(() =>
            {
                Assert.That(attribute.InternalAppliedOn, Is.Null);
                Assert.That(attribute.AppliedOn, Is.EqualTo(appliedOn));
            });
        }
    }
}
