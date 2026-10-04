using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using Tests.Support;

namespace Tests.Export
{
    [TestFixture]
    public class CsvExporterTests
    {
        private const string EmptyCell = "\"\"";

        private const string Header = "CodePath,URL,Author,Copyright,AppliedOn,Comment,Version,Tags,WorkItemId,IsParent,IsHistory";

        [TestCaseSource(nameof(CsvFormulaCases))]
        public void Csv_FormulaPayload_IsNeutralisedInEveryDataCell(string payload, string expectedCell)
        {
            AssertEveryCsvDataCell(payload, expectedCell);
        }

        [TestCaseSource(nameof(CsvCellStartRuleCases))]
        public void Csv_CellStartRule_NeutralisesEveryCellStart(string payload, string expectedCell)
        {
            var records = AssertEveryCsvDataCell(payload, expectedCell);

            var parentHistory = records.Single(Rfc4180Parser.IsParentHistoryRow);
            Assert.That(parentHistory[CsvColumn.Comment], Is.EqualTo(Rfc4180Parser.Unquote(expectedCell)),
                "RFC 4180 parse of the Comment cell");
        }

        [Test]
        public void Csv_CommaQuoteCrLf_RoundTripsThroughRfc4180Parser()
        {
            const string payload = "a,\"b\"\r\nc";

            var records = Rfc4180Parser.Parse(ExportRunner.ExportText(ExportFormats.Csv, CodeSourceResults.Uniform(payload)));
            var baseline = Rfc4180Parser.Parse(ExportRunner.ExportText(ExportFormats.Csv, CodeSourceResults.Uniform("plain")));

            Assert.That(records, Has.Count.EqualTo(baseline.Count), "embedded CRLF must not add records");

            var parent = records.Single(Rfc4180Parser.IsParentRow);
            var child = records.Single(Rfc4180Parser.IsChildRow);
            var histories = records.Where(Rfc4180Parser.IsHistoryRow).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(parent[CsvColumn.CodePath], Is.EqualTo(payload));
                Assert.That(child[CsvColumn.CodePath], Is.EqualTo(payload));
                Assert.That(histories, Has.Count.EqualTo(2));
                foreach (var history in histories)
                {
                    foreach (var column in CsvColumn.HistoryText)
                        Assert.That(history[column], Is.EqualTo(payload), $"column {column}");
                    Assert.That(history[CsvColumn.AppliedOn], Is.Empty, "AppliedOn is null");
                }
            });
        }

        [Test]
        public void Csv_VersionOneTen_IsWrittenVerbatim()
        {
            var items = new List<CodeSourceObjectsResult>
            {
                CodeSourceResults.Result(
                    CodeSourceResults.NewObject("P", "Ns.P", CodeSourceResults.History(codePath: "Ns.P", version: "1.10")),
                    CodeSourceResults.NewObject("M", "Ns.P.M", CodeSourceResults.History(codePath: "Ns.P.M", version: " 1.10 ")))
            };

            var records = Rfc4180Parser.Parse(ExportRunner.ExportText(ExportFormats.Csv, items));
            var histories = records.Where(Rfc4180Parser.IsHistoryRow).ToList();

            Assert.That(histories, Has.Count.EqualTo(2));
            Assert.That(histories.Select(record => record[CsvColumn.Version]), Is.All.EqualTo("1.10"));
        }

        [Test]
        public void Csv_EmptyResults_WritesOnlyTheHeader()
        {
            var text = ExportRunner.ExportText(ExportFormats.Csv, Array.Empty<CodeSourceObjectsResult>());

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Trim().Length > 0)
                .ToList();

            Assert.That(lines, Is.EqualTo(new[] { Header }));
        }

        private static List<List<string>> AssertEveryCsvDataCell(string payload, string expectedCell)
        {
            var text = ExportRunner.ExportText(ExportFormats.Csv, CodeSourceResults.Uniform(payload));
            var newLine = Environment.NewLine;

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain(newLine + DataRow(expectedCell, true, false) + newLine), "parent row (FullName cell)");
                Assert.That(text, Does.Contain(newLine + DataRow(expectedCell, true, true) + newLine), "parent history row (every data cell)");
                Assert.That(text, Does.Contain(newLine + DataRow(expectedCell, false, false) + newLine), "child row (FullName cell)");
                Assert.That(text, Does.Contain(newLine + DataRow(expectedCell, false, true) + newLine), "child history row (every data cell)");
            });

            var records = Rfc4180Parser.Parse(text);
            Assert.That(records, Has.Count.EqualTo(7), "header + (spacer, parent, history) x 2");

            return records;
        }

        private static string DataRow(string cell, bool isParent, bool isHistory)
        {
            var historyCell = isHistory ? cell : EmptyCell;

            return Row(cell, historyCell, historyCell, historyCell, EmptyCell, historyCell, historyCell, historyCell,
                historyCell, Flag(isParent), Flag(isHistory));
        }

        private static string Flag(bool value) => value ? "\"1\"" : "\"0\"";

        private static string Row(params string[] cells) => string.Join(",", cells);

        private static IEnumerable<TestCaseData> CsvFormulaCases()
        {
            yield return NamedCase.Of("hyperlink", "=HYPERLINK(\"http://x\",\"c\")", "\"'=HYPERLINK(\"\"http://x\"\",\"\"c\"\")\"");
            yield return NamedCase.Of("tab_then_equals", "\t=1", "\"'=1\"");
            yield return NamedCase.Of("tab_then_text", "\tX", "\"X\"");
            yield return NamedCase.Of("cr_then_text", "\rX", "\"X\"");
            yield return NamedCase.Of("lf_then_equals", "\n=1", "\"'=1\"");
            yield return NamedCase.Of("space_then_equals", " =1", "\"'=1\"");
            yield return NamedCase.Of("nel_then_equals", "\u0085=1", "\"'=1\"");
            yield return NamedCase.Of("line_separator_then_equals", "\u2028=1", "\"'=1\"");
            yield return NamedCase.Of("nbsp_then_equals", "\u00A0=1", "\"'=1\"");
            yield return NamedCase.Of("already_apostrophe_no_double_prefix", "'=1", "\"'=1\"");
            yield return NamedCase.Of("fullwidth_equals", "\uFF1D1", "\"'\uFF1D1\"");
            yield return NamedCase.Of("fullwidth_plus", "\uFF0B1", "\"'\uFF0B1\"");
            yield return NamedCase.Of("fullwidth_minus", "\uFF0D1", "\"'\uFF0D1\"");
            yield return NamedCase.Of("fullwidth_at", "\uFF20x", "\"'\uFF20x\"");
            yield return NamedCase.Of("plus", "+1", "\"'+1\"");
            yield return NamedCase.Of("minus_formula", "-1+2", "\"'-1+2\"");
            yield return NamedCase.Of("at_sum", "@SUM(A1)", "\"'@SUM(A1)\"");
            yield return NamedCase.Of("negative_number_no_exemption", "-1", "\"'-1\"");
            yield return NamedCase.Of("equals_not_first", "a=1", "\"a=1\"");
            yield return NamedCase.Of("plain_version", "1.10", "\"1.10\"");
        }

        private static IEnumerable<TestCaseData> CsvCellStartRuleCases()
        {
            var fullWidthEquals = ((char)0xFF1D).ToString();

            yield return NamedCase.Of("rule_semicolon_equals", "x;=1+1;y", "\"x;'=1+1;y\"");
            yield return NamedCase.Of("rule_semicolon_quoted_formula", "x;\"=1+1\";y", "\"x;'\"\"=1+1\"\";y\"");
            yield return NamedCase.Of("rule_semicolon_space_quote_plus", "x; \"+1", "\"x; '\"\"+1\"");
            yield return NamedCase.Of("rule_comma_space_at", "a, @b", "\"a, '@b\"");
            yield return NamedCase.Of("rule_every_cell", "=a;=b,-c", "\"'=a;'=b,'-c\"");
            yield return NamedCase.Of("rule_lf_then_equals", "x\n=cmd", "\"x\n'=cmd\"");
            yield return NamedCase.Of("rule_semicolon_lf_text_unchanged", "step1;\nstep2", "\"step1;\nstep2\"");
            yield return NamedCase.Of("rule_comma_tab_text_unchanged", "a,\tb", "\"a,\tb\"");
            yield return NamedCase.Of("rule_plain_list_unchanged", "bug;security", "\"bug;security\"");
            yield return NamedCase.Of("rule_negative_after_comma_tradeoff", "1,-1", "\"1,'-1\"");
            yield return NamedCase.Of("rule_quote_without_trigger_unchanged", "x;\"hello\"", "\"x;\"\"hello\"\"\"");
            yield return NamedCase.Of("rule_leading_hyperlink", "=HYPERLINK(\"http://x\",\"c\")",
                "\"'=HYPERLINK(\"\"http://x\"\",\"\"c\"\")\"");

            yield return NamedCase.Of("derived_crlf_then_equals", "x\r\n=cmd", "\"x\r\n'=cmd\"");
            yield return NamedCase.Of("derived_semicolon_lf_equals", "x;\n=1", "\"x;\n'=1\"");
            yield return NamedCase.Of("derived_semicolon_tab_equals", "x;\t=1", "\"x;\t'=1\"");
            yield return NamedCase.Of("derived_lf_quoted_formula", "x\n\"=1\"", "\"x\n'\"\"=1\"\"\"");
            yield return NamedCase.Of("derived_semicolon_fullwidth_equals", "x;" + fullWidthEquals + "1",
                "\"x;'" + fullWidthEquals + "1\"");
        }

        private static class CsvColumn
        {
            internal const int CodePath = 0;
            internal const int Url = 1;
            internal const int Author = 2;
            internal const int Copyright = 3;
            internal const int AppliedOn = 4;
            internal const int Comment = 5;
            internal const int Version = 6;
            internal const int Tags = 7;
            internal const int WorkItemId = 8;
            internal const int IsParent = 9;
            internal const int IsHistory = 10;
            internal const int Count = 11;

            internal static readonly int[] HistoryText = { CodePath, Url, Author, Copyright, Comment, Version, Tags, WorkItemId };
        }

        private static class Rfc4180Parser
        {
            internal static bool IsParentRow(List<string> record)
                => record[CsvColumn.IsParent] == "1" && record[CsvColumn.IsHistory] == "0";

            internal static bool IsChildRow(List<string> record)
                => record[CsvColumn.IsParent] == "0" && record[CsvColumn.IsHistory] == "0";

            internal static bool IsHistoryRow(List<string> record)
                => record[CsvColumn.IsHistory] == "1";

            internal static bool IsParentHistoryRow(List<string> record)
                => record[CsvColumn.IsParent] == "1" && IsHistoryRow(record);

            internal static string Unquote(string field)
                => field.Substring(1, field.Length - 2).Replace("\"\"", "\"");

            internal static List<List<string>> Parse(string text)
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
                            Close(records, record);
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
                    Close(records, record);
                }

                return records;
            }

            private static void Close(List<List<string>> records, List<string> record)
            {
                if (record.Count != CsvColumn.Count)
                    throw new FormatException($"record {records.Count} has {record.Count} fields, expected {CsvColumn.Count}");

                records.Add(record);
            }
        }
    }
}
