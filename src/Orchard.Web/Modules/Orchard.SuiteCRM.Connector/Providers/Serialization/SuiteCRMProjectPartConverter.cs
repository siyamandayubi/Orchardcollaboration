using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector.Providers.Serialization
{
    public class SuiteCRMProjectPartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            SuiteCRMProjectPart contentPart = value as SuiteCRMProjectPart;

            Action<string, string, JsonWriter, JsonSerializer> propertyWriter = Orchard.CRM.Core.Providers.Serialization.Utility.WriteProperty;

            // ExternalId
            propertyWriter("ExternalId", contentPart.ExternalId, writer, serializer);

            // LastSyncTime
            propertyWriter("LastSyncTime", contentPart.LastSyncTime.ToString(), writer, serializer);
            
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
            return objectType == typeof(SuiteCRMProjectPart);
        }
    }
}