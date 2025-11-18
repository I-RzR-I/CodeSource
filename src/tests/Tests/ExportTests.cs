// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.Tests
//  Author           : RzR
//  Created On       : 2025-11-17 23:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-17 23:19
// ***********************************************************************
//  <copyright file="ExportTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using CodeSource.Services;
using CodeSource.Services.Export;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Tests
{
    public class ExportTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GenerateMarkDown_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            var stream = new FileStream($"GenerateMarkDown_Test_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.md", FileMode.CreateNew);
            new MarkdownExporter().Export(codeSource, stream);
        }
    }
}