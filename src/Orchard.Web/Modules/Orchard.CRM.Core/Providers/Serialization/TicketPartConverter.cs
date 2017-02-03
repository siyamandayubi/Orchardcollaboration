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
    public class TicketPartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            TicketPart contentPart = value as TicketPart;
            var record = contentPart.Record;

            // dynamic  object corresponding to TicketPartRecord 
            dynamic dynamicRecord = new JObject();

            // Basic data
            dynamicRecord.Status = this.Copy(record.StatusRecord);
            dynamicRecord.TicketType = this.Copy(record.TicketType);
            dynamicRecord.PriorityRecord = this.Copy(record.PriorityRecord);
            dynamicRecord.Service = this.Copy(record.Service);
            dynamicRecord.Description = record.Description;
            dynamicRecord.Title = record.Title;
            dynamicRecord.SourceId = record.SourceId;
            dynamicRecord.SourceData = record.SourceData;
            dynamicRecord.Identity = null;
            if (record.Identity != null)
            {
                dynamicRecord.Identity = new JObject();
                dynamicRecord.Identity.Add("Id", record.Identity.Id);
            }

            if (record.RequestingUser != null)
            {
                dynamicRecord.RequestingUser = new JObject();
                dynamicRecord.Id = record.RequestingUser.Id;
                dynamicRecord.UserName = record.RequestingUser.UserName;
            }

            dynamicRecord.DueDate = record.DueDate;

            Utility.WriteProperty("Record", dynamicRecord, writer, serializer);

            this.WriteCommonFields(writer, contentPart, serializer);

            // StatusTimes
            Utility.WriteArray("StatusTimes", contentPart.StatusTimes, writer, serializer);

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
            return objectType == typeof(TicketPart);
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