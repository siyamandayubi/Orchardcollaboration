using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class LayoutRecordConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            LayoutRecord record = value as LayoutRecord;

            // Id
            Utility.WriteProperty("Id", record.Id, writer, serializer);

            // DisplayType
            Utility.WriteProperty("DisplayType", record.DisplayType, writer, serializer);

            // Category
            Utility.WriteProperty("Category", record.Category, writer, serializer);

            // Type
            Utility.WriteProperty("Type", record.Type, writer, serializer);

            // State
            Utility.WriteProperty("State", record.State, writer, serializer);

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
            return objectType == typeof(LayoutRecord);
        }
    }
}