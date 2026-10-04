using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using Tests.Support;

namespace Tests.Conditional
{
    [TestFixture]
    public class CodeSourceBuildOutputTests
    {
        [OneTimeSetUp]
        public void RequireFreshBinRoot()
        {
#if !NCRUNCH
            CodeSourceBuildOutput.AssertBinRootIsFresh();
#endif
        }

        [TestCaseSource(typeof(CodeSourceBuildOutput), nameof(CodeSourceBuildOutput.TargetFrameworks))]
        public void CodeSourceAttribute_BuiltTargetFramework_KeepsItsPublicContract(string targetFramework)
        {
            var image = CodeSourceBuildOutput.ReadImage(targetFramework);
            var attributeType = typeof(CodeSourceAttribute);

            var typeFlags = PeImage.TypeFlags(image, attributeType);
            var conditionals = PeImage.CustomAttributeValues(image, attributeType, typeof(ConditionalAttribute));
            var usages = PeImage.CustomAttributeValues(image, attributeType, typeof(AttributeUsageAttribute));
            var symbol = PeImage.FieldConstant(image, attributeType, nameof(CodeSourceAttribute.ConditionalSymbol));

            Assert.Multiple(() =>
            {
                Assert.That(typeFlags & TypeAttributes.Sealed, Is.EqualTo(TypeAttributes.Sealed));
                Assert.That(conditionals, Has.Count.EqualTo(1));
                Assert.That(conditionals.SelectMany(conditional => conditional.FixedArguments).Select(argument => argument.Value),
                    Is.EqualTo(new object[] { ConditionalSources.ExpectedSymbol }));
                Assert.That(usages, Has.Count.EqualTo(1));
                Assert.That(usages.SelectMany(usage => usage.FixedArguments).Select(argument => argument.Value),
                    Is.EqualTo(new object[] { (int)AttributeTargets.All }));
                Assert.That(usages.SelectMany(usage => usage.NamedArguments).Select(argument => $"{argument.Name}={argument.Value}"),
                    Is.EqualTo(new[] { "AllowMultiple=True" }));
                Assert.That(symbol.Attributes & FieldAttributes.FieldAccessMask, Is.EqualTo(FieldAttributes.Public));
                Assert.That(symbol.Attributes & (FieldAttributes.Static | FieldAttributes.Literal),
                    Is.EqualTo(FieldAttributes.Static | FieldAttributes.Literal));
                Assert.That(symbol.Value, Is.EqualTo(ConditionalSources.ExpectedSymbol));
            });
        }
    }
}
