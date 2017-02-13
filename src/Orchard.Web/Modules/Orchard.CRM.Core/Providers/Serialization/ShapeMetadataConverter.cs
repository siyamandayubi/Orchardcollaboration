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
    public class ShapeMetadataConverter : JsonConverter
    {
        public ShapeMetadataConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ShapeMetadata shapeMetadata = value as ShapeMetadata;

            // Type
            Utility.WriteProperty("Type", shapeMetadata.Type, writer, serializer);

            // DisplayType
            Utility.WriteProperty("DisplayType", shapeMetadata.DisplayType, writer, serializer);

            // Position
            Utility.WriteProperty("Position", shapeMetadata.Position, writer, serializer);

            // Tab
            Utility.WriteProperty("Tab", shapeMetadata.Tab, writer, serializer);

            // PlacementSource
            Utility.WriteProperty("PlacementSource", shapeMetadata.PlacementSource, writer, serializer);

            // Prefix
            Utility.WriteProperty("Prefix", shapeMetadata.Prefix, writer, serializer);

            // Wrappers
            Utility.WriteArray("Wrappers", shapeMetadata.Wrappers, writer, serializer);

            // Alternates
            Utility.WriteArray("Alternates", shapeMetadata.Alternates, writer, serializer);

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
            return objectType == typeof(ShapeMetadata);
        }
    }
}