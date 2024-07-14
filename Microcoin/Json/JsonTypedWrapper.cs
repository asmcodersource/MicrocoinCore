using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Microcoin.Json
{
    /// <summary>
    /// Since JsonSerializer.Deserialize does not guarantee the correspondence of the deserialized type, 
    /// I introduce this class for a more strict type definition in json
    /// </summary>
    public static class JsonTypedWrapper
    {
        public class JsonTypedException : Exception 
        {
            public JsonTypedException() { }
            public JsonTypedException(string message) : base(message) { }
        }

        public static string Serialize<T>(T obj)
        {
            var typename = typeof(T).Name;
            var objectJson = JsonSerializer.Serialize(obj);
            var keyValuesPairs = new Dictionary<string, string>{{typename,  objectJson}};
            return JsonSerializer.Serialize(keyValuesPairs);
        }

        public static T? Deserialize<T>(string json) 
        {
            var typename = typeof(T).Name;
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            if (jsonDocument.RootElement.TryGetProperty(typename, out var obj) is true)
                return JsonSerializer.Deserialize<T>(obj.GetString() ?? throw new JsonTypedException("A field of the specified type was found, but it does not contain data"));
            else
                throw new JsonTypedException("The expected type was not found in the provided json");
        }

        public static string? GetWrappedTypeName(string json)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            return jsonDocument.RootElement.EnumerateObject().First().Name;
        }
    }
}
