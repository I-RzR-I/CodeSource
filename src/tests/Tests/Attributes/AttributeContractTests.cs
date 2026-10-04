using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using Tests.Support;

namespace Tests.Attributes
{
    [TestFixture]
    public class AttributeContractTests
    {
        [Test]
        public void CodeSourceAttribute_AttributeUsage_IsAllWithAllowMultipleAndInherited()
        {
            var usage = typeof(CodeSourceAttribute).GetCustomAttribute<AttributeUsageAttribute>(false);

            Assert.Multiple(() =>
            {
                Assert.That(usage, Is.Not.Null);
                Assert.That(usage?.ValidOn, Is.EqualTo(AttributeTargets.All));
                Assert.That(usage?.AllowMultiple, Is.True);
                Assert.That(usage?.Inherited, Is.True);
            });
        }

        [Test]
        public void CodeSourceAttribute_Type_IsSealedAndImplementsICodeSourceAttribute()
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeof(CodeSourceAttribute).IsSealed, Is.True);
                Assert.That(typeof(ICodeSourceAttribute).IsAssignableFrom(typeof(CodeSourceAttribute)), Is.True);
                Assert.That(typeof(Attribute).IsAssignableFrom(typeof(CodeSourceAttribute)), Is.True);
            });
        }

        [TestCase(CtorSignature.LegacySourceUrlVersion)]
        [TestCase(CtorSignature.LegacySourceUrlAuthorVersion)]
        [TestCase(CtorSignature.SourceUrlAuthorCopyrightVersion)]
        public void PositionalConstructor_Reflected_HasNoOptionalParameters(CtorSignature signature)
        {
            var constructor = CtorSignatures.Get(signature);

            var optional = constructor.GetParameters().Where(parameter => parameter.HasDefaultValue || parameter.IsOptional).Select(parameter => parameter.Name);

            Assert.That(optional, Is.Empty, $"ctor {signature} must not declare default values");
        }

        [Test]
        public void SourceUrlConstructor_Reflected_HasOneRequiredParameter()
        {
            var constructor = CtorSignatures.Get(CtorSignature.SourceUrl);

            Assert.That(constructor.GetParameters().Single().HasDefaultValue, Is.False);
        }

        [Test]
        public void FullConstructor_Reflected_KeepsItsSignatureAndDefaults()
        {
            var defaults = CtorSignatures.Get(CtorSignature.Full).GetParameters()
                .Skip(1)
                .Select(parameter => parameter.HasDefaultValue ? parameter.DefaultValue : "<no default>");

            Assert.That(defaults, Is.EqualTo(new object[] { null, null, null, null, "1.0", null, null }));
        }

        [TestCase(CtorSignature.LegacySourceUrlVersion)]
        [TestCase(CtorSignature.LegacySourceUrlAuthorVersion)]
        public void LegacyPositionalConstructor_Reflected_IsObsoleteWarning(CtorSignature signature)
        {
            var obsolete = CtorSignatures.Get(signature).GetCustomAttribute<ObsoleteAttribute>();

            Assert.Multiple(() =>
            {
                Assert.That(obsolete, Is.Not.Null, $"ctor {signature} must carry [Obsolete]");
                Assert.That(obsolete?.IsError, Is.False, "warning, not error");
                Assert.That(obsolete?.Message, Does.Contain("CodeSource(sourceUrl)"));
                Assert.That(obsolete?.Message, Does.Contain("named properties"));
                Assert.That(obsolete?.Message, Does.Contain("Positional arguments bind by position"));
            });
        }

        [TestCase(CtorSignature.Parameterless)]
        [TestCase(CtorSignature.SourceUrlAuthorCopyrightVersion)]
        [TestCase(CtorSignature.Full)]
        [TestCase(CtorSignature.SourceUrl)]
        public void NonLegacyConstructor_Reflected_IsNotObsolete(CtorSignature signature)
        {
            var obsolete = CtorSignatures.Get(signature).GetCustomAttribute<ObsoleteAttribute>();

            Assert.That(obsolete, Is.Null);
        }
    }
}
