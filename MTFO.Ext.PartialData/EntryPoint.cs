using AssetShards;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameData;
using HarmonyLib;
using Localization;
using MTFO.Ext.PartialData.DataBlockTypes;
using MTFO.Ext.PartialData.Utils;
using System.IO;

namespace MTFO.Ext.PartialData
{
    [BepInPlugin("MTFO.Extension.PartialBlocks", "MTFO pDataBlock", "1.5.1")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.MTFOGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(InjectLibUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    internal class EntryPoint : BasePlugin
    {
        public static bool LogAddBlock = false;
        public static bool LogEditBlock = true;
        public static bool LogOfflineGearLink = false;
        public static bool LogInjectLibLink = false;
        public static bool LogDebugs = false;

        public override void Load()
        {
            Logger.LogInstance = Log;

            InjectLibUtil.Setup();
            MTFOUtil.Setup();

            LogDebugs = Config.Bind(new ConfigDefinition("Logging", "Log Debug Messages"), false, new ConfigDescription("Using Debug Log Messages?")).Value;
            LogAddBlock = Config.Bind(new ConfigDefinition("Logging", "Log AddBlock"), false, new ConfigDescription("Using Log Message for AddBlock?")).Value;
            LogEditBlock = Config.Bind(new ConfigDefinition("Logging", "Log EditBlock"), true, new ConfigDescription("Using Log Message for Editing Block (Mostly by LiveEdit)?")).Value;
            LogOfflineGearLink = Config.Bind(new ConfigDefinition("Logging", "Log OfflineGear Links"), false, new ConfigDescription("Using Log Message for Linking GUID for OfflineGearJSON?")).Value;
            LogInjectLibLink = Config.Bind(new ConfigDefinition("Logging", "Log InjectLib Links"), false, new ConfigDescription("Using Log Message for Linking GUID for InjectLib?")).Value;

            var useLiveEdit = Config.Bind(new ConfigDefinition("Developer", "UseLiveEdit"), false, new ConfigDescription("Using Live Edit?"));
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

            var gdLocalization = Text.TextLocalizationService.Cast<GameDataTextLocalizationService>();
            gdLocalization.m_textDataBlocks = null;
            gdLocalization.m_texts.Clear();
            //gdLocalization.

            var currentLanguage = Text.TextLocalizationService.CurrentLanguage;

            TextDataBlock[] allBlocks = GameDataBlockBase<TextDataBlock>.GetAllBlocks();
            gdLocalization.m_textDataBlocks = allBlocks;
            int num = allBlocks.Length;
            for (int i = 0; i < num; i++)
            {
                TextDataBlock textDataBlock = allBlocks[i];
                var text = textDataBlock.GetText(currentLanguage, false);
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = textDataBlock.English;
                }
                gdLocalization.m_texts[textDataBlock.persistentID] = text;
            }

            Text.TextLocalizationService.SetCurrentLanguage(Text.TextLocalizationService.CurrentLanguage); //Update the TextDataBlock
            Text.UpdateAllTexts();
            PartialDataManager.WriteAllFile(Path.Combine(MTFOUtil.GameDataPath, "CompiledPartialData"));
        }
    }
}