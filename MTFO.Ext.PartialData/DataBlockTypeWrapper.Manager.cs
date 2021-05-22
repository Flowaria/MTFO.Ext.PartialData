using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MTFO.Ext.PartialData
{
    public partial class DataBlockTypeWrapper
    {
        private readonly static Type[] _HasBlockTypeFilter = new Type[] { typeof(uint) };
        private readonly static List<DataBlockTypeWrapper> _DataBlockCache = new List<DataBlockTypeWrapper>();

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
                    var cache = new DataBlockTypeWrapper()
                    {
                        TypeName = type.Name,
                        SerializeType = type,
                        AddBlockMethod = genericType.GetMethod("AddBlock"),
                        GetBlockMethod = genericType.GetMethod("GetBlock", _HasBlockTypeFilter),
                        DoSaveToDiskMethod = genericType.GetMethod("DoSaveToDisk"),
                        FullPathProperty = genericType.GetProperty("m_filePathFull", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
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

        public static void AssignForceChangeMethod(DataBlockTypeWrapper blockTypeCache)
        {
            //TODO: Better Support

            switch (blockTypeCache.TypeName.ToLower())
            {
                case "fogsettingsdatablock":
                    blockTypeCache.OnForceChange = () =>
                    {
                        if (Builder.CurrentFloor.IsBuilt)
                        {
                            var id = EnvironmentStateManager.Current.m_stateReplicator.m_currentState.FogState.FogDataID;
                            EnvironmentStateManager.AttemptStartFogTransition(id, 5.0f);
                        }
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

        public static bool TryFindCache(string blockTypeName, out DataBlockTypeWrapper cache)
        {
            var index = -1;
            if ((index = _DataBlockCache.FindIndex(x => x.TypeName.Equals(blockTypeName, StringComparison.OrdinalIgnoreCase))) != -1)
            {
                cache = _DataBlockCache[index];
                return true;
            }

            cache = null;
            return false;
        }
    }
}