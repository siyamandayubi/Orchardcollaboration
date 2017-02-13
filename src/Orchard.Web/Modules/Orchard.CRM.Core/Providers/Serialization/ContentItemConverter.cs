using Newtonsoft.Json;
using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ContentItemConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ContentItem contentItem = value as ContentItem;
            
            // Version
            Utility.WriteProperty("Version", contentItem.Version, writer, serializer);
            
            // Id
            Utility.WriteProperty("Id", contentItem.Id, writer, serializer);

            // ContentType
            Utility.WriteProperty("ContentType", contentItem.ContentType, writer, serializer);

            // TypeDefinition
            Utility.WriteProperty("TypeDefinition", contentItem.TypeDefinition, writer, serializer);

            // Parts
            Utility.WriteArray("Parts", contentItem.Parts, writer, serializer);

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
            return objectType == typeof(ContentItem);
        }
    }
}