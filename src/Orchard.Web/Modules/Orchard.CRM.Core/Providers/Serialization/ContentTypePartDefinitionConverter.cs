using Newtonsoft.Json;
using Orchard.ContentManagement.MetaData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ContentTypePartDefinitionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ContentTypePartDefinition contentTypePartDefinition = value as ContentTypePartDefinition;
            if (contentTypePartDefinition.ContentTypeDefinition != null)
            {
                // PartDefinition
                Utility.WriteProperty("PartDefinition", contentTypePartDefinition.PartDefinition, writer, serializer);

                // Settings
                Utility.WriteProperty("Settings", contentTypePartDefinition.Settings, writer, serializer);
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
            return objectType == typeof(ContentTypePartDefinition);
        }
    }
}