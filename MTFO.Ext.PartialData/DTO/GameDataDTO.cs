using System;
using System.Collections.Generic;
using System.Text;

namespace MTFO.Ext.PartialData.DTO
{
    public class GameDataDTO
    {
        public uint persistentID { get; set; } = 0;
        public string datablock { get; set; } = string.Empty; //TODO: This is gonna be pain
    }
}
