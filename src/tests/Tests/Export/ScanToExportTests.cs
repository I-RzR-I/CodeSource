using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class ScanToExportTests
    {
        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void Export_ScannedTempLib_ContainsEveryAnnotatedParent(string format)
        {
            var errors = new List<CodeSourceScanError>();
            var results = ScanRunner.Scan(FixtureCatalog.TempLibAssembly, errors);

            var text = ExportRunner.ExportTextViaRegistry(format, results);

            Assert.Multiple(() =>
            {
                Assert.That(errors, Is.Empty, ScanRunner.DescribeErrors(errors));
                foreach (var parent in FixtureCatalog.TempLibParents)
                    Assert.That(text, Does.Match(Regex.Escape(parent) + @"(?![.\w])"), $"{format} output must name '{parent}'");
            });
        }
    }
}
