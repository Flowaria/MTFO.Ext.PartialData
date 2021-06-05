using GameData;
using Gear;
using LevelGeneration;
using MTFO.Ext.PartialData.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFO.Ext.PartialData.DataBlockTypes
{
    internal static class DataBlockTypeManager
    {
        private readonly static List<IDataBlockType> _DataBlockCache = new List<IDataBlockType>();
        private readonly static List<IDBuffer> _DataBlockIdBuffers = new List<IDBuffer>();

        public static bool Initialize()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var asm = assemblies.First(a => !a.IsDynamic && a.Location.EndsWith("Modules-ASM.dll"));

                var dataBlockTypes = new List<Type>();
                foreach (var type in asm.ExportedTypes)
                {
                    if (type == null)
                        continue;

                    if (string.IsNullOrEmpty(type.Namespace))
                        continue;

                    if (!type.Namespace.Equals("GameData"))
                        continue;

                    var baseType = type.BaseType;
                    if (baseType == null)
                        continue;

                    if (!baseType.Name.Equals("GameDataBlockBase`1"))
                    {
                        continue;
                    }

                    dataBlockTypes.Add(type);
                }

                var genericBaseType = typeof(DataBlockTypeWrapper<>);
                foreach (var type in dataBlockTypes)
                {
                    var genericType = genericBaseType.MakeGenericType(type);
                    var cache = (IDataBlockType)Activator.CreateInstance(genericType);
                    AssignForceChangeMethod(cache);
                    _DataBlockCache.Add(cache);
                    _DataBlockIdBuffers.Add(new IDBuffer());
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Can't make cache from Modules-ASM.dll!: {e}");
                return false;
            }
        }

        public static void AssignForceChangeMethod(IDataBlockType blockTypeCache)
        {
            //TODO: Better Support
            switch (blockTypeCache.GetShortName().ToLower())
            {
                case "fogsettings":
                    blockTypeCache.RegisterOnChangeEvent(() =>
                    {
                        if (Builder.CurrentFloor.IsBuilt)
                        {
                            Logger.Error("Fog Transition Code is not working properly, Skipping this one");
                            return;

                            var instance = EnvironmentStateManager.Current;
                            Logger.Error("Read Replicator");
                            var replicator = instance.m_stateReplicator;
                            Logger.Error("Read State");
                            var state = replicator.State;
                            Logger.Error("Read FogState");
                            var fogState = state.FogState;
                            Logger.Error("Read ID");
                            var id = fogState.FogDataID;
                            Logger.Error($"FogTransition {id}");

                            if (!GameDataBlockBase<FogSettingsDataBlock>.HasBlock(id))
                                id = RundownManager.ActiveExpedition.Expedition.FogSettings;

                            if (GameDataBlockBase<FogSettingsDataBlock>.HasBlock(id))
                            {
                                EnvironmentStateManager.SetFogSettingsLocal(id);
                            }
                        }
                    });
                    break;

                case "lightsettings":
                    blockTypeCache.RegisterOnChangeEvent(() =>
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
                    });
                    break;
            }
        }

        public static bool TryFindCache(string blockTypeName, out IDataBlockType cache)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                cache = _DataBlockCache[index];
                return true;
            }

            cache = null;
            return false;
        }

        public static bool TryGetNextID(string blockTypeName, out uint id)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                id = _DataBlockIdBuffers[index].GetNext();
                return true;
            }

            id = 0;
            return false;
        }

        public static void SetIDBuffer(string blockTypeName, uint id)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                _DataBlockIdBuffers[index].CurrentID = id;
            }
        }

        public static void SetIDBuffer(string blockTypeName, uint id, IncrementMode mode)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                var buffer = _DataBlockIdBuffers[index];
                buffer.CurrentID = id;
                buffer.IncrementMode = mode;
            }
        }

        private static int GetIndex(string blockTypeName)
        {
            blockTypeName = GetBlockName(blockTypeName);
            return _DataBlockCache.FindIndex(x => x.GetShortName().Equals(blockTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetBlockName(string blockTypeName)
        {
            blockTypeName = blockTypeName.Trim();
            if (blockTypeName.EndsWith("DataBlock"))
                blockTypeName = blockTypeName[0..^9];

            return blockTypeName;
        }
    }
}