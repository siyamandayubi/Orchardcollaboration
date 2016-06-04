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

using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class PartTypeRecordMatchingService : IPartTypeRecordMatchingService
    {
        public Dictionary<string, PropertyInfo> partsRecordsDictionary = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, Type[]> genericTypesDictionary = new Dictionary<string, Type[]>();

        public void Set(ContentPart part, object record)
        {
            if (!this.partsRecordsDictionary.ContainsKey(part.PartDefinition.Name))
            {
                var partType = part.GetType();
                var propertyInfo = partType.GetProperty("Record");
                this.partsRecordsDictionary.Add(part.PartDefinition.Name, propertyInfo);
            }

            var property = this.partsRecordsDictionary[part.PartDefinition.Name];

            if (property != null)
            {
                property.SetValue(part, record, null);
            }
        }

        public bool Match(ContentPart part, object record)
        {
            if (!this.genericTypesDictionary.ContainsKey(part.PartDefinition.Name))
            {
                var genericTypes = this.GetGenericType(part);
                this.genericTypesDictionary[part.PartDefinition.Name] = genericTypes.ToArray();
            }

            var list = this.genericTypesDictionary[part.PartDefinition.Name];
            var partRecordType = record.GetType();
            foreach (var genericType in list)
            {
                if (genericType == partRecordType)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Type> GetGenericType(object part)
        {
            var partType = part.GetType();
            List<Type> genericTypes = new List<Type>();

            var temp = partType.GetGenericArguments();
            genericTypes.AddRange(temp);

            var baseType = partType.BaseType;
            while (baseType != null)
            {
                var baseTypeGenerics = baseType.GetGenericArguments();
                genericTypes.AddRange(baseTypeGenerics);
                baseType = baseType.BaseType;

                if (baseType == typeof(ContentPart<>))
                {
                    break;
                }
            }

            return genericTypes;
        }
    }
}