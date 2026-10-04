using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;
using RzR.Core.CodeSource;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class JsonExporterTests
    {
        [TestCaseSource(nameof(JsonPayloadCases))]
        public void Json_Payload_RoundTrips(string payload)
        {
            var text = ExportRunner.ExportText(ExportFormats.Json, CodeSourceResults.Uniform(payload));

            using var document = JsonDocument.Parse(text);
            var item = document.RootElement[0];
            var parent = item.GetProperty("parent");
            var child = item.GetProperty("children")[0];

            Assert.Multiple(() =>
            {
                foreach (var obj in new[] { parent, child })
                {
                    Assert.That(obj.GetProperty("name").GetString(), Is.EqualTo(payload));
                    Assert.That(obj.GetProperty("fullName").GetString(), Is.EqualTo(payload));
                    var change = obj.GetProperty("codeChanges")[0];
                    foreach (var key in ExportSchema.TextHistoryKeys)
                        Assert.That(change.GetProperty(key).GetString(), Is.EqualTo(payload), key);
                }
            });
        }

        private static IEnumerable<TestCaseData> JsonPayloadCases()
        {
            yield return NamedCase.Of("quote_and_backslash", "a\"b\\c");
            yield return NamedCase.Of("newline", "x\ny");
            yield return NamedCase.Of("soh", "a\u0001b");
            yield return NamedCase.Of("script_close", "</script>");
            yield return NamedCase.Of("line_separator", "a\u2028b");
            yield return NamedCase.Of("tab", "a\tb");
            yield return NamedCase.Of("surrogate_pair", "a\uD83D\uDE00b");
        }
    }
}
