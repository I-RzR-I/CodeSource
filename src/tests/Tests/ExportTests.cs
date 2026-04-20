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

using CodeSource;
using CodeSource.Services;
using CodeSource.Services.Export;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
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

            using (var stream = new FileStream($"GenerateMarkDown_Test_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.md", FileMode.Create))
                new MarkdownExporter().Export(codeSource, stream);
        }

        [Test]
        public void GenerateCsv_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            using (var stream = new FileStream($"GenerateCsv_Test_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv", FileMode.Create))
                new CsvExporter().Export(codeSource, stream);
        }

        [Test]
        public void GenerateHtml_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            using (var stream = new FileStream($"GenerateHtml_Test{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.html", FileMode.Create))
                new HtmlExporter().Export(codeSource, stream);
        }

        [Test]
        public void GenerateYaml_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            using (var stream = new FileStream($"GenerateYaml_Test{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.yaml", FileMode.Create))
                new YamlExporter().Export(codeSource, stream);
        }

        [Test]
        public void GenerateXml_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            using (var stream = new FileStream($"GenerateXml_Test{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xml", FileMode.Create))
                new XmlExporter().Export(codeSource, stream);
        }

        [Test]
        public void GenerateJson_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            using (var stream = new FileStream($"GenerateJson_Test{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.json", FileMode.Create))
                new JsonExporter().Export(codeSource, stream);
        }

        [Test]
        public void ExporterRegistryJson_Test()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();
            
            ExporterRegistry.Export(ExportFormats.Json, codeSource, $"ExporterRegistryJson_Test{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.json");
        }
    }
}