using MTFO.Ext.PartialData.Utils;

namespace MTFO.Ext.PartialData.DTO
{
    internal class DataBlockDefinition
    {
        public string TypeName { get; set; } = "DataBlock?";
        public uint StartFromID { get; set; } = ushort.MaxValue;
        public IncrementMode IncrementMode { get; set; } = IncrementMode.Decrement;
    }
}