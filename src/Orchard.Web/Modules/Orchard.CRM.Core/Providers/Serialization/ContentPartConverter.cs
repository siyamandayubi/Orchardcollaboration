using Newtonsoft.Json;
using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ContentPartConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ContentPart contentPart = value as ContentPart;
            this.WriteCommonFields(writer, contentPart, serializer);

            writer.WriteEnd();
        }

        protected virtual void WriteCommonFields(JsonWriter writer, ContentPart contentPart, JsonSerializer serializer)
        {
            // PartDefinition
            Utility.WriteProperty("PartDefinition", contentPart.PartDefinition, writer, serializer);

            // Id
            Utility.WriteProperty("Id", contentPart.Id, writer, serializer);

            // TypePartDefinition
            Utility.WriteProperty("TypePartDefinition", contentPart.TypePartDefinition, writer, serializer);

            // TypeDefinition
            Utility.WriteProperty("TypeDefinition", contentPart.TypeDefinition, writer, serializer);

            // Fields
            Utility.WriteArray("Fields", contentPart.Fields, writer, serializer);
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
            if (JsonConvertersTypes.TypesHavingJsonConverters.Any(c => c == objectType))
            {
                return false;
            }

            return objectType.IsSubclassOf(typeof(ContentPart));
        }
    }
}