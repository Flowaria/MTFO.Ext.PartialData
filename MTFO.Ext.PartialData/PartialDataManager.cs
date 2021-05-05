using MTFO.Ext.PartialData.Converters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MTFO.Ext.PartialData
{
    public class PartialDataManager
    {
        public static string PartialDataPath { get; private set; }

        private static readonly List<string> _AddedFileList = new List<string>();
        private static readonly Dictionary<string, uint> _GUIDDict = new Dictionary<string, uint>();

        public static void UpdatePartialData()
        {
            if (!MTFOUtil.IsLoaded)
                return;

            Logger.Log("MTFO is ON");
            PartialDataPath = Path.Combine(MTFOUtil.GameDataPath, "PartialData");
            if (!Directory.Exists(PartialDataPath))
                return;

            Logger.Log($"HAS DIR: {PartialDataPath}");
            var configPath = Path.Combine(PartialDataPath, "config.json");
            if (!File.Exists(configPath))
                return;

            _AddedFileList.Clear();

            var configs = JSON.Deserialize<List<DataBlockDefinition>>(File.ReadAllText(configPath));
            foreach (var def in configs)
            {
                if (!DataBlockTypeLoader.TryFindCache(def.TypeName, out var cache))
                    continue;

                var idBuffer = def.GuidConfig.StartFromID;
                JSON.IDConverter.SaveID = (string guid) =>
                {
                    if (TryAssignID(guid, idBuffer))
                    {
                        if (def.GuidConfig.IncrementMode == MapperIncrementMode.Decrement)
                            return idBuffer--;
                        else
                            return idBuffer++;
                    }
                    return 0;
                };
                JSON.IDConverter.LoadID = (string guid) =>
                {
                    return GetID(guid);
                };

                Logger.Log($"Found Type Cache from TypeName!: {def.TypeName}");
                Logger.Log($"Found Those files with filters:");

                foreach (var searchConfig in def.SearchConfigs)
                {
                    var searchOption = searchConfig.CheckSubDirectory ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var searchPath = Path.Combine(PartialDataPath, searchConfig.BaseSubDirectory);
                    var files = Directory.GetFiles(searchPath, searchConfig.FileSearchPattern, searchOption).OrderBy(f => f);
                    foreach (var file in files)
                    {
                        if (_AddedFileList.Contains(file))
                        {
                            Logger.Error($"File ({file}) has loaded multiple times!");
                            continue;
                        }

                        Logger.Log($" - {file}");
                        cache.AddJsonBlock(File.ReadAllText(file));
                        _AddedFileList.Add(file);
                    }
                }
            }
        }

        private static bool TryAssignID(string guid, uint id)
        {
            if (_GUIDDict.ContainsKey(guid))
            {
                Logger.Error($"GUID is already used: {guid}");
                return false;
            }

            _GUIDDict.Add(guid, id);
            return true;
        }

        private static uint GetID(string guid)
        {
            if (!_GUIDDict.TryGetValue(guid, out var id))
            {
                Logger.Error($"GUID is Missing: {guid}");
                return 0;
            }
            return id;
        }

        public static void WriteToFile(string path)
        {
            var text = "[\n";
            foreach(var pair in _GUIDDict)
            {
                text += "\t{ \"GUID\": \"" + pair.Key + "\", \"ID\": \"" + pair.Value + "\"},\n";
            }

            if(text.Length > 2)
            {
                text = text[0..^2];
            }
            text += "\n]";
            File.WriteAllText(path, text);
        }
    }
}
