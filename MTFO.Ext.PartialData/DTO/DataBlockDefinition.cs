namespace MTFO.Ext.PartialData.DTO
{
    public class DataBlockDefinition
    {
        public string TypeName { get; set; } = "LevelLayoutDataBlock";
        public string FileSearchPattern { get; set; } = "layout.*.json";
        public bool CheckSubDirectory { get; set; } = false;
        public GUIDMapperConfig GuidConfig { get; set; } = new GUIDMapperConfig();
    }

    public class GUIDMapperConfig
    {
        public uint StartFromID { get; set; } = uint.MaxValue;
        public MapperIncrementMode IncrementMode { get; set; } = MapperIncrementMode.Decrement;
    }

    public enum MapperIncrementMode
    {
        Decrement, Increment
    }
}