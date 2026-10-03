using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RzR.Core.CodeSource.Services;

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