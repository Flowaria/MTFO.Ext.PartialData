using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace MTFO.Ext.PartialData.Converters
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Vector2));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Vector2 result = default;
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jobject = JObject.Load(reader);
                result.x = jobject["x"].Value<float>();
                result.y = jobject["y"].Value<float>();
            }
            return result;
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