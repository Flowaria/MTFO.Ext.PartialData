using GameData;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTFO.Ext.PartialData.JsonConverters
{
    internal class LanguageDataConverter : JsonConverter<LanguageData>
    {
        public override bool HandleNull => false;

        public override LanguageData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var langData = new LanguageData();

            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            return langData;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected PropertyName token");

                        var propName = reader.GetString();
                        reader.Read();

                        switch (propName.ToLower())
                        {
                            case "translation":
                                langData.Translation = reader.GetString();
                                break;

                            case "shouldtranslate":
                                langData.ShouldTranslate = reader.GetBoolean();
                                break;
                        }
                    }
                    throw new JsonException("Expected EndObject token");

                case JsonTokenType.String:
                    langData.Translation = reader.GetString();
                    langData.ShouldTranslate = false;
                    return langData;

                default:
                    throw new JsonException($"LangaugeDataJson type: {reader.TokenType} is not implemented!");
            }
        }

        public override void Write(Utf8JsonWriter writer, LanguageData value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}

