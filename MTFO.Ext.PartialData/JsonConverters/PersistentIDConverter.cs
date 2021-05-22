using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTFO.Ext.PartialData.JsonConverters
{
    public class PersistentIDConverter : JsonConverter<uint>
    {
        public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return PersistentIDManager.GetId(reader.GetString());
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetUInt32();
            }
            throw new JsonException($"TOKEN IS NOT VALID!");
        }

        public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
