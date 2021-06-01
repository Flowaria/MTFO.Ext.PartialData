using MTFO.Ext.PartialData.DataBlockTypes;
using System.Collections.Generic;

namespace MTFO.Ext.PartialData
{
    internal class PartialDataCache
    {
        public string Name
        {
            get
            {
                return DataBlockType.GetShortName();
            }
        }

        public IDataBlockType DataBlockType { get; private set; }
        public Queue<string> JsonsToRead { get; private set; } = new Queue<string>();

        private PartialDataCache()
        {
        }

        public PartialDataCache(IDataBlockType dbTypeCache)
        {
            DataBlockType = dbTypeCache;
        }
    }
}