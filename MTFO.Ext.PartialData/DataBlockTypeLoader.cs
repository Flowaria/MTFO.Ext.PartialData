using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFO.Ext.PartialData
{
    public static class DataBlockTypeLoader
    {
        private readonly static Type[] _HasBlockTypeFilter = new Type[] { typeof(uint) };
        private readonly static List<DataBlockTypeCache> _DataBlockCache = new List<DataBlockTypeCache>();
        public static void CacheAll()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var asm = assemblies.First(a => !a.IsDynamic && a.Location.EndsWith("Modules-ASM.dll"));

                var dataBlockNameCache = new List<string>();
                foreach (var cppType in GameDataInit.m_allDataBlockTypes)
                {
                    dataBlockNameCache.Add(cppType.Name);
                }

                var dataBlockTypes = new List<Type>();
                foreach (var type in asm.ExportedTypes)
                {
                    if (type == null)
                        continue;

                    if (string.IsNullOrEmpty(type.Namespace))
                        continue;

                    if (!type.Namespace.Equals("GameData"))
                        continue;

                    if (dataBlockNameCache.Contains(type.Name))
                        dataBlockTypes.Add(type);
                }

                var genericBaseType = typeof(GameDataBlockBase<>);
                foreach (var type in dataBlockTypes)
                {
                    var genericType = genericBaseType.MakeGenericType(type);
                    var cache = new DataBlockTypeCache()
                    {
                        TypeName = type.Name,
                        SerializeType = type,
                        AddBlockMethod = genericType.GetMethod("AddBlock"),
                        GetBlockMethod = genericType.GetMethod("GetBlock", _HasBlockTypeFilter)
                    };
                    AssignForceChangeMethod(cache);
                    _DataBlockCache.Add(cache);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Can't make cache from Modules-ASM.dll!: {e}");
            }
        }

        public static void AssignForceChangeMethod(DataBlockTypeCache blockTypeCache)
        {
            //TODO: Better Support

            switch(blockTypeCache.TypeName.ToLower())
            {
                case "fogsettingsdatablock":
                    blockTypeCache.OnForceChange = () =>
                    {

                        var set = EnvironmentStateManager.Current.m_stateReplicator.m_currentState.FogState.FogDataID;
                        EnvironmentStateManager.SetFogSettingsLocal(set);
                    };
                    break;

                case "lightsettingsdatablock":
                    blockTypeCache.OnForceChange = () =>
                    {
                        if (!Builder.CurrentFloor.IsBuilt)
                        {
                            return;
                        }

                        foreach (var zone in Builder.CurrentFloor.allZones)
                        {
                            foreach (var node in zone.m_courseNodes)
                            {
                                LG_BuildZoneLightsJob.ApplyLightSettings(node.m_lightsInNode, zone.m_lightSettings, false);
                            }
                        }
                    };
                    break;
            }
        }

        public static bool TryFindCache(string blockTypeName, out DataBlockTypeCache cache)
        {
            var index = -1;
            if ((index = _DataBlockCache.FindIndex(x => x.TypeName.Equals(blockTypeName))) != -1)
            {
                cache = _DataBlockCache[index];
                return true;
            }

            cache = null;
            return false;
        }
    }
}