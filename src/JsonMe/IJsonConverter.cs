using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;

namespace JsonMe
{
    public interface IJsonConverter
    {
        JsonValue ToJsonValue(Type type, object value);

        object FromJsonValue(Type type, JsonValue value);
    }
}
