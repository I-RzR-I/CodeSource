using System.Threading.Tasks;
using RzR.Core.CodeSource;

namespace TempLib
{
    [CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC", version: "1")]
    [CodeSource(SourceUrl = "http://local.host", AuthorName = "USR1")]
    public class OwnClassData
    {
        public void Run()
        {
        }

        [CodeSource(AppliedOn = "2025-11-22", AuthorName = "RzR", Version = "1.0", RelatedTaskId = "#784", Copyright = "RzR")]
        public Task RunTask()
        {
            return Task.FromResult(new Task(() => { }));
        }

        [CodeSource("http://local.host/use-async", "User2", "Company INC", "2022-12-12", "IDK how to use async", version: "1.0")]
        [CodeSource("http://local.host/use-async", "User2", "Company INC", "2024-12-12", "IDK", version: "1.1")]
        public async Task RunAsync()
        {
            await Task.Run(() => Task.FromResult(RunAsync()));
        }
    }
}