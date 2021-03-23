using MTFO.Ext.PartialData.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace MTFO.Ext.PartialData
{
    public class DataBlockTypeCache
    {
        private static JsonConverter[] CommonConverters;

        static DataBlockTypeCache()
        {
            CommonConverters = new JsonConverter[]
            {
                new Il2CppListReadOnlyConverter(),
                new ColorConverter(),
                new Vector2Converter(),
                new Vector3Converter()
            };
        }

        public string TypeName;
        public Type SerializeType;
        public MethodInfo AddBlockMethod;

        public void AddBlock(object block)
        {
            AddBlockMethod.Invoke(null, new object[] { block, -1 });

            dynamic blockDyn = block;
            Logger.Log($"Added Block: {blockDyn.persistentID}, {blockDyn.name}");
        }

        public object Deserialize(string content, params JsonConverter[] converters)
        {
            SerializeType.MakeArrayType();

            var newArray = CommonConverters.Concat(converters);
            return JsonConvert.DeserializeObject(content, SerializeType, newArray.ToArray());
        }

        public void AddJsonBlock(string json, params JsonConverter[] converters)
        {
            try
            {
                var convertersArray = CommonConverters.Concat(converters).ToArray();
                var jObj = JToken.Parse(json);
                if (jObj.Type == JTokenType.Array)
                {
                    dynamic blocks = JsonConvert.DeserializeObject(json, SerializeType.MakeArrayType(), convertersArray);
                    foreach (var block in blocks)
                    {
                        AddBlock(block);
                    }
                }
                else if (jObj.Type == JTokenType.Object)
                {
                    var block = JsonConvert.DeserializeObject(json, SerializeType, convertersArray);
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