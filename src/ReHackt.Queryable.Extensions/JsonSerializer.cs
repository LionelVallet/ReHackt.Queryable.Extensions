using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ReHackt.Queryable.Extensions
{
    internal static class JsonSerializer
    {
        internal static T Deserialize<T>(string body)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(body);
            writer.Flush();
            stream.Position = 0;
            return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
        }

        internal static string Serialize<T>(T item)
        {
            using MemoryStream ms = new MemoryStream();
            new DataContractJsonSerializer(typeof(T)).WriteObject(ms, item);
            return Encoding.Default.GetString(ms.ToArray());
        }
    }
}
