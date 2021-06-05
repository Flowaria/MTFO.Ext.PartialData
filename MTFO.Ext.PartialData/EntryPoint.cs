using AssetShards;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using MTFO.Ext.PartialData.DataBlockTypes;
using MTFO.Ext.PartialData.Utils;
using System.IO;

namespace MTFO.Ext.PartialData
{
    [BepInPlugin("MTFO.Extension.PartialBlocks", "MTFO pDataBlock", "1.3.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.MTFOGUID, BepInDependency.DependencyFlags.HardDependency)]
    internal class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            Logger.LogInstance = Log;
            var useDevMsg = Config.Bind(new ConfigDefinition("Logging", "UseLog"), false, new ConfigDescription("Using Log Message for Debug?"));
            var useLiveEdit = Config.Bind(new ConfigDefinition("Developer", "UseLiveEdit"), false, new ConfigDescription("Using Live Edit?"));
            Logger.UsingLog = useDevMsg.Value;
            PartialDataManager.CanLiveEdit = useLiveEdit.Value;

            if (!DataBlockTypeManager.Initialize())
            {
                Logger.Error("Unable to Initialize DataBlockTypeCache");
                return;
            }
            if (!PartialDataManager.Initialize())
            {
                Logger.Error("Unable to Initialize PartialData");
                return;
            }

            PersistentIDManager.DumpToFile(Path.Combine(PartialDataManager.PartialDataPath, "_persistentID.json"));
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

            PartialDataManager.LoadPartialData();
            PartialDataManager.WriteAllFile(Path.Combine(MTFOUtil.GameDataPath, "CompiledPartialData"));
        }
    }
}