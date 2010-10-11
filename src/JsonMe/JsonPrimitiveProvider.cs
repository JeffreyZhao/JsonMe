using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;

namespace JsonMe
{
    internal interface IJsonPrimitiveProvider
    {
        JsonPrimitive ToPrimitive(object value);
        object FromPrimitive(JsonPrimitive value);
    }

    internal class JsonPrimitiveProvider : IJsonPrimitiveProvider
    {
        private Func<object, JsonPrimitive> m_to;
        private Func<JsonPrimitive, object> m_from;

        public JsonPrimitiveProvider(Func<JsonPrimitive, object> from, Func<object, JsonPrimitive> to)
        {
            this.m_from = from;
            this.m_to = to;
        }

        public JsonPrimitive ToPrimitive(object value)
        {
            return this.m_to(value);
        }

        public object FromPrimitive(JsonPrimitive value)
        {
            return this.m_from(value);
        }
    }
}
