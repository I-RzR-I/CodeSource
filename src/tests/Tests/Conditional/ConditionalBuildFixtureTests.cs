using System;
using NUnit.Framework;
using TempLib;
using TempLib.Fixtures;
using TempLib.NoSymbol;
using Tests.Support;

namespace Tests.Conditional
{
    [TestFixture]
    public class ConditionalBuildFixtureTests
    {
        [Test]
        public void NoSymbolBuild_Image_HasNoCodeSourceRowsOrReference()
        {
            var image = PeImage.Read(typeof(NoSymbolAnnotated).Assembly);

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.Empty);
                Assert.That(PeImage.AssemblyReferenceNames(image), Has.Member(TestCompiler.NetStandardAssemblyName), "the AssemblyRef table was read");
                Assert.That(PeImage.AssemblyReferenceNames(image), Has.No.Member(TestCompiler.CodeSourceAssemblyName));
            });
        }

        [TestCase(typeof(OwnClassData))]
        [TestCase(typeof(BindingFixtures))]
        public void SymbolDefinedBuild_Image_HasCodeSourceRowsAndReference(Type marker)
        {
            var image = PeImage.Read(marker.Assembly);

            Assert.Multiple(() =>
            {
                Assert.That(PeImage.CodeSourceUrls(image), Is.Not.Empty);
                Assert.That(PeImage.AssemblyReferenceNames(image), Has.Member(TestCompiler.CodeSourceAssemblyName));
            });
        }
    }
}
