using AssetShards;
using BepInEx;
using BepInEx.IL2CPP;
using MTFO.Ext.PartialData.JsonConverters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using UnhollowerRuntimeLib;
using GameData;
using UnhollowerBaseLib;
using HarmonyLib;
using System.Text.Json;
using System;

namespace MTFO.Ext.PartialData
{
    [BepInPlugin("MTFO.Extension.PartialBlocks", "MTFO pDataBlock", "1.2.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.MTFOGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            Logger.LogInstance = Log;
            var useDevMsg = Config.Bind(new ConfigDefinition("Logging", "UseLog"), false, new ConfigDescription("Using Log Message for Debug?"));
            var useLiveEdit = Config.Bind(new ConfigDefinition("Developer", "UseLiveEdit"), false, new ConfigDescription("Using Live Edit?"));
            Logger.UsingLog = useDevMsg.Value;
            PartialDataManager.CanLiveEdit = useLiveEdit.Value;

            if (!PartialDataManager.Initialize())
            {
                Logger.Warning("Unable to Initialize PartialData");
                return;
            }

            PersistentIDManager.WriteToFile(Path.Combine(PartialDataManager.PartialDataPath, "persistentID.json"));
            AssetShardManager.add_OnStartupAssetsLoaded((Il2CppSystem.Action)OnAssetLoaded);

            var harmony = new Harmony("MTFO.pBlock.Harmony");
            harmony.PatchAll();
        }

        private bool once = false;

        private void OnAssetLoaded()
        {
            if (once)
                return;
            once = true;

            DataBlockTypeWrapper.CacheAll();
            PartialDataManager.LoadPartialData();
            PartialDataManager.WriteAllFile(Path.Combine(MTFOUtil.GameDataPath, "CompiledPartialData"));
        }
    }
}