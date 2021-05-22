using MTFO.Ext.PartialData.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTFO.Ext.PartialData
{
    public class PartialDataFileInfo
    {
        public DataBlockTypeWrapper TypeCache;

        public string TypeName { get; private set; }
        public string[] Files { get; private set; } = new string[0];
        public uint StartID { get; set; }
        public MapperIncrementMode IncrementMode { get; set; }

        private List<string> _Files = new List<string>();

        public PartialDataFileInfo(string typename)
        {
            TypeName = typename;
        }

        public void AddFile(string file)
        {
            if (!_Files.Contains(file))
            {
                _Files.Add(file);
                Files = _Files.ToArray();
            }
        }
    }
}
