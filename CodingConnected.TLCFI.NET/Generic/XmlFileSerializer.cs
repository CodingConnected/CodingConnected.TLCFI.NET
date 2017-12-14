using System.IO;
using System.Xml.Serialization;

namespace CodingConnected.TLCFI.NET.Core.Generic
{
    public static class XmlFileSerializer
    {
        public static T Deserialize<T>(string fileName) where T : class
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException(fileName);
            var serializer = new XmlSerializer(typeof(T));
            T obj;
            using (var stream = File.OpenRead(fileName))
            {
                obj = (T)serializer.Deserialize(stream);
            }
            return obj;
        }
    }
}