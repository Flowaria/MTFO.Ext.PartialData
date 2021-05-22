using MTFO.Ext.PartialData.JsonConverters;
using MTFO.Ext.PartialData.DTO;
using MTFO.Ext.PartialData.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Collections.Generic;
using GameData;
using LevelGeneration;

namespace MTFO.Ext.PartialData
{
    public partial class DataBlockTypeWrapper
    {
        public string TypeName { get; private set; }
        public Type SerializeType { get; private set; }
        public MethodInfo AddBlockMethod { get; private set; }
        public MethodInfo GetBlockMethod { get; private set; }
        public MethodInfo DoSaveToDiskMethod { get; private set; }
        public PropertyInfo FullPathProperty { get; private set; }
        public Action OnForceChange;

        public void ForceApplyChange()
        {
            OnForceChange?.Invoke();
        }

        public void AddBlock(object block)
        {
            dynamic blockDyn = block;
            dynamic extBlockDyn = GetBlockMethod.Invoke(null, new object[] { blockDyn.persistentID });
            if (extBlockDyn != null)
            {
                CopyProperties(blockDyn, extBlockDyn);
                Logger.Warning($"Replaced Block: {blockDyn.persistentID}, {blockDyn.name}");
                return;
            }
            AddBlockMethod.Invoke(null, new object[] { block, -1 });

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
                JSON.IDConverter.IsSaveIDMode = false;

                var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

                switch(doc.RootElement.ValueKind)
                {
                    case JsonValueKind.Array:
                        dynamic blocks = JSON.Deserialize(json, SerializeType.MakeArrayType());
                        foreach (var b in blocks)
                        {
                            AddBlock(b);
                        }
                        break;

                    case JsonValueKind.Object:
                        var block = JSON.Deserialize(json, SerializeType);
                        AddBlock(block);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error While Adding Block: {e}");
            }
        }

        public void WriteAllToFile(string fullPath)
        {
            var oldPath = FullPathProperty.GetValue(null);
            FullPathProperty.SetValue(null, fullPath);

            DoSaveToDiskMethod?.Invoke(null, new object[] { false, false });

            FullPathProperty.SetValue(null, oldPath);
        }

        private static object CopyProperties(object source, object target)
        {
            foreach (var sourceProp in source.GetType().GetProperties())
            {
                bool isMatched = target.GetType().GetProperties().Any(targetProp => targetProp.Name == sourceProp.Name && targetProp.GetType() == sourceProp.GetType() && targetProp.CanWrite);
                if (isMatched)
                {
                    var value = sourceProp.GetValue(source);
                    PropertyInfo propertyInfo = target.GetType().GetProperty(sourceProp.Name);
                    propertyInfo.SetValue(target, value);
                }
            }
            return target;
        }
    }
}