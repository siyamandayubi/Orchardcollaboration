using Newtonsoft.Json;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.Environment;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core
{
    public class Initialization : IOrchardShellEvents
    {
        public void Activated()
        {
            // register JSON Converters
            JsonSerializerSettings defaultSetting = JsonConvert.DefaultSettings != null ? JsonConvert.DefaultSettings() : new JsonSerializerSettings();

            defaultSetting.Converters = defaultSetting.Converters != null ? defaultSetting.Converters : new List<JsonConverter>();

            defaultSetting.Converters.Add(new CommonPartConverter());
            defaultSetting.Converters.Add(new ContentItemConverter());
            defaultSetting.Converters.Add(new ContentPartConverter());
            defaultSetting.Converters.Add(new TitlePartConverter());
            defaultSetting.Converters.Add(new TicketPartConverter());
            defaultSetting.Converters.Add(new ProjectionConverter());
            defaultSetting.Converters.Add(new ShapeConverter());
            defaultSetting.Converters.Add(new ProjectPartConverter());
            defaultSetting.Converters.Add(new ContentTypePartDefinitionConverter());
            defaultSetting.Converters.Add(new ExpandoObjectConverter());
            defaultSetting.Converters.Add(new LayoutRecordConverter());
            defaultSetting.Converters.Add(new ShapeMetadataConverter());

            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(TitlePart));
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(CommonPart));
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(TicketPart));
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(ProjectPart));
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(ProjectionPart));

            JsonConvert.DefaultSettings = () => defaultSetting;
        }

        public void Terminating()
        {

        }
    }
}