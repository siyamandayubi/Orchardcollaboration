/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

using Newtonsoft.Json;
using Orchard.CRM.Core.Providers.Serialization;
using Orchard.Environment;
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
            defaultSetting.Converters.Add(new ShapeConverter());
            defaultSetting.Converters.Add(new ProjectPartConverter());
            defaultSetting.Converters.Add(new ContentTypePartDefinitionConverter());
            defaultSetting.Converters.Add(new ExpandoObjectConverter());

            JsonConvert.DefaultSettings = () => defaultSetting;
        }

        public void Terminating()
        {

        }
    }
}