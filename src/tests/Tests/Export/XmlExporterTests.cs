using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Extensions.Internal.Builder;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class XmlExporterTests
    {
        private static readonly string[] XmlElementNames =
        {
            "codeSources", "codeSource", "parent", "children", "child", "name", "fullName", "codeChanges",
            "codeChange", "codePath", "sourceUrl", "authorName", "copyright", "appliedOn", "comment", "version",
            "tags", "relatedTaskId"
        };

        [TestCaseSource(nameof(XmlPayloadCases))]
        public void Xml_Payload_LoadsStrictlyAndRoundTrips(string payload, string expected)
        {
            var bytes = ExportRunner.Export(ExportFormats.Xml, CodeSourceResults.Uniform(payload));

            Assert.That(ExportRunner.Decode(bytes), Does.Not.Contain("<!DOCTYPE"), "no DTD may be emitted");
            var document = StrictXml.Load(bytes);

            var elementNames = document.SelectNodes("//*").Cast<XmlElement>().Select(x => x.LocalName).Distinct();
            Assert.That(elementNames, Is.SubsetOf(XmlElementNames), "data must not inject elements");

            var objects = document
                .SelectNodes("/codeSources/codeSource/parent | /codeSources/codeSource/children/child")
                .Cast<XmlElement>().ToList();
            Assert.That(objects, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                foreach (var obj in objects)
                {
                    Assert.That(obj["name"]?.InnerText, Is.EqualTo(expected), "name");
                    Assert.That(obj["fullName"]?.InnerText, Is.EqualTo(expected), "fullName");

                    var changes = obj.SelectNodes("codeChanges/codeChange").Cast<XmlElement>().ToList();
                    Assert.That(changes, Has.Count.EqualTo(1));
                    foreach (var change in changes)
                    {
                        foreach (var key in ExportSchema.TextHistoryKeys)
                            Assert.That(change[key]?.InnerText, Is.EqualTo(expected), key);
                        Assert.That(change[ExportSchema.AppliedOnKey]?.InnerText, Is.Empty, ExportSchema.AppliedOnKey);
                    }
                }
            });
        }

        [Test]
        public void XmlBuilder_AttributeValue_EscapesMarkupQuotesAndWhitespace()
        {
            const string value = "\"\t\n\r<&'>";

            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                XmlBuilder.WriteXmlElement(writer, string.Empty, "e", "x",
                    new Dictionary<string, string> { ["a"] = value }, inline: true);
            var bytes = stream.ToArray();

            Assert.That(ExportRunner.Decode(bytes), Does.Contain(" a = \"&quot;&#x9;&#xA;"),
                "payload row: \" + TAB + LF");
            Assert.That(StrictXml.Load(bytes).DocumentElement.GetAttribute("a"), Is.EqualTo(value));
        }

        [Test]
        public void Xml_EmptyResults_WritesEmptyCodeSourcesRoot()
        {
            var root = StrictXml.Load(ExportRunner.Export(ExportFormats.Xml, Array.Empty<CodeSourceObjectsResult>()))
                .DocumentElement;

            Assert.That(root?.LocalName, Is.EqualTo("codeSources"));
            Assert.That(root?.ChildNodes.OfType<XmlElement>(), Is.Empty);
        }

        private static IEnumerable<TestCaseData> XmlPayloadCases()
        {
            yield return NamedCase.Of("close_tag_injection", "</codePath><x>", "</codePath><x>");
            yield return NamedCase.Of("entity_text", "&amp;", "&amp;");
            yield return NamedCase.Of("quotes", "a'\"", "a'\"");
            yield return NamedCase.Of("cdata_end", "]]>", "]]>");
            yield return NamedCase.Of("cdata", "<![CDATA[x]]>", "<![CDATA[x]]>");
            yield return NamedCase.Of("doctype_text", "<!DOCTYPE x [<!ENTITY e \"boom\">]>&e;", "<!DOCTYPE x [<!ENTITY e \"boom\">]>&e;");
            yield return NamedCase.Of("cr", "a\rb", "a\rb");
            yield return NamedCase.Of("crlf", "a\r\nb", "a\r\nb");
            yield return NamedCase.Of("lf", "a\nb", "a\nb");
            yield return NamedCase.Of("tab", "a\tb", "a\tb");
            yield return NamedCase.Of("nel_allowed", "a\u0085b", "a\u0085b");
            yield return NamedCase.Of("del_allowed", "a\u007Fb", "a\u007Fb");
            yield return NamedCase.Of("line_separator_allowed", "a\u2028b", "a\u2028b");
            yield return NamedCase.Of("surrogate_pair_allowed", "a\uD83D\uDE00b", "a\uD83D\uDE00b");
            yield return NamedCase.Of("soh", "a\u0001b", "a\uFFFDb");
            yield return NamedCase.Of("nul", "a\u0000b", "a\uFFFDb");
            yield return NamedCase.Of("vt", "a\u000Bb", "a\uFFFDb");
            yield return NamedCase.Of("ff", "a\u000Cb", "a\uFFFDb");
            yield return NamedCase.Of("us", "a\u001Fb", "a\uFFFDb");
            yield return NamedCase.Of("fffe", "a\uFFFEb", "a\uFFFDb");
            yield return NamedCase.Of("ffff", "a\uFFFFb", "a\uFFFDb");
            yield return NamedCase.Of("lone_high_surrogate", "a\uD800b", "a\uFFFDb");
            yield return NamedCase.Of("lone_low_surrogate", "a\uDC00b", "a\uFFFDb");
            yield return NamedCase.Of("reversed_pair", "a\uDC00\uD800b", "a\uFFFD\uFFFDb");
            yield return NamedCase.Of("trailing_high_surrogate", "ab\uD800", "ab\uFFFD");
        }
    }
}
