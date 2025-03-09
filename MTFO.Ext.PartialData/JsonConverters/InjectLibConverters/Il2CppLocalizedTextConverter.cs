using Il2CppJsonNet;
using Il2CppJsonNet.Linq;
using InjectLib.JsonNETInjection.Converter;
using Localization;
using MTFO.Ext.PartialData.Utils;

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
                var str = (string)jToken;
                if (PersistentIDManager.TryGetId(str, out var id))
                {
                    value.Id = id;
                    value.UntranslatedText = null;

                    if (EntryPoint.LogInjectLibLink)
                    {
                        Logger.Log($"InjectLib_PData: Linked GUID: '{str}' to TextDB: '{id}'");
                    }
                }
                else
                {
                    value.Id = 0;
                    value.UntranslatedText = str;
                }
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
