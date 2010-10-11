using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;

namespace JsonMe
{
    public static class JsonSerializer
    {
        public static JsonObject SerializeObject<T>(T entity, JsonContract<T> contract)
        {
            if (entity == null) return null;

            var jsonObject = new JsonObject();

            foreach (var property in contract.Properties)
            {
                var value = property.PropertyInfo.GetValue(entity, null);
                value = JsonUtils.ToJsonValue(property, value);

                if (JsonUtils.ShouldSerialize(value))
                {
                    throw new ContractMissingException(value);
                }

                jsonObject.Add(property.Name, JsonUtils.ToJson(value));
            }

            return jsonObject;
        }

        public static JsonArray SerializeArray<T>(IEnumerable<T> entities, JsonContract<T> contract)
        {
            if (entities == null) return null;

            
            var jsonEntities = entities.Select(e => SerializeObject(e, contract));
            return new JsonArray(jsonEntities.Cast<JsonValue>());
        }

        public static T DeserializeObject<T>(string jsonString, JsonContract<T> contract)
            where T : class, new()
        {
            var jsonObj = (JsonObject)Deserialize(jsonString);
            return DeserializeObject(jsonObj, contract);
        }

        public static List<T> DeserializeArray<T>(string jsonString, JsonContract<T> contract)
            where T : class, new()
        {
            var jsonArray = (JsonArray)Deserialize(jsonString);
            return DeserializeArray(jsonArray, contract);
        }

        public static T DeserializeObject<T>(JsonObject jsonObj, JsonContract<T> contract)
            where T : class, new()
        {
            if (jsonObj == null) return null;

            var entity = new T();

            foreach (var property in contract.Properties)
            {
                JsonValue jsonValue;
                if (!jsonObj.TryGetValue(property.Name, out jsonValue))
                {
                    throw new KeyNotFoundException(jsonObj, property.Name);
                }

                var value = JsonUtils.FromJsonValue(property, jsonValue);

                property.PropertyInfo.SetValue(entity, value, null);
            }

            return entity;
        }

        public static List<T> DeserializeArray<T>(JsonArray jsonArray, JsonContract<T> contract)
            where T : class, new()
        {
            return jsonArray.Select(e => DeserializeObject((JsonObject)e, contract)).ToList();
        }

        public static JsonValue Deserialize(string jsonString)
        {
            try
            {
                return JsonValue.Parse(jsonString);
            }
            catch (Exception ex)
            {
                throw new JsonFormatException(jsonString, ex);
            }
        }

        public static JsonValue Serialize(object entity)
        {
            return JsonUtils.ToJson(entity);
        }
    }
}
