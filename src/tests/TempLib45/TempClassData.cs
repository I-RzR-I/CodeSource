#region U S A G E S

using CodeSource;
using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace TempLib45
{
    [CodeSource("http://local.host", "User1", "Company INC", version: 1)]
    public class TempClassData
    {
        [CodeSource("http://local.host", "User1", "Company INC", "2022-12-12", "CTOR init")]
        public TempClassData()
        {
        }

        [CodeSource("http://local.host/source1", "User2", "Company INC", "2022-12-12", "IDK")]
        public void Run()
        {
        }

        public Task RunTask()
        {
            return new Task(() => { });
        }

        public Task RunAsync()
        {
            return Runex(() => RunTask());
        }


        private static Task Runex(Action delegateAction)
        {
            var task = new Task(delegateAction);
            task.Start();

            return task;
        }
    }
}