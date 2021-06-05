using BepInEx.IL2CPP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonNetIntegration
{
    public delegate uint GetIDDelegate(string guid);

    public static class PartialDataUtil
    {
        public const string PLUGIN_GUID = "MTFO.Extension.PartialBlocks";

        public static bool IsLoaded { get; private set; } = false;
        public static bool Initialized { get; private set; } = false;
        public static string PartialDataPath { get; private set; } = string.Empty;
        public static string ConfigPath { get; private set; } = string.Empty;
        public static GetIDDelegate GetIDCall { get; private set; } = null;

        static PartialDataUtil()
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
            {
                try
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var ddAsm = assemblies.First(a => !a.IsDynamic && a.Location == info.Location);

                    if (ddAsm is null)
                        throw new Exception("Assembly is Missing!");

                    var types = ddAsm.GetTypes();
                    var converterType = types.First(t => t.Name == "PersistentIDConverter");
                    if (converterType is null)
                        throw new Exception("Unable to Find PersistentIDConverter Class");

                    var dataManager = types.First(t => t.Name == "PartialDataManager");
                    if (dataManager is null)
                        throw new Exception("Unable to Find PartialDataManager Class");

                    var initProp = dataManager.GetProperty("Initialized", BindingFlags.Public | BindingFlags.Static);
                    var dataPathProp = dataManager.GetProperty("PartialDataPath", BindingFlags.Public | BindingFlags.Static);
                    var configPathProp = dataManager.GetProperty("ConfigPath", BindingFlags.Public | BindingFlags.Static);
                    var getidMethod = dataManager.GetMethod("GetID", BindingFlags.Public | BindingFlags.Static);

                    if (initProp is null)
                        throw new Exception("Unable to Find Property: Initialized");

                    if (dataPathProp is null)
                        throw new Exception("Unable to Find Property: PartialDataPath");

                    if (configPathProp is null)
                        throw new Exception("Unable to Find Field: ConfigPath");

                    if (getidMethod is null)
                        throw new Exception("Unable to Find Method: GetID");

                    Initialized = (bool)initProp.GetValue(null);
                    PartialDataPath = (string)dataPathProp.GetValue(null);
                    ConfigPath = (string)configPathProp.GetValue(null);
                    GetIDCall = (GetIDDelegate)Delegate.CreateDelegate(typeof(GetIDDelegate), getidMethod);
                    IsLoaded = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception thrown while reading data from MTFO_Extension_PartialData:\n{e}");
                }
            }
        }

        public static uint GetID(string guid)
        {
            if (!IsLoaded)
                return 0;

            if (!Initialized)
                return 0;

            return GetIDCall?.Invoke(guid) ?? 0;
        }
    }
}
