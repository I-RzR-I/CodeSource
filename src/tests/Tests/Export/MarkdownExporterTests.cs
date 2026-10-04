using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class MarkdownExporterTests
    {
        [TestCaseSource(nameof(MarkdownCellCases))]
        public void Markdown_TableCell_IsEscapedAndRowShapeIsStable(string payload, string expectedCell)
        {
            var text = ExportRunner.ExportText(ExportFormats.Markdown, MarkdownCellDataset(payload));
            var rows = MarkdownTable.Rows(text);

            Assert.That(rows, Has.Count.EqualTo(2 * MarkdownTable.RowsPerTable),
                "2 tables x (header + separator + 1 data row); a raw newline would split a row");
            Assert.That(rows, Is.All.EndsWith("|"), "every table row must end on its own line");
            Assert.That(rows.Select(row => MarkdownTable.SplitRow(row).Count), Is.All.EqualTo(MarkdownColumn.Count),
                "column count must stay 9");

            Assert.Multiple(() =>
            {
                foreach (var row in MarkdownTable.DataRows(rows))
                {
                    var cells = MarkdownTable.SplitRow(row);
                    foreach (var column in MarkdownColumn.Text)
                        Assert.That(cells[column], Is.EqualTo(expectedCell), $"column {column}");
                    Assert.That(cells[MarkdownColumn.SourceUrl], Is.Empty, "empty SourceUrl gives an empty cell");
                    Assert.That(cells[MarkdownColumn.AppliedOn], Is.Empty, "AppliedOn is null");
                }

                Assert.That(text.Replace("<br />", string.Empty), Does.Not.Contain("<"),
                    "no raw markup from data may reach the Markdown output");
            });
        }

        [Test]
        public void Markdown_TableCellWithCrLf_StaysOnOneLine()
        {
            var text = ExportRunner.ExportText(ExportFormats.Markdown, MarkdownCellDataset("line1\r\nline2"));
            var rows = MarkdownTable.Rows(text);

            Assert.That(rows, Has.Count.EqualTo(2 * MarkdownTable.RowsPerTable));
            Assert.That(rows, Is.All.EndsWith("|"), "every table row must end on its own line");
            var cells = MarkdownTable.SplitRow(MarkdownTable.DataRows(rows)[0]);
            Assert.That(cells, Has.Count.EqualTo(MarkdownColumn.Count));
            Assert.That(cells[MarkdownColumn.Comment], Does.Match("^line1 +line2$"));
        }

        [TestCaseSource(nameof(MarkdownCodeSpanCases))]
        public void Markdown_NameAndFullName_CodeSpanKeepsValueVerbatim(string value, string expectedContent)
        {
            var items = new List<CodeSourceObjectsResult>
            {
                CodeSourceResults.Result(
                    CodeSourceResults.NewObject(value, value, CodeSourceResults.History(codePath: "Ns.Cls")),
                    CodeSourceResults.NewObject(value, value, CodeSourceResults.History(codePath: "Ns.Cls.M")))
            };

            var text = ExportRunner.ExportText(ExportFormats.Markdown, items);
            var nameSpans = MarkdownTable.ReadCodeSpans(text, new Regex("(?<![A-Za-z])Name: "));
            var fullNameSpans = MarkdownTable.ReadCodeSpans(text, new Regex("FullName: "));

            Assert.That(nameSpans, Has.Count.EqualTo(2), "Name code spans (parent + child)");
            Assert.That(fullNameSpans, Has.Count.EqualTo(2), "FullName code spans (parent + child)");

            Assert.Multiple(() =>
            {
                foreach (var span in nameSpans.Concat(fullNameSpans))
                {
                    Assert.That(span.Content, Is.EqualTo(expectedContent));
                    Assert.That(span.Fence, Is.GreaterThan(MarkdownTable.LongestBacktickRun(expectedContent)),
                        "fence must be longer than the longest backtick run in the value");
                }
            });
        }

        [TestCaseSource(nameof(MarkdownRejectedUrlCases))]
        public void Markdown_NonHttpSourceUrl_RendersAsPlainTextWithoutLink(string url, string expectedCell)
        {
            var cell = MarkdownUrlCell(url);

            Assert.That(cell, Is.EqualTo(expectedCell));
            Assert.That(cell, Does.Not.Contain("]("), "must not be a Markdown link");
        }

        [TestCaseSource(nameof(MarkdownAcceptedUrlCases))]
        public void Markdown_HttpSourceUrl_RendersPercentEncodedLink(string url, string expectedCell)
        {
            var cell = MarkdownUrlCell(url);

            Assert.That(cell, Is.EqualTo(expectedCell));
            var destination = Regex.Match(cell, @"\]\((?<d>.*)\)$").Groups["d"].Value;
            Assert.That(destination, Does.Not.Match(@"[()|<>\s]"), "destination must not contain raw ( ) | < > or whitespace");
        }

        [TestCase(null, TestName = "{m}(null)")]
        [TestCase("   ", TestName = "{m}(spaces)")]
        public void Markdown_NullOrBlankSourceUrl_RendersEmptyCell(string url)
        {
            Assert.That(MarkdownUrlCell(url), Is.Empty);
        }

        [Test]
        public void Markdown_EmptyResults_WritesNothing()
        {
            var text = ExportRunner.ExportText(ExportFormats.Markdown, Array.Empty<CodeSourceObjectsResult>());

            Assert.That(text.Trim(), Is.Empty);
        }

        private static List<CodeSourceObjectsResult> MarkdownCellDataset(string payload)
        {
            CodeSourceObjectHistory NewHistory() => CodeSourceResults.History(codePath: payload, sourceUrl: null,
                authorName: payload, copyright: payload, appliedOn: null, comment: payload, version: payload,
                tags: payload, relatedTaskId: payload);

            return new List<CodeSourceObjectsResult>
            {
                CodeSourceResults.Result(
                    CodeSourceResults.NewObject("Cls", "Ns.Cls", NewHistory()),
                    CodeSourceResults.NewObject("M", "Ns.Cls.M", NewHistory()))
            };
        }

        private static string MarkdownUrlCell(string url)
        {
            var items = new List<CodeSourceObjectsResult>
            {
                CodeSourceResults.Result(CodeSourceResults.NewObject("Cls", "Ns.Cls",
                    CodeSourceResults.History(codePath: "Ns.Cls", sourceUrl: url, authorName: "a")))
            };

            var rows = MarkdownTable.Rows(ExportRunner.ExportText(ExportFormats.Markdown, items));
            Assert.That(rows, Has.Count.EqualTo(MarkdownTable.RowsPerTable), "header + separator + 1 data row");

            var cells = MarkdownTable.SplitRow(MarkdownTable.DataRows(rows)[0]);
            Assert.That(cells, Has.Count.EqualTo(MarkdownColumn.Count), "column count must stay 9");

            return cells[MarkdownColumn.SourceUrl];
        }

        private static IEnumerable<TestCaseData> MarkdownCellCases()
        {
            yield return NamedCase.Of("script", "<script>alert(1)</script>", "&lt;script&gt;alert(1)&lt;/script&gt;");
            yield return NamedCase.Of("img_onerror", "<img src=x onerror=a()>", "&lt;img src=x onerror=a()&gt;");
            yield return NamedCase.Of("pipe", "a|b", "a\\|b");
            yield return NamedCase.Of("lf", "line1\nline2", "line1 line2");
            yield return NamedCase.Of("cr", "line1\rline2", "line1 line2");
            yield return NamedCase.Of("nel", "a\u0085b", "a b");
            yield return NamedCase.Of("line_separator", "a\u2028b", "a b");
            yield return NamedCase.Of("paragraph_separator", "a\u2029b", "a b");
            yield return NamedCase.Of("backtick", "x`y", "x\\`y");
            yield return NamedCase.Of("emphasis", "*x*", "\\*x\\*");
            yield return NamedCase.Of("link_markup", "[x](javascript:y)", "\\[x\\](javascript:y)");
            yield return NamedCase.Of("backslash", "a\\b", "a\\\\b");
            yield return NamedCase.Of("entity_text", "&amp;", "&amp;amp;");
            yield return NamedCase.Of("ampersand", "a & b", "a &amp; b");
            yield return NamedCase.Of("underscore_not_escaped", "get_X", "get_X");
        }

        private static IEnumerable<TestCaseData> MarkdownCodeSpanCases()
        {
            yield return NamedCase.Of("compiler_generated", "<M>g__L|0_0", "<M>g__L|0_0");
            yield return NamedCase.Of("generic_backtick", "List`1", "List`1");
            yield return NamedCase.Of("leading_backtick", "`lead", "`lead");
            yield return NamedCase.Of("trailing_backtick", "trail`", "trail`");
            yield return NamedCase.Of("double_backtick_run", "a``b", "a``b");
            yield return NamedCase.Of("only_backticks", "``", "``");
            yield return NamedCase.Of("single_backtick", "`", "`");
            yield return NamedCase.Of("markup_literal", "<script>x</script>", "<script>x</script>");
            yield return NamedCase.Of("md_punctuation_literal", "*x* [y](z) \\ _u_ &amp;", "*x* [y](z) \\ _u_ &amp;");
            yield return NamedCase.Of("lf", "a\nb", "a b");
            yield return NamedCase.Of("line_separator", "a\u2028b", "a b");
        }

        private static IEnumerable<TestCaseData> MarkdownRejectedUrlCases()
        {
            yield return NamedCase.Of("javascript", "javascript:alert(1)", "javascript:alert(1)");
            yield return NamedCase.Of("javascript_mixed_case", "JaVaScRiPt:x", "JaVaScRiPt:x");
            yield return NamedCase.Of("data", "data:text/html,x", "data:text/html,x");
            yield return NamedCase.Of("protocol_relative", "//evil.tld", "//evil.tld");
            yield return NamedCase.Of("mailto", "mailto:a@b", "mailto:a@b");
            yield return NamedCase.Of("vbscript", "vbscript:msgbox(1)", "vbscript:msgbox(1)");
            yield return NamedCase.Of("file", "file:///c:/x", "file:///c:/x");
            yield return NamedCase.Of("ftp", "ftp://x.tld/a", "ftp://x.tld/a");
            yield return NamedCase.Of("relative", "/relative/path", "/relative/path");
            yield return NamedCase.Of("not_a_uri_with_pipe", "a|b", "a\\|b");
            yield return NamedCase.Of("javascript_with_markup", "javascript:alert('<x>')", "javascript:alert('&lt;x&gt;')");
        }

        private static IEnumerable<TestCaseData> MarkdownAcceptedUrlCases()
        {
            yield return NamedCase.Of("parens_and_pipe", "https://ok.tld/a_(b)|c", "[https://ok.tld/a_(b)\\|c](https://ok.tld/a_%28b%29%7Cc)");
            yield return NamedCase.Of("plain_http", "http://example.com/path", "[http://example.com/path](http://example.com/path)");
            yield return NamedCase.Of("upper_case_scheme", "HTTPS://ok.tld/x", "[HTTPS://ok.tld/x](https://ok.tld/x)");
            yield return NamedCase.Of("bare_host_normalised", "https://Example.COM", "[https://Example.COM](https://example.com/)");
            yield return NamedCase.Of("space_and_angle_brackets", "https://ok.tld/a b<c>", "[https://ok.tld/a b&lt;c&gt;](https://ok.tld/a%20b%3Cc%3E)");
            yield return NamedCase.Of("padded_is_trimmed", "  https://ok.tld/x  ", "[https://ok.tld/x](https://ok.tld/x)");
        }

        private static class MarkdownColumn
        {
            internal const int SourceUrl = 1;
            internal const int AppliedOn = 4;
            internal const int Comment = 5;
            internal const int Count = 9;

            internal static readonly int[] Text = Enumerable.Range(0, Count).Except(new[] { SourceUrl, AppliedOn }).ToArray();
        }

        private static class MarkdownTable
        {
            internal const int RowsPerTable = 3;

            internal static List<string> Rows(string text)
                => text.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.StartsWith("|")).ToList();

            internal static List<string> DataRows(List<string> rows)
                => rows.Where((row, index) => index % RowsPerTable == RowsPerTable - 1).ToList();

            internal static List<string> SplitRow(string row)
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

            internal static List<(int Fence, string Content)> ReadCodeSpans(string text, Regex label)
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

            internal static int LongestBacktickRun(string value)
            {
                int longest = 0, current = 0;
                foreach (var c in value)
                {
                    current = c == '`' ? current + 1 : 0;
                    longest = Math.Max(longest, current);
                }

                return longest;
            }
        }
    }
}
