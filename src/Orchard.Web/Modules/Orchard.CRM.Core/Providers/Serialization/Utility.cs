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
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public static class Utility
    {
        public static void WriteArray(string name, IEnumerable values, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();

            foreach (var item in values)
            {
                if (item == null)
                {
                    writer.WriteNull();
                    continue;
                }

                if (item is IShape || item is ContentItem || JToken.FromObject(item).Type == JTokenType.Object)
                {
                    serializer.Serialize(writer, item);
                }
                else
                {
                    writer.WriteValue(item);
                }
            }

            writer.WriteEndArray();
        }

        public static void WriteProperty(string name, object value, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(name);

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is IShape || value is ContentItem || JToken.FromObject(value).Type == JTokenType.Object)
            {
                serializer.Serialize(writer, value);
            }
            else
            {
                writer.WriteValue(value);
            }
        }
    }
}