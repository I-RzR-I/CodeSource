namespace Tests.Support
{
    internal static class ConditionalSources
    {
        internal const string ExpectedSymbol = "CODESOURCE";

        internal const string AllTargetsSource = @"using System;
using RzR.Core.CodeSource;

[assembly: CodeSource(""u/assembly"")]
[module: CodeSource(""u/module"")]

namespace Cond.AllTargets
{
    [CodeSource(""u/class"")]
    public class AnnotatedClass
    {
        [CodeSource(""u/ctor"")]
        public AnnotatedClass() { }

        [CodeSource(""u/method"")]
        public void Method() { }

        [CodeSource(""u/property"")]
        public int Property { get; set; }

        [CodeSource(""u/field"")]
        public int Field;

        [CodeSource(""u/event"")]
        public event Action Event;

        public void WithParameter([CodeSource(""u/parameter"")] int value) { }

        [return: CodeSource(""u/return"")]
        public int WithReturn() => 0;

        public void GenericMethod<[CodeSource(""u/generic-method-parameter"")] TMethod>() { }
    }

    [CodeSource(""u/struct"")]
    public struct AnnotatedStruct { }

    [CodeSource(""u/interface"")]
    public interface IAnnotated { }

    [CodeSource(""u/enum"")]
    public enum AnnotatedEnum
    {
        [CodeSource(""u/enum-member"")]
        Member
    }

    [CodeSource(""u/delegate"")]
    public delegate void AnnotatedDelegate();

    public class Generic<[CodeSource(""u/generic-type-parameter"")] TType> { }
}
";

        internal static readonly string[] AllTargetsUrls =
        {
            "u/assembly",
            "u/module",
            "u/class",
            "u/ctor",
            "u/method",
            "u/property",
            "u/field",
            "u/event",
            "u/parameter",
            "u/return",
            "u/generic-method-parameter",
            "u/struct",
            "u/interface",
            "u/enum",
            "u/enum-member",
            "u/delegate",
            "u/generic-type-parameter"
        };
    }
}
