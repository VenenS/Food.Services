using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace ITWebNet.Food.Controllers
{
    public class XslttRansform
    {
        public string XsltTransformation(string inputXml, string inputXslt)
        {
            string resultOfTransformation;

            using (var stringReader = new StringReader(inputXml))
            {
                using (var readerForXml = XmlReader.Create(stringReader))
                {
                    using (var stringWriter = new StringWriter())
                    {
                        using (var readerForXslt = new StringReader(inputXslt))
                        {
                            using (var xmlReaderForXslt = XmlReader.Create(readerForXslt))
                            {
                                var xsl = new XslCompiledTransform();
                                xmlReaderForXslt.ReadToDescendant("xsl:stylesheet");
                                xsl.Load(xmlReaderForXslt);
                                using (var writerToOutput = XmlWriter.Create(stringWriter, xsl.OutputSettings))
                                {
                                    xsl.Transform(readerForXml, writerToOutput);
                                }
                            }
                        }
                        resultOfTransformation = stringWriter.GetStringBuilder().ToString();
                    }
                }
            }
            return resultOfTransformation;
        }
    }
}