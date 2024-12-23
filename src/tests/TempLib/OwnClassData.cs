// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.TempLib
//  Author           : RzR
//  Created On       : 2022-12-14 16:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:47
// ***********************************************************************
//  <copyright file="OwnClassData.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading.Tasks;
using CodeSource;

#endregion

namespace TempLib
{
    [CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC", version: 1)]
    [CodeSource(SourceUrl = "SSS", AuthorName = "USR1")]
    public class OwnClassData
    {
        public void Run()
        {
        }

        public Task RunTask()
        {
            return Task.CompletedTask;
        }

        [CodeSource("LocalHost/use-async", "User2", "Company INC", "2022-12-12", "IDK how to use async", version: 1.0D)]
        [CodeSource("LocalHost/use-async", "User2", "Company INC", "2024-12-12", "IDK", version: 1.1D)]
        public async Task RunAsync()
        {
            await Task.CompletedTask;
        }
    }
}