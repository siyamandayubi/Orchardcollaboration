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
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.UI.Zones;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class ShapeConverter : JsonConverter
    {
        public ShapeConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            Shape shape = value as Shape;
            foreach (DictionaryEntry property in shape.Properties)
            {
                string key = property.Key.ToString();

                if (key == "Parent" || key == "Items" || key == "Source")
                {
                    continue;
                }

                if (property.Value is ExpandoObject)
                {
                    Utility.WriteProperty(key, property.Value, writer, serializer);
                }
                else if (property.Value is IEnumerable)
                {
                    Utility.WriteArray(key, property.Value as IEnumerable, writer, serializer);
                }
                else
                {
                    Utility.WriteProperty(key, property.Value, writer, serializer);
                }
            }

            // Attributes
            Utility.WriteArray("Attributes", shape.Attributes, writer, serializer);

            // Items
            Utility.WriteArray("Items", shape.Items, writer, serializer);

            // Position
            Utility.WriteProperty("Position", shape.Position, writer, serializer);

            // Id
            Utility.WriteProperty("Id", shape.Id, writer, serializer);

            // Classes
            Utility.WriteArray("Classes", shape.Classes, writer, serializer);

            // Metadata
            Utility.WriteProperty("Metadata", shape.Metadata, writer, serializer);

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
            return objectType == typeof(Shape) || objectType == typeof(ZoneHolding);
        }
    }
}