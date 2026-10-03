using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using RzR.Core.CodeSource.Services.Export;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Tests
{
    [TestFixture]
    public class ExporterSafetyTests
    {
        private const string Empty = "\"\"";

        private static readonly string[] HistoryKeys =
        {
            "codePath", "sourceUrl", "authorName", "copyright", "appliedOn", "comment", "version", "tags",
            "relatedTaskId"
        };

        private static readonly string[] XmlElementNames =
        {
            "codeSources", "codeSource", "parent", "children", "child", "name", "fullName", "codeChanges",
            "codeChange", "codePath", "sourceUrl", "authorName", "copyright", "appliedOn", "comment", "version",
            "tags", "relatedTaskId"
        };

        private static IEnumerable<string> Formats() => ExportTestData.AllFormats;

        private static IEnumerable<TestCaseData> CsvFormulaCases()
        {
            yield return Case("hyperlink", "=HYPERLINK(\"http://x\",\"c\")", "\"'=HYPERLINK(\"\"http://x\"\",\"\"c\"\")\"");
            yield return Case("tab_then_equals", "\t=1", "\"'=1\"");
            yield return Case("tab_then_text", "\tX", "\"X\"");
            yield return Case("cr_then_text", "\rX", "\"X\"");
            yield return Case("lf_then_equals", "\n=1", "\"'=1\"");
            yield return Case("space_then_equals", " =1", "\"'=1\"");
            yield return Case("nel_then_equals", "\u0085=1", "\"'=1\"");
            yield return Case("line_separator_then_equals", "\u2028=1", "\"'=1\"");
            yield return Case("nbsp_then_equals", "\u00A0=1", "\"'=1\"");
            yield return Case("already_apostrophe_no_double_prefix", "'=1", "\"'=1\"");
            yield return Case("fullwidth_equals", "\uFF1D1", "\"'\uFF1D1\"");
            yield return Case("fullwidth_plus", "\uFF0B1", "\"'\uFF0B1\"");
            yield return Case("fullwidth_minus", "\uFF0D1", "\"'\uFF0D1\"");
            yield return Case("fullwidth_at", "\uFF20x", "\"'\uFF20x\"");
            yield return Case("plus", "+1", "\"'+1\"");
            yield return Case("minus_formula", "-1+2", "\"'-1+2\"");
            yield return Case("at_sum", "@SUM(A1)", "\"'@SUM(A1)\"");
            yield return Case("negative_number_no_exemption", "-1", "\"'-1\"");
            yield return Case("equals_not_first", "a=1", "\"a=1\"");
            yield return Case("plain_version", "1.10", "\"1.10\"");
        }

        [TestCaseSource(nameof(CsvFormulaCases))]
        public void Csv_FormulaPayload_IsNeutralisedInEveryDataCell(string payload, string expectedCell)
        {
            AssertEveryCsvDataCell(payload, expectedCell);
        }

        private static IEnumerable<TestCaseData> CsvCellStartRuleCases()
        {
            var fullWidthEquals = ((char)0xFF1D).ToString();

            yield return Case("rule_semicolon_equals", "x;=1+1;y", "\"x;'=1+1;y\"");
            yield return Case("rule_semicolon_quoted_formula", "x;\"=1+1\";y", "\"x;'\"\"=1+1\"\";y\"");
            yield return Case("rule_semicolon_space_quote_plus", "x; \"+1", "\"x; '\"\"+1\"");
            yield return Case("rule_comma_space_at", "a, @b", "\"a, '@b\"");
            yield return Case("rule_every_cell", "=a;=b,-c", "\"'=a;'=b,'-c\"");
            yield return Case("rule_lf_then_equals", "x\n=cmd", "\"x\n'=cmd\"");
            yield return Case("rule_semicolon_lf_text_unchanged", "step1;\nstep2", "\"step1;\nstep2\"");
            yield return Case("rule_comma_tab_text_unchanged", "a,\tb", "\"a,\tb\"");
            yield return Case("rule_plain_list_unchanged", "bug;security", "\"bug;security\"");
            yield return Case("rule_negative_after_comma_tradeoff", "1,-1", "\"1,'-1\"");
            yield return Case("rule_quote_without_trigger_unchanged", "x;\"hello\"", "\"x;\"\"hello\"\"\"");
            yield return Case("rule_leading_hyperlink", "=HYPERLINK(\"http://x\",\"c\")",
                "\"'=HYPERLINK(\"\"http://x\"\",\"\"c\"\")\"");

            yield return Case("derived_crlf_then_equals", "x\r\n=cmd", "\"x\r\n'=cmd\"");
            yield return Case("derived_semicolon_lf_equals", "x;\n=1", "\"x;\n'=1\"");
            yield return Case("derived_semicolon_tab_equals", "x;\t=1", "\"x;\t'=1\"");
            yield return Case("derived_lf_quoted_formula", "x\n\"=1\"", "\"x\n'\"\"=1\"\"\"");
            yield return Case("derived_semicolon_fullwidth_equals", "x;" + fullWidthEquals + "1",
                "\"x;'" + fullWidthEquals + "1\"");
        }

        [TestCaseSource(nameof(CsvCellStartRuleCases))]
        public void Csv_CellStartRule_NeutralisesEveryCellStart(string payload, string expectedCell)
        {
            var text = AssertEveryCsvDataCell(payload, expectedCell);

            var parentHistory = ParseCsv(text).Single(r => r.Count == 11 && r[9] == "1" && r[10] == "1");
            Assert.That(parentHistory[5], Is.EqualTo(UnquoteCsv(expectedCell)), "RFC 4180 parse of the Comment cell");
        }

        private static string AssertEveryCsvDataCell(string payload, string expectedCell)
        {
            var text = ExportTestData.ExportText(ExportFormats.Csv, ExportTestData.Uniform(payload));
            var nl = Environment.NewLine;
            var e = expectedCell;

            var parentRow = Row(e, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, "\"1\"", "\"0\"");
            var parentHistoryRow = Row(e, e, e, e, Empty, e, e, e, e, "\"1\"", "\"1\"");
            var childRow = Row(e, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, "\"0\"", "\"0\"");
            var childHistoryRow = Row(e, e, e, e, Empty, e, e, e, e, "\"0\"", "\"1\"");

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain(nl + parentRow + nl), "parent row (FullName cell)");
                Assert.That(text, Does.Contain(nl + parentHistoryRow + nl), "parent history row (every data cell)");
                Assert.That(text, Does.Contain(nl + childRow + nl), "child row (FullName cell)");
                Assert.That(text, Does.Contain(nl + childHistoryRow + nl), "child history row (every data cell)");
            });

            var records = ParseCsv(text);
            Assert.That(records.Count, Is.EqualTo(7), "header + (spacer, parent, history) x 2");
            Assert.That(records.Select(r => r.Count), Is.All.EqualTo(11));

            return text;
        }

        private static string UnquoteCsv(string field)
            => field.Substring(1, field.Length - 2).Replace("\"\"", "\"");

        [Test]
        public void Csv_CommaQuoteCrLf_RoundTripsThroughRfc4180Parser()
        {
            const string payload = "a,\"b\"\r\nc";

            var records = ParseCsv(ExportTestData.ExportText(ExportFormats.Csv, ExportTestData.Uniform(payload)));
            var baseline = ParseCsv(ExportTestData.ExportText(ExportFormats.Csv, ExportTestData.Uniform("plain")));

            Assert.That(records.Count, Is.EqualTo(baseline.Count), "embedded CRLF must not add records");
            Assert.That(records.Select(r => r.Count), Is.All.EqualTo(11));

            var parent = records.Single(r => r[9] == "1" && r[10] == "0");
            var child = records.Single(r => r[9] == "0" && r[10] == "0");
            var histories = records.Where(r => r[10] == "1").ToList();

            Assert.Multiple(() =>
            {
                Assert.That(parent[0], Is.EqualTo(payload));
                Assert.That(child[0], Is.EqualTo(payload));
                Assert.That(histories.Count, Is.EqualTo(2));
                foreach (var history in histories)
                {
                    foreach (var column in new[] { 0, 1, 2, 3, 5, 6, 7, 8 })
                        Assert.That(history[column], Is.EqualTo(payload), $"column {column}");
                    Assert.That(history[4], Is.Empty, "AppliedOn is null");
                }
            });
        }

        [Test]
        public void Csv_Version_1_10_IsWrittenVerbatim()
        {
            var items = new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(
                    ExportTestData.Object("P", "Ns.P", ExportTestData.History(codePath: "Ns.P", version: "1.10")),
                    ExportTestData.Object("M", "Ns.P.M", ExportTestData.History(codePath: "Ns.P.M", version: " 1.10 ")))
            };

            var records = ParseCsv(ExportTestData.ExportText(ExportFormats.Csv, items));
            var histories = records.Where(r => r.Count == 11 && r[10] == "1").ToList();

            Assert.That(histories.Count, Is.EqualTo(2));
            Assert.That(histories.Select(r => r[6]), Is.All.EqualTo("1.10"));
        }

        private static IEnumerable<TestCaseData> MarkdownCellCases()
        {
            yield return Case("script", "<script>alert(1)</script>", "&lt;script&gt;alert(1)&lt;/script&gt;");
            yield return Case("img_onerror", "<img src=x onerror=a()>", "&lt;img src=x onerror=a()&gt;");
            yield return Case("pipe", "a|b", "a\\|b");
            yield return Case("lf", "line1\nline2", "line1 line2");
            yield return Case("cr", "line1\rline2", "line1 line2");
            yield return Case("nel", "a\u0085b", "a b");
            yield return Case("line_separator", "a\u2028b", "a b");
            yield return Case("paragraph_separator", "a\u2029b", "a b");
            yield return Case("backtick", "x`y", "x\\`y");
            yield return Case("emphasis", "*x*", "\\*x\\*");
            yield return Case("link_markup", "[x](javascript:y)", "\\[x\\](javascript:y)");
            yield return Case("backslash", "a\\b", "a\\\\b");
            yield return Case("entity_text", "&amp;", "&amp;amp;");
            yield return Case("ampersand", "a & b", "a &amp; b");
            yield return Case("underscore_not_escaped", "get_X", "get_X");
        }

        [TestCaseSource(nameof(MarkdownCellCases))]
        public void Markdown_TableCell_IsEscapedAndRowShapeIsStable(string payload, string expectedCell)
        {
            var text = ExportTestData.ExportText(ExportFormats.Markdown, MarkdownCellDataset(payload));
            var rows = MarkdownTableRows(text);

            Assert.That(rows.Count, Is.EqualTo(6), "2 tables x (header + separator + 1 data row); a raw newline would split a row");
            Assert.That(rows, Is.All.EndsWith("|"), "every table row must end on its own line");
            Assert.That(rows.Select(r => SplitMarkdownRow(r).Count), Is.All.EqualTo(9), "column count must stay 9");

            Assert.Multiple(() =>
            {
                foreach (var row in new[] { rows[2], rows[5] })
                {
                    var cells = SplitMarkdownRow(row);
                    foreach (var column in new[] { 0, 2, 3, 5, 6, 7, 8 })
                        Assert.That(cells[column], Is.EqualTo(expectedCell), $"column {column}");
                    Assert.That(cells[1], Is.Empty, "empty SourceUrl gives an empty cell");
                    Assert.That(cells[4], Is.Empty, "AppliedOn is null");
                }

                Assert.That(text.Replace("<br />", string.Empty), Does.Not.Contain("<"),
                    "no raw markup from data may reach the Markdown output");
            });
        }

        [Test]
        public void Markdown_TableCell_CrLf_StaysOnOneLine()
        {
            var text = ExportTestData.ExportText(ExportFormats.Markdown, MarkdownCellDataset("line1\r\nline2"));
            var rows = MarkdownTableRows(text);

            Assert.That(rows.Count, Is.EqualTo(6));
            Assert.That(rows, Is.All.EndsWith("|"), "every table row must end on its own line");
            var cells = SplitMarkdownRow(rows[2]);
            Assert.That(cells.Count, Is.EqualTo(9));
            Assert.That(cells[5], Does.Match("^line1 +line2$"));
        }

        private static IEnumerable<TestCaseData> MarkdownCodeSpanCases()
        {
            yield return Case("compiler_generated", "<M>g__L|0_0", "<M>g__L|0_0");
            yield return Case("generic_backtick", "List`1", "List`1");
            yield return Case("leading_backtick", "`lead", "`lead");
            yield return Case("trailing_backtick", "trail`", "trail`");
            yield return Case("double_backtick_run", "a``b", "a``b");
            yield return Case("only_backticks", "``", "``");
            yield return Case("single_backtick", "`", "`");
            yield return Case("markup_literal", "<script>x</script>", "<script>x</script>");
            yield return Case("md_punctuation_literal", "*x* [y](z) \\ _u_ &amp;", "*x* [y](z) \\ _u_ &amp;");
            yield return Case("lf", "a\nb", "a b");
            yield return Case("line_separator", "a\u2028b", "a b");
        }

        [TestCaseSource(nameof(MarkdownCodeSpanCases))]
        public void Markdown_NameAndFullName_CodeSpanKeepsValueVerbatim(string value, string expectedContent)
        {
            var items = new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(
                    ExportTestData.Object(value, value, ExportTestData.History(codePath: "Ns.Cls")),
                    ExportTestData.Object(value, value, ExportTestData.History(codePath: "Ns.Cls.M")))
            };

            var text = ExportTestData.ExportText(ExportFormats.Markdown, items);
            var nameSpans = ReadCodeSpans(text, new Regex("(?<![A-Za-z])Name: "));
            var fullNameSpans = ReadCodeSpans(text, new Regex("FullName: "));

            Assert.That(nameSpans.Count, Is.EqualTo(2), "Name code spans (parent + child)");
            Assert.That(fullNameSpans.Count, Is.EqualTo(2), "FullName code spans (parent + child)");

            Assert.Multiple(() =>
            {
                foreach (var span in nameSpans.Concat(fullNameSpans))
                {
                    Assert.That(span.Content, Is.EqualTo(expectedContent));
                    Assert.That(span.Fence, Is.GreaterThan(LongestBacktickRun(expectedContent)),
                        "fence must be longer than the longest backtick run in the value");
                }
            });
        }

        private static IEnumerable<TestCaseData> MarkdownRejectedUrlCases()
        {
            yield return Case("javascript", "javascript:alert(1)", "javascript:alert(1)");
            yield return Case("javascript_mixed_case", "JaVaScRiPt:x", "JaVaScRiPt:x");
            yield return Case("data", "data:text/html,x", "data:text/html,x");
            yield return Case("protocol_relative", "//evil.tld", "//evil.tld");
            yield return Case("mailto", "mailto:a@b", "mailto:a@b");
            yield return Case("vbscript", "vbscript:msgbox(1)", "vbscript:msgbox(1)");
            yield return Case("file", "file:///c:/x", "file:///c:/x");
            yield return Case("ftp", "ftp://x.tld/a", "ftp://x.tld/a");
            yield return Case("relative", "/relative/path", "/relative/path");
            yield return Case("not_a_uri_with_pipe", "a|b", "a\\|b");
            yield return Case("javascript_with_markup", "javascript:alert('<x>')", "javascript:alert('&lt;x&gt;')");
        }

        [TestCaseSource(nameof(MarkdownRejectedUrlCases))]
        public void Markdown_SourceUrl_NotHttp_RendersAsPlainTextWithoutLink(string url, string expectedCell)
        {
            var cell = MarkdownUrlCell(url);

            Assert.That(cell, Is.EqualTo(expectedCell));
            Assert.That(cell, Does.Not.Contain("]("), "must not be a Markdown link");
        }

        private static IEnumerable<TestCaseData> MarkdownAcceptedUrlCases()
        {
            yield return Case("parens_and_pipe", "https://ok.tld/a_(b)|c", "[https://ok.tld/a_(b)\\|c](https://ok.tld/a_%28b%29%7Cc)");
            yield return Case("plain_http", "http://example.com/path", "[http://example.com/path](http://example.com/path)");
            yield return Case("upper_case_scheme", "HTTPS://ok.tld/x", "[HTTPS://ok.tld/x](https://ok.tld/x)");
            yield return Case("bare_host_normalised", "https://Example.COM", "[https://Example.COM](https://example.com/)");
            yield return Case("space_and_angle_brackets", "https://ok.tld/a b<c>", "[https://ok.tld/a b&lt;c&gt;](https://ok.tld/a%20b%3Cc%3E)");
            yield return Case("padded_is_trimmed", "  https://ok.tld/x  ", "[https://ok.tld/x](https://ok.tld/x)");
        }

        [TestCaseSource(nameof(MarkdownAcceptedUrlCases))]
        public void Markdown_SourceUrl_Http_RendersPercentEncodedLink(string url, string expectedCell)
        {
            var cell = MarkdownUrlCell(url);

            Assert.That(cell, Is.EqualTo(expectedCell));
            var destination = Regex.Match(cell, @"\]\((?<d>.*)\)$").Groups["d"].Value;
            Assert.That(destination, Does.Not.Match(@"[()|<>\s]"), "destination must not contain raw ( ) | < > or whitespace");
        }

        [Test]
        public void Markdown_SourceUrl_Empty_RendersEmptyCell()
        {
            Assert.That(MarkdownUrlCell(null), Is.Empty);
            Assert.That(MarkdownUrlCell("   "), Is.Empty);
        }

        private static IEnumerable<TestCaseData> YamlPayloadCases()
        {
            var implicitTyped = new[]
            {
                "yes", "no", "on", "off", "y", "n", "true", "false", "null", "Null", "~", ".inf", "-.inf", ".nan",
                "0x1F", "0o17", "012", "1e3", "1_000", "2024-01-01", "12:30:45"
            };
            for (var i = 0; i < implicitTyped.Length; i++)
                yield return Case($"implicit_{i}_{Regex.Replace(implicitTyped[i], "[^A-Za-z0-9]", "_")}",
                    implicitTyped[i], implicitTyped[i]);

            yield return Case("quote_and_backslash", "a\"b\\c", "a\"b\\c");
            yield return Case("newline_key_injection", "x\ny: z", "x\ny: z");
            yield return Case("cr", "a\rb", "a\rb");
            yield return Case("crlf", "a\r\nb", "a\r\nb");
            yield return Case("tab", "a\tb", "a\tb");
            yield return Case("nul", "a\u0000b", "a\u0000b");
            yield return Case("bell", "a\u0007b", "a\u0007b");
            yield return Case("esc", "a\u001Bb", "a\u001Bb");
            yield return Case("del", "a\u007Fb", "a\u007Fb");
            yield return Case("c1", "a\u0090b", "a\u0090b");
            yield return Case("nel", "a\u0085b", "a\u0085b");
            yield return Case("line_separator", "a\u2028b", "a\u2028b");
            yield return Case("paragraph_separator", "a\u2029b", "a\u2029b");
            yield return Case("bom", "a\uFEFFb", "a\uFEFFb");
            yield return Case("noncharacter_fffe", "a\uFFFEb", "a\uFFFEb");
            yield return Case("noncharacter_ffff", "a\uFFFFb", "a\uFFFFb");
            yield return Case("lone_high_surrogate", "a\uD800b", "a\uFFFDb");
            yield return Case("lone_low_surrogate", "a\uDC00b", "a\uFFFDb");
            yield return Case("surrogate_pair", "a\uD83D\uDE00b", "a\uD83D\uDE00b");
            yield return Case("mapping_comment", "a: b #c", "a: b #c");
            yield return Case("sequence_entry", "- x", "- x");
            yield return Case("complex_key", "? x", "? x");
            yield return Case("flow_sequence", "[a, b]", "[a, b]");
            yield return Case("flow_mapping", "{a: b}", "{a: b}");
            yield return Case("single_quotes", "'q'", "'q'");
            yield return Case("anchor", "&a x", "&a x");
            yield return Case("alias", "*a", "*a");
            yield return Case("tag", "!!int 1", "!!int 1");
            yield return Case("directive", "%YAML 1.2", "%YAML 1.2");
            yield return Case("document_start", "---", "---");
            yield return Case("document_end", "...", "...");
            yield return Case("literal_block", "|", "|");
            yield return Case("folded_block", ">", ">");
            yield return Case("comment", "#c", "#c");
            yield return Case("reserved_at", "@x", "@x");
            yield return Case("reserved_backtick", "`x", "`x");
            yield return Case("generic_type", "System.Collections.Generic.List`1[[System.String, mscorlib]]",
                "System.Collections.Generic.List`1[[System.String, mscorlib]]");
            yield return Case("nested_generic", "Outer+Inner<T>.M(Dictionary<string, List<int>>)",
                "Outer+Inner<T>.M(Dictionary<string, List<int>>)");
        }

        [TestCaseSource(nameof(YamlPayloadCases))]
        public void Yaml_Payload_RoundTripsAsDoubleQuotedScalarWithoutExtraKeys(string payload, string expected)
        {
            var text = ExportTestData.ExportText(ExportFormats.Yaml, ExportTestData.Uniform(payload));
            var entries = LoadYamlEntries(text);

            Assert.That(entries.Count, Is.EqualTo(2), "parent + child entries under codeSources");

            Assert.Multiple(() =>
            {
                foreach (var entry in entries)
                {
                    Assert.That(YamlKeys(entry), Is.EquivalentTo(new[] { "fullName", "name", "history" }));
                    AssertQuotedScalar(entry, "fullName", expected);
                    AssertQuotedScalar(entry, "name", expected);

                    var history = YamlGet(entry, "history") as YamlSequenceNode;
                    Assert.That(history, Is.Not.Null, "history must be a sequence");
                    if (history == null)
                        continue;

                    Assert.That(history.Children.Count, Is.EqualTo(1));
                    var change = (YamlMappingNode)history.Children[0];
                    Assert.That(YamlKeys(change), Is.EquivalentTo(HistoryKeys));
                    foreach (var key in HistoryKeys.Where(k => k != "appliedOn"))
                        AssertQuotedScalar(change, key, expected);
                    AssertQuotedScalar(change, "appliedOn", string.Empty);
                }
            });
        }

        [Test]
        public void Yaml_NullValues_AreEmittedAsEmptyQuotedStrings()
        {
            var items = new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(
                    ExportTestData.Object(null, null, ExportTestData.History()),
                    ExportTestData.Object(null, null, ExportTestData.History()))
            };

            var entries = LoadYamlEntries(ExportTestData.ExportText(ExportFormats.Yaml, items));

            Assert.That(entries.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                foreach (var entry in entries)
                {
                    AssertQuotedScalar(entry, "fullName", string.Empty);
                    AssertQuotedScalar(entry, "name", string.Empty);
                    var change = (YamlMappingNode)((YamlSequenceNode)YamlGet(entry, "history")).Children[0];
                    foreach (var key in HistoryKeys)
                        AssertQuotedScalar(change, key, string.Empty);
                }
            });
        }

        private static IEnumerable<TestCaseData> XmlPayloadCases()
        {
            yield return Case("close_tag_injection", "</codePath><x>", "</codePath><x>");
            yield return Case("entity_text", "&amp;", "&amp;");
            yield return Case("quotes", "a'\"", "a'\"");
            yield return Case("cdata_end", "]]>", "]]>");
            yield return Case("cdata", "<![CDATA[x]]>", "<![CDATA[x]]>");
            yield return Case("doctype_text", "<!DOCTYPE x [<!ENTITY e \"boom\">]>&e;", "<!DOCTYPE x [<!ENTITY e \"boom\">]>&e;");
            yield return Case("cr", "a\rb", "a\rb");
            yield return Case("crlf", "a\r\nb", "a\r\nb");
            yield return Case("lf", "a\nb", "a\nb");
            yield return Case("tab", "a\tb", "a\tb");
            yield return Case("nel_allowed", "a\u0085b", "a\u0085b");
            yield return Case("del_allowed", "a\u007Fb", "a\u007Fb");
            yield return Case("line_separator_allowed", "a\u2028b", "a\u2028b");
            yield return Case("surrogate_pair_allowed", "a\uD83D\uDE00b", "a\uD83D\uDE00b");
            yield return Case("soh", "a\u0001b", "a\uFFFDb");
            yield return Case("nul", "a\u0000b", "a\uFFFDb");
            yield return Case("vt", "a\u000Bb", "a\uFFFDb");
            yield return Case("ff", "a\u000Cb", "a\uFFFDb");
            yield return Case("us", "a\u001Fb", "a\uFFFDb");
            yield return Case("fffe", "a\uFFFEb", "a\uFFFDb");
            yield return Case("ffff", "a\uFFFFb", "a\uFFFDb");
            yield return Case("lone_high_surrogate", "a\uD800b", "a\uFFFDb");
            yield return Case("lone_low_surrogate", "a\uDC00b", "a\uFFFDb");
            yield return Case("reversed_pair", "a\uDC00\uD800b", "a\uFFFD\uFFFDb");
            yield return Case("trailing_high_surrogate", "ab\uD800", "ab\uFFFD");
        }

        [TestCaseSource(nameof(XmlPayloadCases))]
        public void Xml_Payload_LoadsStrictlyAndRoundTrips(string payload, string expected)
        {
            var bytes = ExportTestData.Export(ExportFormats.Xml, ExportTestData.Uniform(payload));

            Assert.That(ExportTestData.Decode(bytes), Does.Not.Contain("<!DOCTYPE"), "no DTD may be emitted");
            var document = LoadStrictXml(bytes);

            var elementNames = document.SelectNodes("//*").Cast<XmlElement>().Select(x => x.LocalName).Distinct();
            Assert.That(elementNames, Is.SubsetOf(XmlElementNames), "data must not inject elements");

            var objects = document
                .SelectNodes("/codeSources/codeSource/parent | /codeSources/codeSource/children/child")
                .Cast<XmlElement>().ToList();
            Assert.That(objects.Count, Is.EqualTo(2));

            Assert.Multiple(() =>
            {
                foreach (var obj in objects)
                {
                    Assert.That(obj["name"]?.InnerText, Is.EqualTo(expected), "name");
                    Assert.That(obj["fullName"]?.InnerText, Is.EqualTo(expected), "fullName");

                    var changes = obj.SelectNodes("codeChanges/codeChange").Cast<XmlElement>().ToList();
                    Assert.That(changes.Count, Is.EqualTo(1));
                    foreach (var change in changes)
                    {
                        foreach (var key in HistoryKeys.Where(k => k != "appliedOn"))
                            Assert.That(change[key]?.InnerText, Is.EqualTo(expected), key);
                        Assert.That(change["appliedOn"]?.InnerText, Is.Empty, "appliedOn");
                    }
                }
            });
        }

        [Test]
        public void XmlBuilder_BuildAttribute_EscapesMarkupQuotesAndWhitespace()
        {
            var builderType = typeof(XmlExporter).Assembly
                .GetType("RzR.Core.CodeSource.Extensions.Internal.Builder.XmlBuilder", false);
            var buildAttribute = builderType?
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "BuildAttribute"
                                     && m.ReturnType == typeof(string)
                                     && m.GetParameters().Length == 1
                                     && m.GetParameters()[0].ParameterType
                                         .IsAssignableFrom(typeof(Dictionary<string, string>)));
            if (buildAttribute == null)
                Assert.Fail("XmlBuilder.BuildAttribute(Dictionary<string,string>) not found by reflection: update this test.");

            const string value = "\"\t\n\r<&'>";
            var row = (string)buildAttribute.Invoke(null,
                new object[] { new Dictionary<string, string> { ["a"] = value } });

            Assert.That(row, Does.Contain("&quot;&#x9;&#xA;"), "payload row: \" + TAB + LF");

            var document = LoadStrictXml(new UTF8Encoding(false).GetBytes("<e" + row + "/>"));
            Assert.That(document.DocumentElement.GetAttribute("a"), Is.EqualTo(value));
        }

        [Test]
        public void Html_BreakoutPayload_IsFullyEncoded()
        {
            const string payload = "\"><svg onload=a()>";
            var text = ExportTestData.ExportText(ExportFormats.Html, ExportTestData.Uniform(payload));

            Assert.That(text, Does.Not.Contain("<svg"));
            Assert.That(text, Does.Contain("&quot;&gt;&lt;svg onload=a()&gt;"));
        }

        private static IEnumerable<TestCaseData> JsonPayloadCases()
        {
            yield return Case("quote_and_backslash", "a\"b\\c");
            yield return Case("newline", "x\ny");
            yield return Case("soh", "a\u0001b");
            yield return Case("script_close", "</script>");
            yield return Case("line_separator", "a\u2028b");
            yield return Case("tab", "a\tb");
            yield return Case("surrogate_pair", "a\uD83D\uDE00b");
        }

        [TestCaseSource(nameof(JsonPayloadCases))]
        public void Json_Payload_RoundTrips(string payload)
        {
            var text = ExportTestData.ExportText(ExportFormats.Json, ExportTestData.Uniform(payload));

            using (var document = JsonDocument.Parse(text))
            {
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
                        foreach (var key in HistoryKeys.Where(k => k != "appliedOn"))
                            Assert.That(change.GetProperty(key).GetString(), Is.EqualTo(payload), key);
                    }
                });
            }
        }

        [TestCaseSource(nameof(Formats))]
        public void AppliedOn_UnderThaiCulture_StaysGregorian(string format)
        {
            var thai = new CultureInfo("th-TH");
            Assume.That(new DateTime(2022, 12, 12).ToString("yyyy-MM-dd", thai), Is.Not.EqualTo("2022-12-12"),
                "precondition: th-TH must use a non-Gregorian calendar on this machine");

            var items = new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(
                    ExportTestData.Object("P", "Ns.P",
                        ExportTestData.History(codePath: "Ns.P", appliedOn: new DateTime(2022, 12, 12))),
                    ExportTestData.Object("M", "Ns.P.M",
                        ExportTestData.History(codePath: "Ns.P.M", appliedOn: new DateTime(2023, 1, 31))))
            };

            var invariant = ExportUnderCulture(format, items, CultureInfo.InvariantCulture);
            var thaiBytes = ExportUnderCulture(format, items, thai);
            var text = ExportTestData.Decode(thaiBytes);

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
                return ExportTestData.Export(format, items);
            }
            finally
            {
                thread.CurrentCulture = originalCulture;
                thread.CurrentUICulture = originalUiCulture;
            }
        }

        private static TestCaseData Case(string label, params object[] args)
            => new TestCaseData(args).SetName("{m}(" + label + ")");

        private static string Row(params string[] cells) => string.Join(",", cells);

        private static List<List<string>> ParseCsv(string text)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;
            var wasQuoted = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuotes)
                {
                    if (c != '"')
                    {
                        field.Append(c);
                        continue;
                    }

                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = false;
                    if (i + 1 < text.Length && text[i + 1] != ',' && text[i + 1] != '\r' && text[i + 1] != '\n')
                        throw new FormatException($"RFC 4180 violation: '{text[i + 1]}' after a closing quote at {i}.");
                    continue;
                }

                switch (c)
                {
                    case '"':
                        if (field.Length > 0 || wasQuoted)
                            throw new FormatException($"RFC 4180 violation: quote inside an unquoted field at {i}.");
                        inQuotes = true;
                        wasQuoted = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        wasQuoted = false;
                        break;
                    case '\r':
                    case '\n':
                        if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                            i++;
                        record.Add(field.ToString());
                        records.Add(record);
                        record = new List<string>();
                        field.Clear();
                        wasQuoted = false;
                        break;
                    default:
                        field.Append(c);
                        break;
                }
            }

            if (inQuotes)
                throw new FormatException("RFC 4180 violation: unterminated quoted field.");

            if (field.Length > 0 || wasQuoted || record.Count > 0)
            {
                record.Add(field.ToString());
                records.Add(record);
            }

            return records;
        }

        private static List<CodeSourceObjectsResult> MarkdownCellDataset(string payload)
        {
            CodeSourceObjectHistory NewHistory() => ExportTestData.History(codePath: payload, sourceUrl: null,
                authorName: payload, copyright: payload, appliedOn: null, comment: payload, version: payload,
                tags: payload, relatedTaskId: payload);

            return new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(
                    ExportTestData.Object("Cls", "Ns.Cls", NewHistory()),
                    ExportTestData.Object("M", "Ns.Cls.M", NewHistory()))
            };
        }

        private static string MarkdownUrlCell(string url)
        {
            var items = new List<CodeSourceObjectsResult>
            {
                ExportTestData.Result(ExportTestData.Object("Cls", "Ns.Cls",
                    ExportTestData.History(codePath: "Ns.Cls", sourceUrl: url, authorName: "a")))
            };

            var rows = MarkdownTableRows(ExportTestData.ExportText(ExportFormats.Markdown, items));
            Assert.That(rows.Count, Is.EqualTo(3), "header + separator + 1 data row");

            var cells = SplitMarkdownRow(rows[2]);
            Assert.That(cells.Count, Is.EqualTo(9), "column count must stay 9");

            return cells[1];
        }

        private static List<string> MarkdownTableRows(string text)
            => text.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.StartsWith("|")).ToList();

        private static List<string> SplitMarkdownRow(string row)
        {
            var cells = new List<string>();
            var cell = new StringBuilder();
            for (var i = 0; i < row.Length; i++)
            {
                var c = row[i];
                if (c == '\\' && i + 1 < row.Length)
                {
                    cell.Append(c).Append(row[i + 1]);
                    i++;
                    continue;
                }

                if (c == '|')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                    continue;
                }

                cell.Append(c);
            }

            cells.Add(cell.ToString());

            return cells.Skip(1).Take(cells.Count - 2).Select(x => x.Trim(' ')).ToList();
        }

        private static List<(int Fence, string Content)> ReadCodeSpans(string text, Regex label)
        {
            var spans = new List<(int, string)>();
            foreach (Match match in label.Matches(text))
            {
                var i = match.Index + match.Length;
                var fence = 0;
                while (i < text.Length && text[i] == '`')
                {
                    fence++;
                    i++;
                }

                if (fence == 0)
                    Assert.Fail($"No code span after '{label}' at {match.Index}.");

                var contentStart = i;
                string content = null;
                while (i < text.Length && content == null)
                {
                    if (text[i] != '`')
                    {
                        i++;
                        continue;
                    }

                    var runStart = i;
                    while (i < text.Length && text[i] == '`')
                        i++;
                    if (i - runStart == fence)
                        content = text.Substring(contentStart, runStart - contentStart);
                }

                if (content == null)
                    Assert.Fail($"Unterminated code span (fence {fence}) after '{label}' at {match.Index}.");

                if (content.Length >= 2 && content[0] == ' ' && content[content.Length - 1] == ' '
                    && content.Trim(' ').Length > 0)
                    content = content.Substring(1, content.Length - 2);

                spans.Add((fence, content));
            }

            return spans;
        }

        private static int LongestBacktickRun(string value)
        {
            int longest = 0, current = 0;
            foreach (var c in value)
            {
                current = c == '`' ? current + 1 : 0;
                longest = Math.Max(longest, current);
            }

            return longest;
        }

        private static List<YamlMappingNode> LoadYamlEntries(string text)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(text));

            Assert.That(stream.Documents.Count, Is.EqualTo(1), "exactly one YAML document");
            var root = (YamlMappingNode)stream.Documents[0].RootNode;
            Assert.That(YamlKeys(root), Is.EquivalentTo(new[] { "codeSources" }), "root keys");

            return ((YamlSequenceNode)YamlGet(root, "codeSources")).Children.Cast<YamlMappingNode>().ToList();
        }

        private static IEnumerable<string> YamlKeys(YamlMappingNode mapping)
            => mapping.Children.Keys.Select(k => ((YamlScalarNode)k).Value);

        private static YamlNode YamlGet(YamlMappingNode mapping, string key)
            => mapping.Children.FirstOrDefault(kv => ((YamlScalarNode)kv.Key).Value == key).Value;

        private static void AssertQuotedScalar(YamlMappingNode mapping, string key, string expected)
        {
            var scalar = YamlGet(mapping, key) as YamlScalarNode;
            Assert.That(scalar, Is.Not.Null, $"'{key}' must be a scalar");
            if (scalar == null)
                return;

            Assert.That(scalar.Style, Is.EqualTo(ScalarStyle.DoubleQuoted), $"'{key}' must be double-quoted");
            Assert.That(scalar.Value, Is.EqualTo(expected), $"'{key}' value");
        }

        private static XmlDocument LoadStrictXml(byte[] bytes)
        {
            var settings = new XmlReaderSettings
            {
                CheckCharacters = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using (var stream = new MemoryStream(bytes))
            using (var reader = XmlReader.Create(stream, settings))
            {
                var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                document.Load(reader);

                return document;
            }
        }
    }
}
