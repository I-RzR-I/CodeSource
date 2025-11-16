using CodeSource.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests40
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var codeSource = CodeSourceHelper.GetCodeSourceAssembly("TempLib2").ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib.TempClassData")));
        }
    }
}
