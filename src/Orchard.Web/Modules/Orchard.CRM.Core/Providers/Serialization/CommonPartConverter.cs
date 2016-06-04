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
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Providers.Serialization
{
    public class CommonPartConverter : ContentPartConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            CommonPart contentPart = value as CommonPart;

            // CreatedUtc
            Utility.WriteProperty("CreatedUtc", contentPart.CreatedUtc, writer, serializer);

            // PublishedUtc
            Utility.WriteProperty("PublishedUtc", contentPart.PublishedUtc, writer, serializer);

            // ModifiedUtc
            Utility.WriteProperty("ModifiedUtc", contentPart.ModifiedUtc, writer, serializer);

            // VersionCreatedUtc
            Utility.WriteProperty("VersionCreatedUtc", contentPart.VersionCreatedUtc, writer, serializer);

            // VersionModifiedUtc
            Utility.WriteProperty("VersionModifiedUtc", contentPart.VersionModifiedUtc, writer, serializer);

            // VersionPublishedUtc
            Utility.WriteProperty("VersionPublishedUtc", contentPart.VersionPublishedUtc, writer, serializer);

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
            return objectType == typeof(CommonPart);
        }
    }
}