using System.Linq;

namespace Tests.Support
{
    internal static class ExportSchema
    {
        internal const string AppliedOnKey = "appliedOn";

        internal static readonly string[] HistoryKeys =
        {
            "codePath", "sourceUrl", "authorName", "copyright", AppliedOnKey, "comment", "version", "tags",
            "relatedTaskId"
        };

        internal static readonly string[] TextHistoryKeys = HistoryKeys.Where(key => key != AppliedOnKey).ToArray();
    }
}
