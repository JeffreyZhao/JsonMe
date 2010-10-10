using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace JsonMe
{
    public abstract class JsonPropertyBase<TSelf> : IJsonProperty
        where TSelf : JsonPropertyBase<TSelf>
    {
        public JsonPropertyBase(PropertyInfo propertyInfo)
        {
            this.m_propertyInfo = propertyInfo;
            this.m_name = propertyInfo.Name;
            this.m_converter = null;
        }

        private PropertyInfo m_propertyInfo;
        private string m_name;
        private IJsonConverter m_converter;

        public TSelf Name(string name)
        {
            this.m_name = name;
            return (TSelf)this;
        }

        public TSelf Converter(IJsonConverter converter)
        {
            this.m_converter = converter;
            return (TSelf)this;
        }

        PropertyInfo IJsonProperty.PropertyInfo { get { return this.m_propertyInfo; } }

        string IJsonProperty.Name { get { return this.m_name; } }

        IJsonConverter IJsonProperty.Converter { get { return this.m_converter; } }
    }

    public class JsonSimpleProperty : JsonPropertyBase<JsonSimpleProperty>
    {
        public JsonSimpleProperty(PropertyInfo propertyInfo)
            : base(propertyInfo) { }
    }

    public class JsonComplexProperty<TProperty> : JsonPropertyBase<JsonComplexProperty<TProperty>>
        where TProperty : class, new()
    {
        public JsonComplexProperty(PropertyInfo propertyInfo)
            : base(propertyInfo) { }

        public JsonComplexProperty<TProperty> Contract(JsonContract<TProperty> contract)
        {
            return this.Converter(new ContractConverter(contract));
        }

        private class ContractConverter : IJsonConverter
        {
            public JsonContract<TProperty> Contract { get; private set; }

            public ContractConverter(JsonContract<TProperty> contract)
            {
                this.Contract = contract;
            }

            public object ToJsonValue(Type type, object value)
            {
                return JsonSerializer.SerializeObject<TProperty>((TProperty)value, this.Contract);
            }

            public object FromJsonValue(Type type, object value)
            {
                return JsonSerializer.DeserializeObject<TProperty>((JsonObject)value, this.Contract);
            }
        }
    }

    public class JsonArrayProperty<TElement> : JsonPropertyBase<JsonArrayProperty<TElement>>
        where TElement : class, new()
    {
        public JsonArrayProperty(PropertyInfo propertyInfo)
            : base(propertyInfo) { }

        public JsonArrayProperty<TElement> ElementContract(JsonContract<TElement> contract)
        {
            return this.Converter(new ContractConverter(contract));
        }

        private class ContractConverter : IJsonConverter
        {
            public JsonContract<TElement> Contract { get; private set; }

            public ContractConverter(JsonContract<TElement> contract)
            {
                this.Contract = contract;
            }

            public object ToJsonValue(Type type, object value)
            {
                return JsonSerializer.SerializeArray<TElement>((IEnumerable<TElement>)value, this.Contract);
            }

            public object FromJsonValue(Type type, object value)
            {
                var array = JsonSerializer.DeserializeArray<TElement>((JsonArray)value, this.Contract);
                if (array == null) return null;
                if (type == array.GetType()) return array;

                var list = (ICollection<TElement>)Activator.CreateInstance(type);
                foreach (var item in array) list.Add(item);
                return list;
            }
        }
    }

    public interface IJsonProperty
    {
        PropertyInfo PropertyInfo { get; }

        string Name { get; }

        IJsonConverter Converter { get; }
    }
}
