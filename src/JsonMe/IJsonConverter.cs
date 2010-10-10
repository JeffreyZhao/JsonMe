using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonMe
{
    public interface IJsonConverter
    {
        object ToJsonValue(Type type, object value);

        object FromJsonValue(Type type, object value);
    }
}
