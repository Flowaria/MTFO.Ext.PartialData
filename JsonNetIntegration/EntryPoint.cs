using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System.IO;

namespace JsonNetIntegration
{
    [BepInPlugin("MTFO.Extension.PartialBlocks.JsonNetTest", "MTFO pDataBlock Json. Net Test", "1.0.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("MTFO.Extension.PartialBlocks", BepInDependency.DependencyFlags.SoftDependency)]
    internal class EntryPoint : BasePlugin
    {
        internal static ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;

            var json = "{\"TargetID\": \"Gather_PDEC\", \"TargetID2\": 55}";
            Log.LogMessage($"Deserializing Json: {json}");

            var obj = JsonConvert.DeserializeObject<ExampleDTO>(json, new IDReadOnlyConverter());
            Log.LogMessage($"TargetID value is: {obj.TargetID}");
            Log.LogMessage($"TargetID2 value is: {obj.TargetID2}");
        }
    }

    internal class ExampleDTO
    {
        public uint TargetID { get; set; }
        public uint TargetID2 { get; set; }
    }
}