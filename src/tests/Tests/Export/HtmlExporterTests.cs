using System;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class HtmlExporterTests
    {
        [Test]
        public void Html_BreakoutPayload_IsFullyEncoded()
        {
            const string payload = "\"><svg onload=a()>";
            var text = ExportRunner.ExportText(ExportFormats.Html, CodeSourceResults.Uniform(payload));

            Assert.That(text, Does.Not.Contain("<svg"));
            Assert.That(text, Does.Contain("&quot;&gt;&lt;svg onload=a()&gt;"));
        }

        [Test]
        public void Html_EmptyResults_WritesTableWithoutDataCells()
        {
            var text = ExportRunner.ExportText(ExportFormats.Html, Array.Empty<CodeSourceObjectsResult>());

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("<table>").And.Contain("</table>").And.Contain("</html>"));
                Assert.That(text, Does.Not.Contain("<td"), "no data cells");
            });
        }
    }
}
