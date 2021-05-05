using MTFO.Ext.PartialData.Converters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace MTFO.Ext.PartialData
{
    public class DataBlockTypeCache
    {
        public string TypeName;
        public Type SerializeType;
        public MethodInfo AddBlockMethod;

        public void AddBlock(object block)
        {
            AddBlockMethod.Invoke(null, new object[] { block, -1 });

            dynamic blockDyn = block;
            Logger.Log($"Added Block: {blockDyn.persistentID}, {blockDyn.name}");
        }

        public object Deserialize(string content)
        {
            SerializeType.MakeArrayType();
            return JSON.Deserialize(content, SerializeType);
        }

        public void AddJsonBlock(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                var isArray = doc.RootElement.ValueKind == JsonValueKind.Array;
                var isObject = doc.RootElement.ValueKind == JsonValueKind.Object;
                if (isArray)
                {
                    JSON.IDConverter.IsSaveIDMode = true;
                    _ = JSON.Deserialize<GameDataDTO[]>(json);

                    JSON.IDConverter.IsSaveIDMode = false;
                    dynamic blocks = JSON.Deserialize(json, SerializeType.MakeArrayType());
                    foreach (var block in blocks)
                    {
                        AddBlock(block);
                    }
                }
                else if (isObject)
                {
                    JSON.IDConverter.IsSaveIDMode = true;
                    _ = JSON.Deserialize<GameDataDTO>(json);

                    JSON.IDConverter.IsSaveIDMode = false;
                    var block = JSON.Deserialize(json, SerializeType);
                    AddBlock(block);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error While Adding Block: {e}");
            }
        }
    }
}