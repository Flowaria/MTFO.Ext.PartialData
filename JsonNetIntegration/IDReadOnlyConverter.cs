using Newtonsoft.Json;
using System;

namespace JsonNetIntegration
{
    public class IDReadOnlyConverter : JsonConverter<uint>
    {
        public override uint ReadJson(JsonReader reader, Type objectType, uint existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var id = PartialDataUtil.GetID((string)reader.Value);
                if (id == 0)
                    return existingValue;
                else
                    return id;
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                var value = (long)reader.Value;
                if (value >= 0)
                    return (uint)value;
                else
                    return 0u;
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, uint value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
