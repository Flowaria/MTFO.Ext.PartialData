using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MTFO.Ext.PartialData.Utils
{
    public static class GearJSONUtil
    {
        private const string COMP_CHARS = "abcdefghijklmnopqrst";

        public static bool TryProcessGUID(string gearjson, string namespaceStr, out string processedJson)
        {
            var newGearJson = gearjson;
            var isChanged = false;
            using var doc = JsonDocument.Parse(gearjson);

            if (!doc.RootElement.TryGetProperty("Packet", out var packetProp))
            {
                processedJson = string.Empty;
                return false;
            }
                

            if (!packetProp.TryGetProperty("Comps", out var compProp))
            {
                processedJson = string.Empty;
                return false;
            }
                

            foreach (var c in COMP_CHARS)
            {
                if (!compProp.TryGetProperty(c.ToString(), out var compEProp))
                    continue;

                if (!compEProp.TryGetProperty("v", out var valueProp))
                    continue;

                if (valueProp.ValueKind != JsonValueKind.String)
                    continue;

                var strValue = valueProp.GetString();
                Logger.Warning($"Found String id: {strValue}");

                var id = PersistentIDManager.GetId(strValue);
                if (id != 0)
                {
                    var regex = new Regex($"(\"v\")\\s*:\\s*(\"{strValue}\")");
                    newGearJson = regex.Replace(newGearJson, $"\"v\":{id}");

                    isChanged = true;
                }
            }

            if (isChanged)
            {
                Logger.Warning(gearjson);
                Logger.Warning(newGearJson);

                processedJson = newGearJson;
                return true;
            }

            processedJson = string.Empty;
            return false;
        }
    }
}
