using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace JsonMe
{
    public class JsonSerializationException : Exception
    {
        public JsonSerializationException(string message) : base(message) { }

        public JsonSerializationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class JsonFormatException : JsonSerializationException
    {
        public JsonFormatException(string jsonString, Exception innerException)
            : base("Invalid JSON format.", innerException)
        {
            this.JsonString = jsonString;
        }

        public string JsonString { get; private set; }
    }

    public class KeyNotFoundException : JsonSerializationException
    {
        public KeyNotFoundException(JsonObject jsonObj, string key) :
            base(BuildMessage(jsonObj, key))
        {
            this.Object = jsonObj;
            this.Key = key;
        }

        public JsonObject Object { get; private set; }

        public string Key { get; private set; }

        private static string BuildMessage(JsonObject jsonObj, string key)
        {
            return String.Format(
                "Cannot find the required key '{0}' in {1} ", jsonObj, key);
        }
    }

    public class ContractMissingException : JsonSerializationException
    {
        public ContractMissingException(object value) :
            base(BuildMessage(value))
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        private static string BuildMessage(object value)
        {
            return String.Format(
                "Missing the contract when serializing {0}",
                value);
        }
    }

    public class ConversionException : JsonSerializationException
    {
        public IJsonProperty Property { get; private set; }

        public object Value { get; private set; }

        public ConversionException(IJsonProperty property, object value, Exception innerException)
            : base(BuildMessage(property, value), innerException)
        {
            this.Property = property;
            this.Value = value;
        }

        private static string BuildMessage(IJsonProperty property, object value)
        {
            return String.Format(
                "Error occurred when converting value {0} for {1}", value, property);
        }
    }
}
