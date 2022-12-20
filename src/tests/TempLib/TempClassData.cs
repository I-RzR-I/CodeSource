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
    [CodeSource("LocalHost", "User1", "Company INC")]
    public class TempClassData
    {
        [CodeSource("LocalHost", "User1", "Company INC", "2022-12-12", "CTOR init")]
        public TempClassData()
        {
        }

        [CodeSource("LocalHost/source1", "User2", "Company INC", "2022-12-12", "IDK")]
        public void Run()
        {
        }

        public Task RunTask()
        {
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            await Task.CompletedTask;
        }
    }
}