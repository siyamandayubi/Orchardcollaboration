using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.UI.Zones;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ShapeConverter : JsonConverter
    {
        public ShapeConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            Shape shape = value as Shape;
            foreach (DictionaryEntry property in shape.Properties)
            {
                string key = property.Key.ToString();

                if (key == "Parent" || key == "Items" || key == "Source")
                {
                    continue;
                }

                if (property.Value is ExpandoObject)
                {
                    Utility.WriteProperty(key, property.Value, writer, serializer);
                }
                else if(property.Value is JObject)
                {
                    Utility.WriteProperty(key, property.Value, writer, serializer);
                }
                else if (property.Value is IEnumerable)
                {
                    Utility.WriteArray(key, property.Value as IEnumerable, writer, serializer);
                }
                else
                {
                    Utility.WriteProperty(key, property.Value, writer, serializer);
                }
            }

            // Attributes
            Utility.WriteArray("Attributes", shape.Attributes, writer, serializer);

            // Items
            Utility.WriteArray("Items", shape.Items, writer, serializer);

            // Position
            Utility.WriteProperty("Position", shape.Position, writer, serializer);

            // Id
            Utility.WriteProperty("Id", shape.Id, writer, serializer);

            // Classes
            Utility.WriteArray("Classes", shape.Classes, writer, serializer);

            // Metadata
            Utility.WriteProperty("Metadata", shape.Metadata, writer, serializer);

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
            return objectType == typeof(Shape) || objectType == typeof(ZoneHolding);
        }
    }
}