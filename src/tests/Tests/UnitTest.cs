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
using CodeSource.Helpers;
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
            var codeSource = CodeSourceHelper.GetCodeSourceAssembly("TempLib").ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.TempClassData")));
        }
    }
}