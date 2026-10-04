using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;

namespace Tests.Support
{
    internal static class ScanRunner
    {
        internal static List<CodeSourceObjectsResult> Scan(Assembly assembly, List<CodeSourceScanError> errors)
            => CodeSourceScanner.Instance.FindAnnotations(assembly, CollectInto(errors)).ToList();

        internal static List<CodeSourceObjectsResult> Scan(IEnumerable<Assembly> assemblies, List<CodeSourceScanError> errors)
            => CodeSourceScanner.Instance.FindAnnotations(assemblies, CollectInto(errors)).ToList();

        internal static List<string> ParentNames(IEnumerable<CodeSourceObjectsResult> results)
            => results.Select(result => result.Parent?.FullName).ToList();

        internal static string DescribeErrors(IEnumerable<CodeSourceScanError> errors)
            => Environment.NewLine + "Reported errors:" + string.Concat(errors.Select(error =>
                $"{Environment.NewLine}  {error.Stage} | {error.AssemblyName} | {error.TypeName} | {error.MemberName} | {error.Exception?.GetType().FullName}"));

        private static CodeSourceScanOptions CollectInto(List<CodeSourceScanError> errors)
            => new CodeSourceScanOptions { OnError = errors.Add };
    }
}
