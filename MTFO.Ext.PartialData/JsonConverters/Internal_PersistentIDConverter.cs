using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTFO.Ext.PartialData.JsonConverters
{
    internal class Internal_PersistentIDConverter : JsonConverter<uint>
    {
        public override bool HandleNull => false;

        public bool IsSaveIDMode = true;

        public Func<string, uint> SaveID;
        public Func<string, uint> LoadID;

        public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var guidValue = reader.GetString();
                if (IsSaveIDMode)
                {
                    if (SaveID == null)
                    {
                        throw new Exception($"SaveID has not implemented!");
                    }

                    return SaveID.Invoke(guidValue);
                }
                else
                {
                    if (LoadID == null)
                    {
                        throw new Exception($"LoadID has not implemented!");
                    }

                    return LoadID.Invoke(guidValue);
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetUInt32();
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}