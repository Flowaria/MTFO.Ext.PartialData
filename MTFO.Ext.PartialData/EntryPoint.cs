using AssetShards;
using BepInEx;
using BepInEx.IL2CPP;
using MTFO.Ext.PartialData.Converters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

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

            if (!MTFOUtil.IsLoaded)
                return;

            var basePath = Path.Combine(MTFOUtil.GameDataPath, "PartialData");
            if (!Directory.Exists(basePath))
                return;

            var configPath = Path.Combine(basePath, "_config.json");
            if (!File.Exists(configPath))
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new List<DataBlockDefinition>() { new DataBlockDefinition() }, Formatting.Indented));

            var configs = JsonConvert.DeserializeObject<List<DataBlockDefinition>>(File.ReadAllText(configPath));
            foreach (var def in configs)
            {
                if (DataBlockTypeLoader.TryFindCache(def.TypeName, out var cache))
                {
                    var idBuffer = def.GuidConfig.StartFromID;
                    var idConverter = new PersistentIDConverter
                    {
                        SaveID = (string guid) =>
                        {
                            if (PersistentIDManager.TryAssignId(guid, idBuffer))
                            {
                                if (def.GuidConfig.IncrementMode == MapperIncrementMode.Decrement)
                                    return idBuffer--;
                                else
                                    return idBuffer++;
                            }
                            return 0;
                        },

                        LoadID = (string guid) =>
                        {
                            return PersistentIDManager.GetId(guid);
                        }
                    };

                    Logger.Log($"Found Type Cache from TypeName!: {def.TypeName}");
                    Logger.Log($"Found Those files with filter: {def.FileSearchPattern}");

                    var searchOption = def.CheckSubDirectory ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = Directory.GetFiles(basePath, def.FileSearchPattern, searchOption);
                    foreach (var file in files)
                    {
                        Logger.Log($" - {file}");
                        cache.AddJsonBlock(File.ReadAllText(file), idConverter);
                    }
                }
            }
        }
    }
}