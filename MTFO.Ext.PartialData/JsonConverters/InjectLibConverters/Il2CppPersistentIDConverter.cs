using Il2CppJsonNet;
using Il2CppJsonNet.Linq;
using Il2CppSystem;
using InjectLib.JsonNETInjection.Converter;
using MTFO.Ext.PartialData.Utils;

namespace MTFO.Ext.PartialData.JsonConverters.InjectLibConverters;
internal class Il2CppPersistentIDConverter : Il2CppJsonUnmanagedTypeConverter<uint>
{
    protected override uint Read(JToken jToken, uint existingValue, JsonSerializer serializer)
    {
        uint value = 0;
        switch (jToken.Type)
        {
            case JTokenType.Integer:
                value = (uint)jToken;
                break;

            case JTokenType.String:
                var str = (string)jToken;
                if (PersistentIDManager.TryGetId(str, out var id))
                {
                    value = id;
                    if (EntryPoint.LogInjectLibLink)
                    {
                        Logger.Log($"InjectLib_PData: Linked GUID: '{str}' to persistentID: '{id}'");
                    }
                }
                else
                {
                    Logger.Error($"InjectLib_PData: Unable to find persistent id from GUID: {str}");
                }
                break;
        }

        return value;
    }

    protected override void Write(JsonWriter writer, uint value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }

    protected override Object ToIl2CppObject(uint value)
    {
        return new Il2CppSystem.UInt32() { m_value = value }.BoxIl2CppObject();
    }
}
