using System;
using ikvm.extensions;
using java.io;
using org.apache.tika.metadata;
using org.apache.tika.mime;
using org.apache.tika.parser;
using org.apache.tika.sax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.apache.tika;

namespace TikaXpsParser.Tests
{
    [TestClass]
    public class XpsParserTests
    {
        [TestMethod]
        public void TestTikaAutodetect()
        {
            Tika tika = new Tika();
            File xpsFile = new File("samples\\test1.xps");
		    if (!xpsFile.isFile())
			    throw new Exception(xpsFile.getName() + " does not exists.");

            using (InputStream inputStream = new FileInputStream(xpsFile))
            {
                Metadata metadata = new Metadata();

                string mimeType = tika.detect(inputStream, metadata);
                Assert.AreEqual("application/x-tika-ooxml", mimeType);

                inputStream.close();
            }
        }

        [TestMethod]
        public void TestSample1()
        {
            var result = GetContent("samples\\test1.xps");
            Assert.IsTrue(result.Contains("4111 1111 1111 1111"));
        }

        [TestMethod]
        public void TestSample2()
        {
            var result = GetContent("samples\\test2.xps");

            Assert.IsTrue(result.Contains("Collection of the Dresses of Different Nations"));
        }

        private static string GetContent(string fileName)
        {
            using (InputStream stream = new FileInputStream(new File(fileName)))
            {
                AutoDetectParser parser = new AutoDetectParser();
                BodyContentHandler handler = new BodyContentHandler();
                Metadata metadata = new Metadata();

                var xpsParser = new XpsParser();

                parser.setParsers(new java.util.HashMap { { MediaType.application("vnd.ms-xpsdocument"), xpsParser } });
                parser.setParsers(new java.util.HashMap { { MediaType.application("x-tika-ooxml"), xpsParser } });

                parser.parse(stream, handler, metadata);

                return handler.toString();
            }
        }
    }
}
