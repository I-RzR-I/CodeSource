using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using Tests.Support;

namespace Tests.Conditional
{
    [TestFixture]
    public class ConditionalAttributeTests
    {
        [Test]
        public void CodeSourceAttribute_ConditionalAttribute_RequiresCodeSourceSymbol()
        {
            var conditionals = typeof(CodeSourceAttribute).GetCustomAttributes<ConditionalAttribute>(false).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(conditionals.Select(conditional => conditional.ConditionString), Is.EqualTo(new[] { ConditionalSources.ExpectedSymbol }));
                Assert.That(CodeSourceAttribute.ConditionalSymbol, Is.EqualTo(ConditionalSources.ExpectedSymbol));
            });
        }

        [Test]
        public void CodeSourceProject_TargetFrameworks_MatchTestCases()
        {
            Assert.That(CodeSourceBuildOutput.ProjectTargetFrameworks(), Is.EquivalentTo(CodeSourceBuildOutput.TargetFrameworks),
                $"TargetFrameworks in '{CodeSourceBuildOutput.ProjectPath}'");
        }
    }
}
