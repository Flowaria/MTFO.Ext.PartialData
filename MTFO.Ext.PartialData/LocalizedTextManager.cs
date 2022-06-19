using Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTFO.Ext.PartialData
{
    public static class LocalizedTextManager
    {
        public static Dictionary<LocalizedText, (uint, string)> _lookup = new Dictionary<LocalizedText, (uint, string)>();

        public static void Register(LocalizedText localizedText, uint id, string unlocalized)
        {
            _lookup[localizedText] = (id, unlocalized);
        }

        public static void Get(LocalizedText localizedText)
        {

        }
    }
}
