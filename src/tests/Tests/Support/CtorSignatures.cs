using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class CtorSignatures
    {
        internal static string[] ParameterNames(CtorSignature signature)
            => signature switch
            {
                CtorSignature.Parameterless => new string[0],
                CtorSignature.SourceUrl => new[] { "sourceUrl" },
                CtorSignature.LegacySourceUrlVersion => new[] { "sourceUrl", "version" },
                CtorSignature.LegacySourceUrlAuthorVersion => new[] { "sourceUrl", "authorName", "version" },
                CtorSignature.SourceUrlAuthorCopyrightVersion => new[] { "sourceUrl", "authorName", "copyright", "version" },
                CtorSignature.Full => new[] { "sourceUrl", "authorName", "copyright", "appliedOn", "comment", "version", "workItemId", "tags" },
                _ => throw new ArgumentOutOfRangeException(nameof(signature), signature, null)
            };

        internal static ConstructorInfo Get(CtorSignature signature)
        {
            var names = ParameterNames(signature);
            var constructor = typeof(CodeSourceAttribute).GetConstructor(Enumerable.Repeat(typeof(string), names.Length).ToArray());

            Assert.That(constructor, Is.Not.Null, $"ctor {signature} ({string.Join(", ", names)}) must exist");
            Assert.That(constructor.GetParameters().Select(parameter => parameter.Name), Is.EqualTo(names), $"ctor {signature} parameter names");

            return constructor;
        }
    }
}
