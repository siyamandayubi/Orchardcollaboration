using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class CommonPartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            CommonPart contentPart = value as CommonPart;

            // CreatedUtc
            Utility.WriteProperty("CreatedUtc", contentPart.CreatedUtc, writer, serializer);

            // PublishedUtc
            Utility.WriteProperty("PublishedUtc", contentPart.PublishedUtc, writer, serializer);

            // ModifiedUtc
            Utility.WriteProperty("ModifiedUtc", contentPart.ModifiedUtc, writer, serializer);

            this.WriteCommonFields(writer, contentPart, serializer);

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
            return objectType == typeof(CommonPart);
        }
    }
}