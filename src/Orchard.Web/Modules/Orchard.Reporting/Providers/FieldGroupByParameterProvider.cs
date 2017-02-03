using NHibernate.Transform;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Reporting.Models;
using Orchard.Reporting.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Orchard.Reporting.Providers
{
    public class FieldGroupByParameterProvider : IGroupByParameterProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IEnumerable<IFieldTypeEditor> _fieldTypeEditors;
        private readonly IFieldAggregateQueryService fieldAggregateQueryService;

        public FieldGroupByParameterProvider(
            IFieldAggregateQueryService fieldAggregateQueryService,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IEnumerable<IFieldTypeEditor> fieldTypeEditors)
        {
            this.fieldAggregateQueryService = fieldAggregateQueryService;
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            _fieldTypeEditors = fieldTypeEditors;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void DescribeNumericFieldMemberContext(
           ContentPartFieldDefinition localField,
           ContentPartDefinition localPart,
           DescribeGroupByParameterFor descriptor,
           string storageName,
           LocalizedString description,
           LocalizedString displayName,
           IFieldTypeEditor fieldTypeEditor)
        {
            if (localField.FieldDefinition.Name != "NumericField")
            {
                throw new ArgumentException("localField is not an instance of DateTimeField");
            }

            //field.FieldDefinition.Name 
            List<AggregateMethods> methods = new List<AggregateMethods>();
            methods.Add(AggregateMethods.Count);
            methods.Add(AggregateMethods.Sum);
            methods.Add(AggregateMethods.Average);
            methods.Add(AggregateMethods.Minimum);
            methods.Add(AggregateMethods.Maximum);

            Tuple<string, string, int>[] intervals = new Tuple<string, string, int>[]{
                new Tuple<string, string, int>("Without","Without any interval", 1),
                new Tuple<string, string, int>("By10","in intervals of 10", 10),
                new Tuple<string, string, int>("By100","in intervals of 100", 100),
                new Tuple<string, string, int>("By1000","in intervals of 1000", 1000),
                new Tuple<string, string, int>("By10000","in intervals of 10000", 10000),
            };

            foreach (var interval in intervals)
            {
                string typeValue = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", localPart.Name, localField.Name, storageName, interval.Item1);

                LocalizedString name = interval.Item3 != 1 ?
                    T("{0} ({1})", localField.DisplayName, interval.Item3.ToString(CultureInfo.InvariantCulture)) :
                    T(localField.DisplayName);

                descriptor.Element(
                    type: typeValue,
                    name: name,
                    description: T("group the result by {0} values {1}", localField.DisplayName, interval.Item2),
                    run: (query, method, state) => { return this.fieldAggregateQueryService.RunNumericAggregation(query, method, localField.Name, localPart.Name, interval.Item3); },
                    aggregateMethods: methods,
                    display: context => fieldTypeEditor.DisplayFilter(localPart.Name.CamelFriendly() + "." + localField.DisplayName, storageName, context.State));
            }
        }

        public void Describe(DescribeGroupByContext describe)
        {
            foreach (var part in _contentDefinitionManager.ListPartDefinitions())
            {
                if (!part.Fields.Any())
                {
                    continue;
                }

                var descriptor = describe.For(part.Name + "ContentFields", T("{0} Content Fields", part.Name.CamelFriendly()), T("Content Fields for {0}", part.Name.CamelFriendly()));

                foreach (var field in part.Fields)
                {
                    var localField = field;
                    var localPart = part;
                    var drivers = _contentFieldDrivers.Where(x => x.GetFieldInfo().Any(fi => fi.FieldTypeName == localField.FieldDefinition.Name)).ToList();

                    var membersContext = new DescribeMembersContext(
                           (storageName, storageType, displayName, description) =>
                           {
                               // look for a compatible field type editor
                               IFieldTypeEditor fieldTypeEditor = _fieldTypeEditors.FirstOrDefault(x => x.CanHandle(storageType));

                               if (fieldTypeEditor == null)
                               {
                                   return;
                               }

                               if (localField.FieldDefinition.Name == "NumericField")
                               {
                                   this.DescribeNumericFieldMemberContext(
                                       localField: localField,
                                       description: description,
                                       storageName: storageName,
                                       descriptor: descriptor,
                                       displayName: displayName,
                                       fieldTypeEditor: fieldTypeEditor,
                                       localPart: localPart);
                               }
                               else if (localField.FieldDefinition.Name == "EnumerationField")
                               {
                                   //field.FieldDefinition.Name 
                                   List<AggregateMethods> methods = new List<AggregateMethods>();
                                   methods.Add(AggregateMethods.Count);

                                   descriptor.Element(
                                       type: localPart.Name + "." + localField.Name + "." + storageName,
                                       name: T(localField.DisplayName),
                                       description: T("group the result by {0} values", localField.DisplayName),
                                       run: (query, method, state) => this.fieldAggregateQueryService.RunEnumerationAggregation(query,method,localField.Name,localPart.Name),
                                       aggregateMethods: methods,
                                       display: context => fieldTypeEditor.DisplayFilter(localPart.Name.CamelFriendly() + "." + localField.DisplayName, storageName, context.State));
                               }
                           });

                    foreach (var driver in drivers)
                    {
                        driver.Describe(membersContext);
                    }
                }
            }
        }
    }
}