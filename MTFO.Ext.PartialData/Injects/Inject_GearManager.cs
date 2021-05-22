using GameData;
using Gear;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MTFO.Ext.PartialData.Injects
{
    [HarmonyPatch(typeof(GearManager))]
    class Inject_GearManager
    {
        const string COMP_CHARS = "abcdefghijklmnopqrst";

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(GearManager.LoadOfflineGearDatas))]
        static void Pre_LoadOfflineGearDatas()
        {
            var allBlocks = GameDataBlockBase<PlayerOfflineGearDataBlock>.GetAllBlocks();
            foreach(var block in allBlocks)
            {
                if (!block.internalEnabled || string.IsNullOrEmpty(block.GearJSON))
                    continue;

                if (block.Type != eOfflineGearType.StandardInventory && block.Type != eOfflineGearType.RundownSpecificInventory)
                    continue;

                var newGearJson = block.GearJSON;
                var isChanged = false;
                using var doc = JsonDocument.Parse(block.GearJSON);

                if (!doc.RootElement.TryGetProperty("Packet", out var packetProp))
                    continue;

                if (!packetProp.TryGetProperty("Comps", out var compProp))
                    continue;
                    

                foreach(var c in COMP_CHARS)
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
                    Logger.Warning(block.GearJSON);
                    Logger.Warning(newGearJson);

                    block.GearJSON = newGearJson;
                }
            }
        }
    }
}
