using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;
using System.Collections;

namespace JsonMe
{
    internal static class JsonUtils
    {
        private static Dictionary<Type, JsonPrimitiveProvider> s_primitiveProviders =
            new Dictionary<Type, JsonPrimitiveProvider>
            {
                { 
                    typeof(int),
                    new JsonPrimitiveProvider(p => (int)p, o => new JsonPrimitive((int)o))
                },
                { 
                    typeof(long),
                    new JsonPrimitiveProvider(p => (long)p, o => new JsonPrimitive((long)o))
                },
                { 
                    typeof(float),
                    new JsonPrimitiveProvider(p => (float)p, o => new JsonPrimitive((float)o))
                },
                { 
                    typeof(double),
                    new JsonPrimitiveProvider(p => (double)p, o => new JsonPrimitive((double)o))
                },
                { 
                    typeof(string),
                    new JsonPrimitiveProvider(p => (string)p, o => new JsonPrimitive((string)o))
                },
                { 
                    typeof(bool),
                    new JsonPrimitiveProvider(p => (bool)p, o => new JsonPrimitive((bool)o))
                },
            };

        public static bool IsPrimitive(object value)
        {
            if (value == null) return true;

            return s_primitiveProviders.ContainsKey(value.GetType());
        }

        public static bool ShouldSerialize(object value)
        {
            if (IsPrimitive(value)) return false;

            return !(value is JsonValue);
        }

        public static JsonValue ToJson(object value)
        {
            if (value == null) return null;

            var jsonValue = value as JsonValue;
            if (jsonValue != null) return jsonValue;

            JsonPrimitiveProvider provider;
            if (s_primitiveProviders.TryGetValue(value.GetType(), out provider))
            {
                return provider.ToPrimitive(value);
            }

            var dict = value as Dictionary<string, object>;
            if (dict != null)
            {
                return new JsonObject(dict.ToDictionary(p => p.Key, p => ToJson(p.Value)));
            }

            var array = value as IEnumerable;
            if (array != null)
            {
                return new JsonArray(array.Cast<object>().Select(o => ToJson(o)));
            }

            var jsonObject = new JsonObject();
            foreach (var property in value.GetType().GetProperties().Where(p => !p.IsSpecialName))
            {
                jsonObject.Add(property.Name, ToJson(property.GetValue(value, null)));
            }

            return jsonObject;
        }

        public static JsonValue ToJsonValue(IJsonProperty property, object value)
        {
            try
            {
                if (property.Converter == null)
                {
                    return JsonUtils.ToJson(value);
                }

                return property.Converter.ToJsonValue(property.PropertyInfo.PropertyType, value);
            }
            catch (Exception ex)
            {
                throw new ConversionException(property, value, ex);
            }
        }

        public static object FromJsonValue(IJsonProperty property, JsonValue value)
        {
            var propertyType = property.PropertyInfo.PropertyType;

            if (property.Converter == null)
            {
                if (propertyType.IsAssignableFrom(value.GetType()))
                {
                    return value;
                }

                var primitive = value as JsonPrimitive;
                if (primitive == null)
                {
                    throw new ConversionException(property, value, null);
                }
                else
                {
                    JsonPrimitiveProvider provider;
                    if (s_primitiveProviders.TryGetValue(propertyType, out provider))
                    {
                        return provider.FromPrimitive(primitive);
                    }
                    else
                    {
                        throw new ConversionException(property, value, null);
                    }
                }
            }
            else
            {
                try
                {
                    return property.Converter.FromJsonValue(propertyType, value);
                }
                catch (Exception ex)
                {
                    throw new ConversionException(property, value, ex);
                }
            }
        }
    }
}
