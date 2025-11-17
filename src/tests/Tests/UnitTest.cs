// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.Tests
//  Author           : RzR
//  Created On       : 2022-12-14 09:35
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:47
// ***********************************************************************
//  <copyright file="UnitTest.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Linq;
using System.Reflection;
using CodeSource.Extensions;
using CodeSource.Helpers;
using CodeSource.Services;
using NUnit.Framework;

#endregion

namespace Tests
{
    public class UnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var codeSource = CodeSourceScanner.Instance.FindAnnotations("TempLib").ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.TempClassData")));
        }

        [Test]
        public void Test2()
        {
            var codeSource = CodeSourceScanner.Instance.FindAnnotations("TempLib").ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.TempClassData")));
        }

        [Test]
        public void Test3()
        {
            var assembly = Assembly.Load("TempLib");
            var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly).ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.TempClassData")));
        }
    }
}