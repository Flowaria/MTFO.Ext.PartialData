using MTFO.Ext.PartialData.JsonConverters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTFO.Ext.PartialData.Utils
{
    internal static class JSON
    {
        private readonly static JsonSerializerOptions _Setting = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        static JSON()
        {
            _Setting.Converters.Add(new PersistentIDConverter());
            _Setting.Converters.Add(new Il2CppListConverterFactory());
            _Setting.Converters.Add(new ColorConverter());
            _Setting.Converters.Add(new JsonStringEnumConverter());
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _Setting);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, _Setting);
        }

        public static string Serialize(object obj, Type type)
        {
            return JsonSerializer.Serialize(obj, type, _Setting);
        }
    }
}