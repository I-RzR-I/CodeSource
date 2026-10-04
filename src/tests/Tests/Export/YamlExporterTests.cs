using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using Tests.Support;
using YamlDotNet.RepresentationModel;

namespace Tests.Export
{
    [TestFixture]
    public class YamlExporterTests
    {
        [TestCaseSource(nameof(YamlPayloadCases))]
        public void Yaml_Payload_RoundTripsAsDoubleQuotedScalarWithoutExtraKeys(string payload, string expected)
        {
            var text = ExportRunner.ExportText(ExportFormats.Yaml, CodeSourceResults.Uniform(payload));
            var entries = YamlDocumentReader.LoadEntries(text);

            Assert.That(entries, Has.Count.EqualTo(2), "parent + child entries under codeSources");

            Assert.Multiple(() =>
            {
                foreach (var entry in entries)
                {
                    Assert.That(YamlDocumentReader.Keys(entry), Is.EquivalentTo(new[] { "fullName", "name", "history" }));
                    YamlDocumentReader.AssertQuotedScalar(entry, "fullName", expected);
                    YamlDocumentReader.AssertQuotedScalar(entry, "name", expected);

                    var history = YamlDocumentReader.Get(entry, "history") as YamlSequenceNode;
                    Assert.That(history, Is.Not.Null, "history must be a sequence");
                    if (history == null)
                        continue;

                    Assert.That(history.Children, Has.Count.EqualTo(1));
                    var change = (YamlMappingNode)history.Children[0];
                    Assert.That(YamlDocumentReader.Keys(change), Is.EquivalentTo(ExportSchema.HistoryKeys));
                    foreach (var key in ExportSchema.TextHistoryKeys)
                        YamlDocumentReader.AssertQuotedScalar(change, key, expected);
                    YamlDocumentReader.AssertQuotedScalar(change, ExportSchema.AppliedOnKey, string.Empty);
                }
            });
        }

        [Test]
        public void Yaml_NullValues_AreEmittedAsEmptyQuotedStrings()
        {
            var entries = YamlDocumentReader.LoadEntries(
                ExportRunner.ExportText(ExportFormats.Yaml, CodeSourceResults.Uniform(null)));

            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                foreach (var entry in entries)
                {
                    YamlDocumentReader.AssertQuotedScalar(entry, "fullName", string.Empty);
                    YamlDocumentReader.AssertQuotedScalar(entry, "name", string.Empty);
                    var change = (YamlMappingNode)((YamlSequenceNode)YamlDocumentReader.Get(entry, "history")).Children[0];
                    foreach (var key in ExportSchema.HistoryKeys)
                        YamlDocumentReader.AssertQuotedScalar(change, key, string.Empty);
                }
            });
        }

        [Test]
        public void Yaml_EmptyResults_WritesCodeSourcesWithoutEntries()
        {
            var root = YamlDocumentReader.LoadRoot(
                ExportRunner.ExportText(ExportFormats.Yaml, Array.Empty<CodeSourceObjectsResult>()));

            Assert.That(YamlDocumentReader.Get(root, YamlDocumentReader.RootKey), Is.InstanceOf<YamlScalarNode>(),
                "no entries");
        }

        private static IEnumerable<TestCaseData> YamlPayloadCases()
        {
            var implicitTyped = new[]
            {
                "yes", "no", "on", "off", "y", "n", "true", "false", "null", "Null", "~", ".inf", "-.inf", ".nan",
                "0x1F", "0o17", "012", "1e3", "1_000", "2024-01-01", "12:30:45"
            };
            for (var i = 0; i < implicitTyped.Length; i++)
                yield return NamedCase.Of($"implicit_{i}_{Regex.Replace(implicitTyped[i], "[^A-Za-z0-9]", "_")}",
                    implicitTyped[i], implicitTyped[i]);

            yield return NamedCase.Of("quote_and_backslash", "a\"b\\c", "a\"b\\c");
            yield return NamedCase.Of("newline_key_injection", "x\ny: z", "x\ny: z");
            yield return NamedCase.Of("cr", "a\rb", "a\rb");
            yield return NamedCase.Of("crlf", "a\r\nb", "a\r\nb");
            yield return NamedCase.Of("tab", "a\tb", "a\tb");
            yield return NamedCase.Of("nul", "a\u0000b", "a\u0000b");
            yield return NamedCase.Of("bell", "a\u0007b", "a\u0007b");
            yield return NamedCase.Of("esc", "a\u001Bb", "a\u001Bb");
            yield return NamedCase.Of("del", "a\u007Fb", "a\u007Fb");
            yield return NamedCase.Of("c1", "a\u0090b", "a\u0090b");
            yield return NamedCase.Of("nel", "a\u0085b", "a\u0085b");
            yield return NamedCase.Of("line_separator", "a\u2028b", "a\u2028b");
            yield return NamedCase.Of("paragraph_separator", "a\u2029b", "a\u2029b");
            yield return NamedCase.Of("bom", "a\uFEFFb", "a\uFEFFb");
            yield return NamedCase.Of("noncharacter_fffe", "a\uFFFEb", "a\uFFFEb");
            yield return NamedCase.Of("noncharacter_ffff", "a\uFFFFb", "a\uFFFFb");
            yield return NamedCase.Of("lone_high_surrogate", "a\uD800b", "a\uFFFDb");
            yield return NamedCase.Of("lone_low_surrogate", "a\uDC00b", "a\uFFFDb");
            yield return NamedCase.Of("surrogate_pair", "a\uD83D\uDE00b", "a\uD83D\uDE00b");
            yield return NamedCase.Of("mapping_comment", "a: b #c", "a: b #c");
            yield return NamedCase.Of("sequence_entry", "- x", "- x");
            yield return NamedCase.Of("complex_key", "? x", "? x");
            yield return NamedCase.Of("flow_sequence", "[a, b]", "[a, b]");
            yield return NamedCase.Of("flow_mapping", "{a: b}", "{a: b}");
            yield return NamedCase.Of("single_quotes", "'q'", "'q'");
            yield return NamedCase.Of("anchor", "&a x", "&a x");
            yield return NamedCase.Of("alias", "*a", "*a");
            yield return NamedCase.Of("tag", "!!int 1", "!!int 1");
            yield return NamedCase.Of("directive", "%YAML 1.2", "%YAML 1.2");
            yield return NamedCase.Of("document_start", "---", "---");
            yield return NamedCase.Of("document_end", "...", "...");
            yield return NamedCase.Of("literal_block", "|", "|");
            yield return NamedCase.Of("folded_block", ">", ">");
            yield return NamedCase.Of("comment", "#c", "#c");
            yield return NamedCase.Of("reserved_at", "@x", "@x");
            yield return NamedCase.Of("reserved_backtick", "`x", "`x");
            yield return NamedCase.Of("generic_type", "System.Collections.Generic.List`1[[System.String, mscorlib]]",
                "System.Collections.Generic.List`1[[System.String, mscorlib]]");
            yield return NamedCase.Of("nested_generic", "Outer+Inner<T>.M(Dictionary<string, List<int>>)",
                "Outer+Inner<T>.M(Dictionary<string, List<int>>)");
        }
    }
}
