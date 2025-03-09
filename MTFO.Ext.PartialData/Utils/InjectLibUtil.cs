using BepInEx.Unity.IL2CPP;
using InjectLib.JsonNETInjection;
using InjectLib.JsonNETInjection.Supports;
using MTFO.Ext.PartialData.JsonConverters.InjectLibConverters;
using System;
using System.Text.Json.Serialization;

namespace MTFO.Ext.PartialData.Utils
{
    public static class InjectLibUtil
    {
        public const string PLUGIN_GUID = "GTFO.InjectLib";

        public static JsonConverter InjectLibConnector { get; private set; } = null;

        public static bool IsLoaded { get; private set; } = false;

        internal static void Setup()
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
            {
                try
                {
                    JsonInjector.SetConverter(new Il2CppPersistentIDConverter());
                    JsonInjector.SetConverter(new Il2CppLocalizedTextConverter());
                    InjectLibConnector = new InjectLibConnector();
                    JSON.Setting.Converters.Add(InjectLibConnector);

                    IsLoaded = true;
                }
                catch (Exception e)
                {
                    Logger.Error($"Exception thrown while reading data from GTFO.AWO: {e}");
                }
            }
        }
    }
}
