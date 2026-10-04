using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class CodeSourceAttributeProbe
    {
        internal static CustomAttributeData SingleData(MemberInfo member)
            => AllData(member).Single();

        internal static CodeSourceAttribute Single(MemberInfo member)
            => All(member).Single();

        internal static List<CustomAttributeData> AllData(MemberInfo member)
            => member.GetCustomAttributesData().Where(data => data.AttributeType == typeof(CodeSourceAttribute)).ToList();

        internal static List<CodeSourceAttribute> All(MemberInfo member)
            => member.GetCustomAttributes<CodeSourceAttribute>(inherit: false).ToList();

        internal static List<string> BoundParameterNames(CustomAttributeData data)
            => data.Constructor.GetParameters().Select(parameter => parameter.Name).ToList();
    }
}
