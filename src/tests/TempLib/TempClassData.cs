// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.TempLib
//  Author           : RzR
//  Created On       : 2022-12-14 15:54
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:47
// ***********************************************************************
//  <copyright file="TempClassData.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Threading.Tasks;
using CodeSource;

#endregion

namespace TempLib
{
    [CodeSource("http://local.host", "User01", "Company INC", version: 1)]
    public class TempClassData
    {
        [CodeSource("http://local.host", "User1", "Company INC", "2022-12-01", "CTOR init")]
        public TempClassData()
        {
        }

        [CodeSource("http://local.host/source1", "User2", "Company INC", "2022-12-10", "IDK")]
        [CodeSource(SourceUrl = "http://local.host/source2", AuthorName = "User3", Copyright = "Company INC", AppliedOn = "2022-12-12", Comment = "IDK", Version = 1.1)]
        [CodeSource(SourceUrl = "http://local.host/source2", AuthorName = "User4", Copyright = "Company INC", AppliedOn = "2022-12-14", Comment = "IDK4", Version = 1.2, Tags = "bug;security", RelatedTaskId = "#74")]
        public void Run()
        {
        }

        public Task RunTask()
        {
            return Task.FromResult(new Task(() => { }));
        }

        public async Task RunAsync()
        {
            await Task.Run(() => Task.FromResult(RunAsync()));
        }
    }
}