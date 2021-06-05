using MTFO.Ext.PartialData.DataBlockTypes;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.JsonConverters;
using MTFO.Ext.PartialData.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MTFO.Ext.PartialData
{
    public class PartialDataManager
    {
        public static string PartialDataPath { get; private set; }
        public static string ConfigPath { get; private set; }
        public static bool Initialized { get; private set; } = false;
        public static bool CanLiveEdit { get; set; } = false;
        public static PersistentIDConverter IDConverter { get; private set; } = new PersistentIDConverter();

        private static List<DataBlockDefinition> _Config;
        private static readonly List<string> _AddedFileList = new List<string>();
        private static readonly List<PartialDataCache> _DataCache = new List<PartialDataCache>();

        internal static bool Initialize()
        {
            if (Initialized)
                return false;

            if (!MTFOUtil.IsLoaded)
                return false;

            PartialDataPath = Path.GetFullPath(Path.Combine(MTFOUtil.GameDataPath, "PartialData"));
            if (!Directory.Exists(PartialDataPath))
                return false;

            ConfigPath = Path.GetFullPath(Path.Combine(PartialDataPath, "_config.json"));
            if (!File.Exists(ConfigPath))
                return false;

            _AddedFileList.Clear();
            _DataCache.Clear();
            _Config = JSON.Deserialize<List<DataBlockDefinition>>(File.ReadAllText(ConfigPath));

            Initialized = true;
            ReadAndAssignIDs();
            return true;
        }

        private static void ReadAndAssignIDs()
        {
            foreach (var def in _Config)
            {
                DataBlockTypeManager.SetIDBuffer(def.TypeName, def.StartFromID, def.IncrementMode);
            }

            var files = Directory.GetFiles(PartialDataPath, "*.json", SearchOption.AllDirectories).OrderBy(f => f);
            foreach (var file in files)
            {
                if (Path.GetFileName(file).StartsWith("_"))
                {
                    Logger.Log($"{file} have discard prefix (_) excluding from loader!");
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

                AssignPersistentID(file);

                Logger.Log($" - {file}");
            }
        }

        private static void AssignPersistentID(string file)
        {
            var json = File.ReadAllText(file);
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

            var root = doc.RootElement;
            switch (root.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var element in root.EnumerateArray())
                    {
                        Read(element, true, file);
                    }
                    break;

                case JsonValueKind.Object:
                    Read(root, true, file);
                    break;
            }
        }

        private static void ReadChangedFile(string file)
        {
            var json = File.ReadAllText(file);
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

            var root = doc.RootElement;
            switch (root.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var element in root.EnumerateArray())
                    {
                        Read(element, false, file);
                    }
                    break;

                case JsonValueKind.Object:
                    Read(root, false, file);
                    break;
            }
        }

        private static void Read(JsonElement objNode, bool assignID, string debugName)
        {
            if (!objNode.TryGetProperty("persistentID", out var idNode))
            {
                Logger.Error($"persistentID field is missing: {debugName}");
                return;
            }

            if (!objNode.TryGetProperty("datablock", out var dbNode))
            {
                Logger.Error($"datablock field is missing: {debugName}");
                return;
            }

            if (assignID)
            {
                if (idNode.ValueKind == JsonValueKind.String)
                {
                    if (!DataBlockTypeManager.TryGetNextID(dbNode.GetString(), out var id))
                    {
                        Logger.Error($"datablock field is not valid: {debugName} {objNode}");
                        return;
                    }
                    PersistentIDManager.TryAssignId(idNode.GetString(), id);
                }
            }

            var datablockName = DataBlockTypeManager.GetBlockName(dbNode.GetString());
            if (!DataBlockTypeManager.TryFindCache(datablockName, out var cache))
            {
                Logger.Error($"datablock field is not valid: {debugName} {objNode}");
                return;
            }

            PartialDataCache partialDataCache = _DataCache.FirstOrDefault(x => x.Name.Equals(datablockName));
            if (partialDataCache == null)
            {
                partialDataCache = new PartialDataCache(cache);
                _DataCache.Add(partialDataCache);
            }

            partialDataCache.JsonsToRead.Enqueue(objNode.ToString());
        }

        internal static void LoadPartialData()
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
                    Filter = "*.json"
                };
                watcher.Changed += new FileSystemEventHandler(OnDataBlockChanged);
                watcher.EnableRaisingEvents = true;
            }

            AddAllCache();
        }

        internal static void WriteAllFile(string path)
        {
            if (!Directory.Exists(path))
                return;

            foreach (var cache in _DataCache)
            {
                var file = Path.Combine(path, "GameData_" + cache.DataBlockType.GetFullName() + "_bin.json");
                var fileFullPath = Path.GetFullPath(file);
                cache.DataBlockType.DoSaveToDisk(fileFullPath);
            }
        }

        private static void OnDataBlockChanged(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetFileName(e.Name).StartsWith("_"))
            {
                Logger.Warning($"LiveEdit File Changed: {e.Name}");
                ReadChangedFile(e.Name);
                AddAllCache(true);
            }
        }

        private static void AddAllCache(bool isLiveEdit = false)
        {
            foreach (var cache in _DataCache)
            {
                if (isLiveEdit && cache.JsonsToRead.Count > 0 && cache.Name.Equals("Rundown"))
                {
                    cache.JsonsToRead.Clear(); //TODO: Someday, Fix it
                    Logger.Error("Editing Rundown DataBlock will leads to crash, Ignored");
                    continue;
                }

                bool isChanged = false;
                while (cache.JsonsToRead.Count > 0)
                {
                    var json = cache.JsonsToRead.Dequeue();
                    cache.DataBlockType.AddJsonBlock(json);
                    isChanged = true;
                }

                if (isChanged && isLiveEdit)
                {
                    cache.DataBlockType.OnChanged();
                }
            }
        }

        public static uint GetID(string guid)
        {
            if (!Initialized)
                return 0;

            return PersistentIDManager.GetId(guid);
        }
    }
}