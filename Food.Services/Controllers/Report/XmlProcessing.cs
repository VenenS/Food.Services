using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ITWebNet.Food.Controllers
{
    public class XmlProcessing<T>
    {
        public string SerializeObject(T objectForSerialize)
        {
            var resultOfSerialization = string.Empty;

            if (objectForSerialize != null)
            {
                var serializer = new XmlSerializer(objectForSerialize.GetType());

                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        using (var xmlWriter =
                            XmlWriter.Create(
                                memoryStream,
                                new XmlWriterSettings
                                {
                                    Indent = true
                                }
                            )
                        )
                        {
                            serializer.Serialize(xmlWriter, objectForSerialize);
                        }

                        memoryStream.Position = 0;

                        using (var streamReader = new StreamReader(memoryStream))
                        {
                            resultOfSerialization = streamReader.ReadToEnd();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return resultOfSerialization;
        }
    }
}