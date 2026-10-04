using System;
using System.Linq;
using System.Reflection;
using TempLib;
using TempLib.Fixtures;

namespace Tests.Support
{
    internal static class FixtureCatalog
    {
        internal static readonly string[] TempLibParents =
        {
            typeof(OwnClassData).FullName,
            typeof(TempClassData).FullName
        };

        internal static readonly Type[] InvalidAppliedOnTypes =
        {
            typeof(BindingFixtures.AppliedOnNotADate),
            typeof(BindingFixtures.AppliedOnOutOfRange),
            typeof(BindingFixtures.PropertiesInvalidAppliedOn)
        };

        internal static readonly Type[] ValidAppliedOnTypes =
        {
            typeof(BindingFixtures.AppliedOnValid),
            typeof(BindingFixtures.AppliedOnPadded),
            typeof(BindingFixtures.PropertiesOnly)
        };

        internal static readonly Type[] InvalidAppliedOnReportingTypes =
            InvalidAppliedOnTypes.Concat(new[] { typeof(BindingFixtures.InvalidAppliedOnOnMethod) }).ToArray();

        internal static Assembly TempLibAssembly => typeof(OwnClassData).Assembly;

        internal static Assembly FixturesAssembly => typeof(BindingFixtures).Assembly;
    }
}
