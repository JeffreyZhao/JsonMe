using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace JsonMe
{
    public class JsonObject : Dictionary<string, dynamic>
    {
        public JsonObject() { }

        public JsonObject(Dictionary<string, object> rawValue)
            : base(rawValue.ToDictionary(p => p.Key, p => JsonUtils.ToJson(p.Value)))
        { }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class JsonArray : List<dynamic>
    {
        public JsonArray() { }

        public JsonArray(IEnumerable<object> rawValue)
            : base(rawValue.Select(JsonUtils.ToJson)) { }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
