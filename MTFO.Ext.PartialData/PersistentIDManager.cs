using System.Collections.Generic;

namespace MTFO.Ext.PartialData
{
    public static class PersistentIDManager
    {
        private static Dictionary<string, uint> _GUIDDict = new Dictionary<string, uint>();

        public static bool TryAssignId(string guid, uint id)
        {
            if (_GUIDDict.ContainsKey(guid))
            {
                Logger.Error($"GUID is already used: {guid}");
                return false;
            }

            _GUIDDict.Add(guid, id);
            return true;
        }

        public static uint GetId(string guid)
        {
            if (!_GUIDDict.TryGetValue(guid, out var id))
            {
                Logger.Error($"GUID is Missing: {guid}");
                return 0;
            }
            return id;
        }
    }
}