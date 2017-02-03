using Newtonsoft.Json;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Providers.Serialization;
using Orchard.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project
{
    public class Initialization : IOrchardShellEvents
    {
        public void Activated()
        {
            // register JSON Converters
            JsonSerializerSettings defaultSetting = JsonConvert.DefaultSettings != null ? JsonConvert.DefaultSettings() : new JsonSerializerSettings();

            defaultSetting.Converters = defaultSetting.Converters != null ? defaultSetting.Converters : new List<JsonConverter>();

            defaultSetting.Converters.Add(new AttachToMilestonePartConverter());
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(AttachToMilestonePart));

            JsonConvert.DefaultSettings = () => defaultSetting;
        }

        public void Terminating()
        {

        }
    }
}