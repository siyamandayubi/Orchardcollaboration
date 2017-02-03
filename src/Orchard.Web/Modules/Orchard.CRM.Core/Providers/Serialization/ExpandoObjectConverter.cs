using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ExpandoObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            IDictionary<string, object> dictionary = value as IDictionary<string, object>;

            foreach (var property in dictionary)
            {
                if (property.Value is IEnumerable && !(property.Value is string))
                {
                    Utility.WriteArray(property.Key, property.Value as IEnumerable, writer, serializer);
                }
                else
                {
                    Utility.WriteProperty(property.Key, property.Value, writer, serializer);
                }
            }

            writer.WriteEnd();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ExpandoObject);
        }
    }
}