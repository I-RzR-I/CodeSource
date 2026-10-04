using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Tests.Support
{
    internal static class YamlDocumentReader
    {
        internal const string RootKey = "codeSources";

        internal static YamlMappingNode LoadRoot(string text)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(text));

            Assert.That(stream.Documents, Has.Count.EqualTo(1), "exactly one YAML document");
            var root = (YamlMappingNode)stream.Documents[0].RootNode;
            Assert.That(Keys(root), Is.EquivalentTo(new[] { RootKey }), "root keys");

            return root;
        }

        internal static List<YamlMappingNode> LoadEntries(string text)
            => ((YamlSequenceNode)Get(LoadRoot(text), RootKey)).Children.Cast<YamlMappingNode>().ToList();

        internal static IEnumerable<string> Keys(YamlMappingNode mapping)
            => mapping.Children.Keys.Select(key => ((YamlScalarNode)key).Value);

        internal static YamlNode Get(YamlMappingNode mapping, string key)
            => mapping.Children.FirstOrDefault(entry => ((YamlScalarNode)entry.Key).Value == key).Value;

        internal static void AssertQuotedScalar(YamlMappingNode mapping, string key, string expected)
        {
            var scalar = Get(mapping, key) as YamlScalarNode;
            Assert.That(scalar, Is.Not.Null, $"'{key}' must be a scalar");
            if (scalar == null)
                return;

            Assert.That(scalar.Style, Is.EqualTo(ScalarStyle.DoubleQuoted), $"'{key}' must be double-quoted");
            Assert.That(scalar.Value, Is.EqualTo(expected), $"'{key}' value");
        }
    }
}
