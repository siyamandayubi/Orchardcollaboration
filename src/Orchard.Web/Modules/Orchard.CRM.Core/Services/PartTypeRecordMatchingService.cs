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