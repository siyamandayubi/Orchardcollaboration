using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ProjectPartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ProjectPart contentPart = value as ProjectPart;
            var record = contentPart.Record;

            // dynamic  object corresponding to TicketPartRecord 
            dynamic dynamicRecord = new JObject();

            // Basic data
            dynamicRecord.Title = record.Title;
            dynamicRecord.Description = record.Description;
            dynamicRecord.Id = record.Id;

            Utility.WriteProperty("Record", dynamicRecord, writer, serializer);

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
            return objectType == typeof(ProjectPart);
        }

        private dynamic Copy(IBasicDataRecord source)
        {
            if (source != null)
            {
                dynamic returnValue = new JObject();
                returnValue.Id = source.Id;
                return returnValue;
            }

            return null;
        }
    }
}