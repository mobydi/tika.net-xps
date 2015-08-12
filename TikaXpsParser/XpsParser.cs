using System;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Xml;
using org.apache.tika.metadata;
using org.apache.tika.mime;
using org.apache.tika.parser;
using org.apache.tika.sax;
using org.xml.sax;

namespace TikaXpsParser
{
    public class XpsParser : AbstractParser
    {
        private static java.util.Set SUPPORTED_TYPES = java.util.Collections.singleton(MediaType.application("vnd.ms-xpsdocument"));
        private static String XPS_MIME_TYPE = "application/vnd.ms-xpsdocument";

        const string UniStr = "UnicodeString";
        const string Glyphs = "Glyphs";

        public override void parse(java.io.InputStream stream, ContentHandler handler, Metadata metadata, ParseContext context)
        {
            metadata.set(Metadata.TYPE, XPS_MIME_TYPE);

            try
            {
                var getFile = stream.getClass().getMethod("getFile");
                var file = getFile.invoke(stream);
                var path = file.ToString();

                var xhtml = new XHTMLContentHandler(handler, metadata);
                xhtml.startDocument();

                using (var xpsDocument = new XpsDocument(path, FileAccess.Read))
                {
                    var fixedDocSeqReader = xpsDocument.FixedDocumentSequenceReader;
                    if (fixedDocSeqReader == null)
                        return;

                    foreach (var document in fixedDocSeqReader.FixedDocuments)
                    {
                        var page = document.FixedPages[0];
                        using (var pageContentReader = page.XmlReader)
                        {
                            parsePage(xhtml, pageContentReader);
                        }
                    }
                }

                xhtml.endDocument();
            }
            catch (Exception e)
            {
                throw new java.io.IOException(e);
            }
        }

        private static void parsePage(XHTMLContentHandler xhtml, XmlReader pageContentReader)
        {
            xhtml.startElement("span");

            if (pageContentReader == null)
                return;

            while (pageContentReader.Read())
            {
                if (pageContentReader.Name != Glyphs)
                    continue;
                if (!pageContentReader.HasAttributes)
                    continue;

                var text = pageContentReader.GetAttribute(UniStr);
                if (string.IsNullOrEmpty(text))
                    continue;

                xhtml.element("div", text);
            }

            xhtml.endElement("span");
        }

        public override java.util.Set getSupportedTypes(ParseContext context)
        {
            return SUPPORTED_TYPES;
        }
    }
}
