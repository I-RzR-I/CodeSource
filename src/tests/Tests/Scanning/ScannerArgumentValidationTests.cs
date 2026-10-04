using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Tests.Support;

namespace Tests.Scanning
{
    [TestFixture]
    [NonParallelizable]
    public class ScannerArgumentValidationTests
    {
        private const string ProbeAssemblyName = "PathProbe";

        private const string ProbeFileName = ProbeAssemblyName + ".dll";

        private const string ProbeSource = "namespace P { public class C { } }";

        public enum PathForm
        {
            AbsoluteBackslash,
            AbsoluteForwardSlash,
            Relative
        }

        private static CodeSourceScanner Scanner => CodeSourceScanner.Instance;

        [Test]
        public void FindAnnotations_NullAssembly_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((Assembly)null).ToList(),
                    ThrowsNull("assembly"));
                Assert.That(() => Scanner.FindAnnotations((Assembly)null, new CodeSourceScanOptions()).ToList(),
                    ThrowsNull("assembly"));
            });
        }

        [Test]
        public void FindAnnotations_NullAssemblyList_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((IEnumerable<Assembly>)null).ToList(),
                    ThrowsNull("assemblies"));
                Assert.That(() => Scanner.FindAnnotations((IEnumerable<Assembly>)null, new CodeSourceScanOptions()).ToList(),
                    ThrowsNull("assemblies"));
            });
        }

        [Test]
        public void FindAnnotations_NullAssemblyName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations((string)null).ToList(),
                    ThrowsNull("assemblyName"));
                Assert.That(() => Scanner.FindAnnotations((string)null, new CodeSourceScanOptions()).ToList(),
                    ThrowsNull("assemblyName"));
            });
        }

        [TestCase("", TestName = "{m}(empty)")]
        [TestCase("   ", TestName = "{m}(spaces)")]
        public void FindAnnotations_EmptyOrWhitespaceAssemblyName_ThrowsArgumentException(string assemblyName)
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Scanner.FindAnnotations(assemblyName).ToList(),
                    ThrowsArgument("assemblyName"));
                Assert.That(() => Scanner.FindAnnotations(assemblyName, new CodeSourceScanOptions()).ToList(),
                    ThrowsArgument("assemblyName"));
            });
        }

        [TestCase(PathForm.AbsoluteBackslash)]
        [TestCase(PathForm.AbsoluteForwardSlash)]
        [TestCase(PathForm.Relative)]
        public void FindAnnotations_PathLikeName_ThrowsAndNeverOpensTheFile(PathForm form)
        {
            var folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "cs-pathprobe-" + Guid.NewGuid().ToString("N")));
            var probe = Path.Combine(folder.FullName, ProbeFileName);
            var original = Environment.CurrentDirectory;
            try
            {
                File.WriteAllBytes(probe, TestCompiler.Emit(TestCompiler.Compile(
                    ProbeAssemblyName, false, new[] { TestCompiler.NetStandard }, ProbeSource)));
                try
                {
                    if (form == PathForm.Relative)
                        Environment.CurrentDirectory = folder.Parent.FullName;

                    var name = form switch
                    {
                        PathForm.AbsoluteBackslash => probe,
                        PathForm.AbsoluteForwardSlash => probe.Replace('\\', '/'),
                        _ => Path.GetRelativePath(Environment.CurrentDirectory, probe)
                    };
                    Assert.That(Path.IsPathRooted(name), Is.EqualTo(form != PathForm.Relative), $"the {form} name is '{name}'");

                    var thrown = Assert.Catch(() => Scanner.FindAnnotations(name).ToList());

                    Assert.Multiple(() =>
                    {
                        Assert.That(thrown, Is.InstanceOf<ArgumentException>()
                            .Or.InstanceOf<FileLoadException>()
                            .Or.InstanceOf<FileNotFoundException>(), thrown?.ToString());
                        Assert.That(LoadedAssemblyLocations(), Has.None.EqualTo(probe).IgnoreCase, "the probe must never be loaded");
                    });
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                }
            }
            finally
            {
                try
                {
                    folder.Delete(recursive: true);
                }
                catch (IOException ex)
                {
                    TestContext.WriteLine($"cleanup skipped ({ex.GetType().Name}): {folder.FullName}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    TestContext.WriteLine($"cleanup skipped ({ex.GetType().Name}): {folder.FullName}");
                }
            }
        }

        private static IResolveConstraint ThrowsNull(string paramName)
            => Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo(paramName);

        private static IResolveConstraint ThrowsArgument(string paramName)
            => Throws.TypeOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo(paramName);

        private static IEnumerable<string> LoadedAssemblyLocations()
            => AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .Where(assembly => !assembly.IsDynamic)
                .Select(assembly => assembly.Location);
    }
}
