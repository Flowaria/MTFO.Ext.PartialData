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
    [BepInPlugin("MTFO.Extension.PartialBlocks", "MTFO pDataBlock", "1.4.1")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.MTFOGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
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