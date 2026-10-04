using System;
using System.Collections.Generic;
using RzR.Core.CodeSource.Models;

namespace Tests.Support
{
    internal static class CodeSourceResults
    {
        internal const string SampleLastValue = "END-OF-DATA";

        internal static CodeSourceObjectHistory History(string codePath = null, string sourceUrl = null,
            string authorName = null, string copyright = null, DateTime? appliedOn = null, string comment = null,
            string version = null, string tags = null, string relatedTaskId = null)
            => new CodeSourceObjectHistory
            {
                CodePath = codePath,
                SourceUrl = sourceUrl,
                AuthorName = authorName,
                Copyright = copyright,
                AppliedOn = appliedOn,
                Comment = comment,
                Version = version,
                Tags = tags,
                RelatedTaskId = relatedTaskId
            };

        internal static CodeSourceObjectHistory HistoryWithEveryField(string value, DateTime? appliedOn = null)
            => History(codePath: value, sourceUrl: value, authorName: value, copyright: value, appliedOn: appliedOn,
                comment: value, version: value, tags: value, relatedTaskId: value);

        internal static CodeSourceObject NewObject(string name, string fullName,
            params CodeSourceObjectHistory[] history)
            => new CodeSourceObject
            {
                Name = name,
                FullName = fullName,
                History = new List<CodeSourceObjectHistory>(history)
            };

        internal static CodeSourceObjectsResult Result(CodeSourceObject parent, params CodeSourceObject[] children)
            => new CodeSourceObjectsResult { Parent = parent, Children = new List<CodeSourceObject>(children) };

        internal static List<CodeSourceObjectsResult> Uniform(string payload, DateTime? appliedOn = null)
            => new List<CodeSourceObjectsResult>
            {
                Result(
                    NewObject(payload, payload, HistoryWithEveryField(payload, appliedOn)),
                    NewObject(payload, payload, HistoryWithEveryField(payload, appliedOn)))
            };

        internal static List<CodeSourceObjectsResult> Sample()
            => new List<CodeSourceObjectsResult>
            {
                Result(
                    NewObject("OwnClassData", "TempLib.OwnClassData",
                        History(codePath: "TempLib.OwnClassData", sourceUrl: "https://example.com/a?x=1&y=2",
                            authorName: "RzR", copyright: "\u00A9 RzR", appliedOn: new DateTime(2022, 12, 12),
                            comment: "Comment with \"quotes\", commas and <tags>", version: "1.0", tags: "tag1;tag2",
                            relatedTaskId: "WI-1"),
                        History(codePath: "TempLib.OwnClassData", version: "1.10")),
                    NewObject("Method1", "TempLib.OwnClassData.Method1",
                        History(codePath: "TempLib.OwnClassData.Method1", sourceUrl: "http://example.org/",
                            authorName: "Someone", appliedOn: new DateTime(2023, 1, 31), comment: "a|b `c`",
                            version: "2.0")),
                    NewObject("Method2", "TempLib.OwnClassData.Method2")),
                Result(
                    NewObject("TempClassData", "TempLib.TempClassData",
                        History(codePath: "TempLib.TempClassData", comment: "line1\nline2", tags: SampleLastValue)))
            };
    }
}
