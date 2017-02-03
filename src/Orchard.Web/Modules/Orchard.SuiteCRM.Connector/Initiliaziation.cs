using Newtonsoft.Json;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.Environment;
using Orchard.SuiteCRM.Connector.Models;
using Orchard.SuiteCRM.Connector.Providers.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector
{
    public class Initiliaziation : IOrchardShellEvents
    {
        public void Activated()
        {
            // register JSON Converters
            JsonSerializerSettings defaultSetting = JsonConvert.DefaultSettings != null ? JsonConvert.DefaultSettings() : new JsonSerializerSettings();

            defaultSetting.Converters = defaultSetting.Converters != null ? defaultSetting.Converters : new List<JsonConverter>();

            defaultSetting.Converters.Add(new SuiteCRMProjectPartConverter());
            JsonConvertersTypes.TypesHavingJsonConverters.Add(typeof(SuiteCRMProjectPart));

            JsonConvert.DefaultSettings = () => defaultSetting;
        }

        public void Terminating()
        {

        }
    }
}