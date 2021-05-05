using AssetShards;
using BepInEx;
using BepInEx.IL2CPP;
using MTFO.Ext.PartialData.Converters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTFO.Ext.PartialData
{
    [BepInPlugin("MTFO.Extension.PartialBlocks", "MTFO pDataBlock", "1.0.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.MTFOGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            Logger.LogInstance = Log;

            AssetShardManager.add_OnStartupAssetsLoaded((Il2CppSystem.Action)OnAssetLoaded);
        }

        private bool once = false;

        private void OnAssetLoaded()
        {
            if (once)
                return;
            once = true;

            PartialDataManager.UpdatePartialData();
            PartialDataManager.WriteToFile(Path.Combine(PartialDataManager.PartialDataPath, "persistentID.json"));
        }
    }
}