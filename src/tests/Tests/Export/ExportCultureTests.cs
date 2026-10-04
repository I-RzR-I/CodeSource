using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class ExportCultureTests
    {
        [TestCaseSource(typeof(BuiltInExporters), nameof(BuiltInExporters.Formats))]
        public void AppliedOn_UnderThaiCulture_StaysGregorian(string format)
        {
            var thai = (CultureInfo)CultureInfo.GetCultureInfo("th-TH").Clone();
            thai.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
            Assert.That(new DateTime(2022, 12, 12).ToString("yyyy-MM-dd", thai), Is.Not.EqualTo("2022-12-12"),
                "precondition: the th-TH culture under test must use the Thai Buddhist calendar");

            var items = new List<CodeSourceObjectsResult>
            {
                CodeSourceResults.Result(
                    CodeSourceResults.NewObject("P", "Ns.P",
                        CodeSourceResults.History(codePath: "Ns.P", appliedOn: new DateTime(2022, 12, 12))),
                    CodeSourceResults.NewObject("M", "Ns.P.M",
                        CodeSourceResults.History(codePath: "Ns.P.M", appliedOn: new DateTime(2023, 1, 31))))
            };

            var invariant = ExportUnderCulture(format, items, CultureInfo.InvariantCulture);
            var thaiBytes = ExportUnderCulture(format, items, thai);
            var text = ExportRunner.Decode(thaiBytes);

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("2022-12-12"));
                Assert.That(text, Does.Contain("2023-01-31"));
                Assert.That(text, Does.Not.Contain("2565-12-12"));
                Assert.That(text, Does.Not.Contain("2566-01-31"));
                Assert.That(thaiBytes, Is.EqualTo(invariant), "output must not depend on the current culture");
            });
        }

        private static byte[] ExportUnderCulture(string format, IEnumerable<CodeSourceObjectsResult> items,
            CultureInfo culture)
        {
            var thread = Thread.CurrentThread;
            var originalCulture = thread.CurrentCulture;
            var originalUiCulture = thread.CurrentUICulture;
            try
            {
                thread.CurrentCulture = culture;
                thread.CurrentUICulture = culture;
                return ExportRunner.Export(format, items);
            }
            finally
            {
                thread.CurrentCulture = originalCulture;
                thread.CurrentUICulture = originalUiCulture;
            }
        }
    }
}
