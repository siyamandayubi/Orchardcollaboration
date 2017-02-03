using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public static class Utility
    {
        public static void WriteArray(string name, IEnumerable values, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();

            foreach (var item in values)
            {
                if (item == null)
                {
                    writer.WriteNull();
                    continue;
                }

                var type = item.GetType();
                if ((item is IShape || item is ContentItem) || !(type.IsPrimitive || type == typeof(Decimal) || type == typeof(String)))
                {
                    serializer.Serialize(writer, item);
                }
                else
                {
                    writer.WriteValue(item);
                }
            }

            writer.WriteEndArray();
        }

        public static void WriteProperty(string name, object value, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(name);

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();
            if ((value is IShape || value is ContentItem) || !(type.IsPrimitive || type == typeof(Decimal) || type == typeof(String)))
            {
                serializer.Serialize(writer, value);
            }
            else
            {
                writer.WriteValue(value);
            }
        }
    }
}