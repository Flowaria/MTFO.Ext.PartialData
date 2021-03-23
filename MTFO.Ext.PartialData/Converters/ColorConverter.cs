using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace MTFO.Ext.PartialData.Converters
{
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Color)) || (objectType == typeof(Color32));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default(Color);
            }
            JObject jobject = JObject.Load(reader);
            if (objectType == typeof(Color32))
            {
                return new Color32((byte)jobject["r"], (byte)jobject["g"], (byte)jobject["b"], (byte)jobject["a"]);
            }
            return new Color((float)jobject["r"], (float)jobject["g"], (float)jobject["b"], (float)jobject["a"]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }
}