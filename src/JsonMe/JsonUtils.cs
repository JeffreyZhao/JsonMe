using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonMe
{
    internal static class JsonUtils
    {
        public static bool IsPrimitive(object value)
        {
            if (value == null) return false;

            var type = value.GetType();
            return 
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(string) ||
                type == typeof(bool);
        }

        public static bool ShouldSerialize(object value)
        {
            if (IsPrimitive(value)) return false;

            var type = value.GetType();
            var isBuiltIn =
                type == typeof(JsonArray) ||
                type == typeof(JsonObject);

            return !isBuiltIn;
        }

        public static object ToJson(object value)
        {
            if (IsPrimitive(value))
            {
                return value;
            }

            var jsonObj = value as Dictionary<string, object>;
            if (jsonObj != null)
            {
                return new JsonObject(jsonObj);
            }

            var jsonArray = value as object[];
            if (jsonArray != null)
            {
                return new JsonArray(jsonArray);
            }

            throw new Exception("Unrecognized type: " + value.GetType());
        }

        public static object ToJsonValue(IJsonProperty property, object value)
        {
            if (property.Converter == null) return value;

            try
            {
                return property.Converter.ToJsonValue(property.PropertyInfo.PropertyType, value);
            }
            catch (Exception ex)
            {
                return new ConversionException(property, value, ex);
            }
        }

        public static object FromJsonValue(IJsonProperty property, object value)
        {
            if (property.Converter == null) return value;

            try
            {
                return property.Converter.FromJsonValue(property.PropertyInfo.PropertyType, value);
            }
            catch (Exception ex)
            {
                return new ConversionException(property, value, ex);
            }
        }
    }
}
