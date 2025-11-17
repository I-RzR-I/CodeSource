#region U S A G E S

using System.Threading.Tasks;
using CodeSource;

#endregion

namespace TempLib45
{
    [CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC", version: 1)]
    [CodeSource(SourceUrl = "http://local.host", AuthorName = "USR1")]
    public class OwnClassData
    {
        public void Run()
        {
        }

        public Task RunTask()
        {
            return new Task(() => { });
        }

        [CodeSource("http://local.host/use-async", "User2", "Company INC", "2022-12-12", "IDK how to use async", version: 1.0D)]
        [CodeSource("http://local.host/use-async", "User2", "Company INC", "2024-12-12", "IDK", version: 1.1D)]
        public Task RunAsync()
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}