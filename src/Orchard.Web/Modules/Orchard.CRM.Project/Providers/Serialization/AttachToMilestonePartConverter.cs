using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.CRM.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers.Serialization
{
    public class AttachToMilestonePartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            AttachToMilestonePart contentPart = value as AttachToMilestonePart;
            var record = contentPart.Record;

            // dynamic  object corresponding to TicketPartRecord 
            dynamic dynamicRecord = new JObject();

            // Basic data
            dynamicRecord.Size = record.Size;
            dynamicRecord.MilestoneId = record.MilestoneId;
            dynamicRecord.Order = record.OrderId;
            dynamicRecord.Id = record.Id;

            Orchard.CRM.Core.Providers.Serialization.Utility.WriteProperty("Record", dynamicRecord, writer, serializer);

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
            return objectType == typeof(AttachToMilestonePart);
        }

    }
}