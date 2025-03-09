using Il2CppJsonNet;
using Il2CppJsonNet.Linq;
using InjectLib.JsonNETInjection.Converter;
using Localization;

namespace MTFO.Ext.PartialData.JsonConverters.InjectLibConverters;
internal class Il2CppLocalizedTextConverter : Il2CppJsonReferenceTypeConverter<LocalizedText>
{
    protected override LocalizedText Read(JToken jToken, LocalizedText existingValue, JsonSerializer serializer)
    {
        LocalizedText value = new LocalizedText();

        switch (jToken.Type)
        {
            case JTokenType.Integer:
                value.Id = (uint)jToken;
                value.UntranslatedText = null;
                break;

            case JTokenType.String:
                value.Id = 0;
                value.UntranslatedText = (string)jToken;
                break;
        }

        return value;
    }

    protected override void Write(JsonWriter writer, LocalizedText value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else if (value.Id == 0)
            writer.WriteValue(value.UntranslatedText);
        else
            writer.WriteValue(value.Id);
    }
}
