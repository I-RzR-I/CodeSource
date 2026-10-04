using NUnit.Framework;

namespace Tests.Support
{
    internal static class NamedCase
    {
        internal static TestCaseData Of(string label, params object[] args)
            => new TestCaseData(args).SetName("{m}(" + label + ")");
    }
}
