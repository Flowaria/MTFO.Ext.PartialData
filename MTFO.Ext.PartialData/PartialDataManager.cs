using MTFO.Ext.PartialData.JsonConverters;
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
        public static string ConfigPath { get; private set; }
        public static bool Initialized { get; private set; } = false;
        public static bool CanLiveEdit { get; set; } = true;

        private static List<DataBlockDefinition> _Config;
        private static readonly List<string> _AddedFileList = new List<string>();
        private static readonly List<PartialDataFileInfo> _BlockFileInfos = new List<PartialDataFileInfo>();
        
        public static bool Initialize()
        {
            if (Initialized)
                return false;

            if (!MTFOUtil.IsLoaded)
                return false;

            PartialDataPath = Path.GetFullPath(Path.Combine(MTFOUtil.GameDataPath, "PartialData"));
            if (!Directory.Exists(PartialDataPath))
                return false;

            ConfigPath = Path.GetFullPath(Path.Combine(PartialDataPath, "config.json"));
            if (!File.Exists(ConfigPath))
                return false;

            _AddedFileList.Clear();
            _BlockFileInfos.Clear();
            _Config = JSON.Deserialize<List<DataBlockDefinition>>(File.ReadAllText(ConfigPath));

            Initialized = true;
            ReadAndAssignIDs();
            return true;
        }

        private static void ReadAndAssignIDs()
        {
            foreach (var def in _Config)
            {
                
                var fileInfo = new PartialDataFileInfo(def.TypeName);
                fileInfo.StartID = def.GuidConfig.StartFromID;
                fileInfo.IncrementMode = def.GuidConfig.IncrementMode;

                var idBuffer = def.GuidConfig.StartFromID;
                JSON.IDConverter.SaveID = (string guid) =>
                {
                    if (PersistentIDManager.TryAssignId(guid, idBuffer))
                    {
                        if (def.GuidConfig.IncrementMode == MapperIncrementMode.Decrement)
                            return idBuffer--;
                        else
                            return idBuffer++;
                    }
                    return 0;
                };

                foreach (var searchConfig in def.SearchConfigs)
                {
                    var searchOption = searchConfig.CheckSubDirectory ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var searchPath = Path.Combine(PartialDataPath, searchConfig.BaseSubDirectory);
                    var files = Directory.GetFiles(searchPath, searchConfig.FileSearchPattern, searchOption).OrderBy(f => f);
                    foreach (var file in files)
                    {
                        if (ConfigPath.Equals(Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Error($"config.json is not allowed to be loaded as datablock!");
                            continue;
                        }

                        if (!File.Exists(file))
                        {
                            Logger.Error($"File ({file}) is not exist somehow?");
                            continue;
                        }

                        if (_AddedFileList.Contains(file))
                        {
                            Logger.Error($"File ({file}) has loaded multiple times!");
                            continue;
                        }

                        _AddedFileList.Add(file);
                        fileInfo.AddFile(file);
                        AssignPersistentID(file);

                        Logger.Log($" - {file}");
                    }
                }

                _BlockFileInfos.Add(fileInfo);
            }
        }

        private static void AssignPersistentID(string file)
        {
            var json = File.ReadAllText(file);
            var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

            JSON.IDConverter.IsSaveIDMode = true;
            switch (doc.RootElement.ValueKind)
            {
                case JsonValueKind.Array:
                    _ = JSON.Deserialize<GameDataDTO[]>(json);
                    break;

                case JsonValueKind.Object:
                    _ = JSON.Deserialize<GameDataDTO>(json);
                    break;
            }
        }

        public static void LoadPartialData()
        {
            ValidateDataBlockTypeName();
            ReadPartialData();
        }

        private static void ValidateDataBlockTypeName()
        {
            foreach (var info in _BlockFileInfos)
            {
                if (!DataBlockTypeLoader.TryFindCache(info.TypeName, out info.TypeCache))
                {
                    Logger.Warning($"TypeName: {info.TypeName} is not valid DataBlockType!");
                    _BlockFileInfos.Remove(info);
                }
            }
        }

        private static void ReadPartialData()
        {
            if (!Initialized)
                return;

            if (CanLiveEdit)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = PartialDataPath,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*"
                };
                watcher.Changed += new FileSystemEventHandler(OnDataBlockChanged);
                watcher.EnableRaisingEvents = true;
            }

            JSON.IDConverter.LoadID = (string guid) =>
            {
                return PersistentIDManager.GetId(guid);
            };

            foreach (var info in _BlockFileInfos)
            {
                uint idBuffer = info.StartID;
                

                foreach (var file in info.Files)
                {
                    if (!File.Exists(file))
                        continue;

                    var content = File.ReadAllText(file);
                    info.TypeCache.AddJsonBlock(content);
                }
            }
        }

        private static void OnDataBlockChanged(object sender, FileSystemEventArgs e)
        {
            foreach(var info in _BlockFileInfos)
            {
                if (info.Files.Any(f=>f.Equals(e.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    info.TypeCache.AddJsonBlock(File.ReadAllText(e.Name));
                    info.TypeCache.ForceApplyChange();
                    Logger.Log($"DataBlock has edited!: {e.Name}");
                }
            }
        }
    }
}
