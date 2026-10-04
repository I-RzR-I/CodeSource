using System.IO;
using System.Xml;

namespace Tests.Support
{
    internal static class StrictXml
    {
        internal static XmlDocument Load(byte[] bytes)
        {
            var settings = new XmlReaderSettings
            {
                CheckCharacters = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var stream = new MemoryStream(bytes);
            using var reader = XmlReader.Create(stream, settings);
            var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            document.Load(reader);

            return document;
        }
    }
}
