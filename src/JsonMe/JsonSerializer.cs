using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

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

                jsonObject.Add(property.Name, value);
            }

            return jsonObject;
        }

        public static JsonArray SerializeArray<T>(IEnumerable<T> entities, JsonContract<T> contract)
        {
            if (entities == null) return null;

            var jsonEntities = entities.Select(e => SerializeObject(e, contract));
            var jsonArray = new JsonArray();
            jsonArray.AddRange(jsonEntities);

            return jsonArray;
        }
        
        public static string Serialize(object entity)
        {
            return new JavaScriptSerializer().Serialize(entity);
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
                dynamic value;
                if (!jsonObj.TryGetValue(property.Name, out value))
                {
                    throw new KeyNotFoundException(jsonObj, property.Name);
                }

                value = JsonUtils.FromJsonValue(property, value);

                property.PropertyInfo.SetValue(entity, value, null);
            }

            return entity;
        }

        public static List<T> DeserializeArray<T>(JsonArray jsonArray, JsonContract<T> contract)
            where T : class, new()
        {
            return jsonArray.Select(e => DeserializeObject((JsonObject)e, contract)).ToList();
        }

        public static object Deserialize(string jsonString)
        {
            object obj;
            try
            {
                obj = new JavaScriptSerializer().DeserializeObject(jsonString);
            }
            catch (Exception ex)
            {
                throw new JsonFormatException(jsonString, ex);
            }

            return JsonUtils.ToJson(obj);           
        }
    }
}
