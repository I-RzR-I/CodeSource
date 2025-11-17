using CodeSource.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests45
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var codeSource = CodeSourceHelper.GetCodeSourceAssembly("TempLib45").ToList();

            Assert.AreEqual(2, codeSource.Count());
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib45.OwnClassData")));
            Assert.AreEqual(1, codeSource.Count(x => x.Parent.FullName.Equals("TempLib45.TempClassData")));
        }
    }
}
