using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class TitlePartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            TitlePart contentPart = value as TitlePart;

            // Title
            Utility.WriteProperty("Title", contentPart.Title, writer, serializer);

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
            return objectType == typeof(TitlePart);
        }
    }
}