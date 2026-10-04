using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class DynamicAssemblies
    {
        internal const string ScannerProbeName = "CodeSourceScannerTests.Dynamic";

        private const string ScannerProbeTypeName = "DynamicType";

        private static readonly Lazy<AssemblyBuilder> ScannerProbeBuilder = new Lazy<AssemblyBuilder>(
            () => Define(ScannerProbeName, ScannerProbeTypeName).Assembly,
            LazyThreadSafetyMode.ExecutionAndPublication);

        internal static AssemblyBuilder ScannerProbe => ScannerProbeBuilder.Value;

        internal static (AssemblyBuilder Assembly, Type Type) Define(string assemblyName, string typeName, string sourceUrl = null)
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var type = assembly.DefineDynamicModule(assemblyName).DefineType(typeName, TypeAttributes.Public);
            if (sourceUrl != null)
            {
                var constructor = typeof(CodeSourceAttribute).GetConstructor(new[] { typeof(string) });
                type.SetCustomAttribute(new CustomAttributeBuilder(constructor, new object[] { sourceUrl }));
            }

            return (assembly, type.CreateType());
        }
    }
}
