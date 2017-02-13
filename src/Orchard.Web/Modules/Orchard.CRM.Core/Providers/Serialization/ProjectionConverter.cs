using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Projections.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ProjectionConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            ProjectionPart contentPart = value as ProjectionPart;
            var record = contentPart.Record;
            // dynamic  object corresponding to TicketPartRecord 
            dynamic dynamicRecord = new JObject();

            dynamicRecord.ItemsPerPage = record.ItemsPerPage;
            dynamicRecord.MaxItems = record.MaxItems;
            dynamicRecord.PagerSuffix = record.PagerSuffix;
            dynamicRecord.Skip = record.Skip;
            dynamicRecord.DisplayPager = record.DisplayPager;
            dynamicRecord.DisplayPager = record.DisplayPager;

            // layout
            if (record.LayoutRecord != null)
            {
                var layoutRecord = record.LayoutRecord;
                dynamic layout = new JObject();
                layout.Id = layoutRecord.Id;
                layout.DisplayType = layoutRecord.DisplayType;
                layout.Category = layoutRecord.Category;
                layout.Type = layoutRecord.Type;
                layout.State = layoutRecord.State;
                dynamicRecord.LayoutRecord = layout;
            }

            // query
            if (record.QueryPartRecord != null)
            {
                var queryRecord = record.QueryPartRecord;
                dynamic query = new JObject();
                query.Id = queryRecord.Id;
                dynamicRecord.QueryPartRecord = query;
            }

            // Id
            Utility.WriteProperty("Id", contentPart.Id, writer, serializer);

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
            return objectType == typeof(ProjectionPart);
        }
    }
}