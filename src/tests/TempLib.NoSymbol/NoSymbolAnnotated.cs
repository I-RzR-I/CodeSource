using System;
using RzR.Core.CodeSource;

[assembly: CodeSource("http://local.host/nosymbol/assembly", AuthorName = "NoSymbol")]

namespace TempLib.NoSymbol
{
    [CodeSource("http://local.host/nosymbol/container", AuthorName = "NoSymbol")]
    public static class NoSymbolAnnotated
    {
        [CodeSource("http://local.host/nosymbol/class", AuthorName = "NoSymbol", Version = "1.0")]
        public class AnnotatedClass
        {
            [CodeSource("http://local.host/nosymbol/field")]
            public int Field;

            [CodeSource("http://local.host/nosymbol/ctor")]
            public AnnotatedClass()
            {
            }

            [CodeSource("http://local.host/nosymbol/property")]
            public string Property { get; set; }

            [CodeSource("http://local.host/nosymbol/method", Comment = "method")]
            public void Method()
            {
            }

            [return: CodeSource("http://local.host/nosymbol/return")]
            public int WithParameter([CodeSource("http://local.host/nosymbol/parameter")] int value)
            {
                return value;
            }
        }

        [CodeSource("http://local.host/nosymbol/struct")]
        public struct AnnotatedStruct
        {
            [CodeSource("http://local.host/nosymbol/struct-method")]
            public void Method()
            {
            }
        }

        [CodeSource("http://local.host/nosymbol/interface")]
        public interface IAnnotated
        {
            [CodeSource("http://local.host/nosymbol/interface-method")]
            void Method();
        }

        [CodeSource("http://local.host/nosymbol/enum")]
        public enum AnnotatedEnum
        {
            [CodeSource("http://local.host/nosymbol/enum-member")]
            Member
        }

        [CodeSource("http://local.host/nosymbol/delegate")]
        public delegate void AnnotatedDelegate(EventArgs args);
    }
}
