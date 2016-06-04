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


namespace Orchard.CRM.Core.Providers
{
    using NHibernate.Transform;
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Projections.Descriptors.Filter;
    using Orchard.Reporting.Models;
    using Orchard.Reporting.Providers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    public class TicketGroupByParameterProvider : IGroupByParameterProvider
    {
        public const string CategoryName = "TicketProperties";
        public const string GroupingByTicketType = "TicketType";
        public const string GroupingByTicketStatus = "TicketStatus";
        public const string GroupingByTicketPriority = "TicketPriority";

        private readonly Lazy<ISessionLocator> sessionLocator;
        private readonly IBasicDataService basicDataService;

        public TicketGroupByParameterProvider(Lazy<ISessionLocator> sessionLocator, IBasicDataService basicDataService)
        {
            this.sessionLocator = sessionLocator;
            this.basicDataService = basicDataService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeGroupByContext describe)
        {
            var descriptor = describe.For(CategoryName, T("{Ticket properties"), T("Ticket properties"));

            List<AggregateMethods> methods = new List<AggregateMethods>();
            methods.Add(AggregateMethods.Count);

            // Ticket Type
            descriptor.Element(
                type: GroupingByTicketType,
                name: T("Ticket Type"),
                description: T("group the result by {0} values", "Ticket Type"),
                run: (query, method) => RunTicketTypesGroupingQuery(query, method),
                aggregateMethods: methods,
                display: context => T("Ticket Type"));

            // Status
            descriptor.Element(
                type: GroupingByTicketStatus,
                name: T("Ticket Status"),
                description: T("group the result by {0} values", "Ticket states"),
                run: (query, method) => RunTicketStatusGroupingQuery(query, method),
                aggregateMethods: methods,
                display: context => T("Ticket Status"));

            // Priority
            descriptor.Element(
                type: GroupingByTicketPriority,
                name: T("Ticket Priority"),
                description: T("group the result by {0} values", "Ticket Priorities"),
                run: (query, method) => RunTicketPriorityGroupingQuery(query, method),
                aggregateMethods: methods,
                display: context => T("Ticket Priority"));
        }

        private IEnumerable<AggregationResult> RunTicketTypesGroupingQuery(IHqlQuery query, AggregateMethods aggregation)
        {
            var queryResult = this.RunQuery(query, aggregation, "TicketType");

            var ticketTypes = this.basicDataService.GetTicketTypes();

            List<AggregationResult> returnValue = new List<AggregationResult>();

            foreach (var item in queryResult)
            {
                var ticketType = item.Key != null ? ticketTypes.FirstOrDefault(c => c.Id == item.Key.Value) : null;
                string label = ticketType == null ? T("[No Ticket Type]").Text : ticketType.Name;

                AggregationResult aggregationResult = new AggregationResult
                {
                    AggregationValue = item.Value,
                    Label = label,
                    GroupingField = item.Key
                };

                returnValue.Add(aggregationResult);
            }

            return returnValue;
        }

        private IEnumerable<AggregationResult> RunTicketStatusGroupingQuery(IHqlQuery query, AggregateMethods aggregation)
        {
            var queryResult = this.RunQuery(query, aggregation, "StatusRecord");

            var statusItems = this.basicDataService.GetStatusRecords();

            List<AggregationResult> returnValue = new List<AggregationResult>();

            foreach (var item in queryResult)
            {
                var statusRecord = item.Key != null ? statusItems.FirstOrDefault(c => c.Id == item.Key.Value) : null;
                string label = statusRecord == null ? T("[No Status]").Text : statusRecord.Name;

                AggregationResult aggregationResult = new AggregationResult
                {
                    AggregationValue = item.Value,
                    Label = label,
                    GroupingField = item.Key
                };

                returnValue.Add(aggregationResult);
            }

            return returnValue;
        }

        private IEnumerable<AggregationResult> RunTicketPriorityGroupingQuery(IHqlQuery query, AggregateMethods aggregation)
        {
            var queryResult = this.RunQuery(query, aggregation, "PriorityRecord");

            var priorities = this.basicDataService.GetPriorities();

            List<AggregationResult> returnValue = new List<AggregationResult>();

            foreach (var item in queryResult)
            {
                var priority = item.Key != null ? priorities.FirstOrDefault(c => c.Id == item.Key.Value) : null;
                string label = priority == null ? T("[No Priority]").Text : priority.Name;

                AggregationResult aggregationResult = new AggregationResult
                {
                    AggregationValue = item.Value,
                    Label = label,
                    GroupingField = item.Key
                };

                returnValue.Add(aggregationResult);
            }

            return returnValue;
        }

        private IList<KeyValuePair<int?, double>> RunQuery(IHqlQuery query, AggregateMethods aggregation, string propertyPath)
        {
            DefaultHqlQuery defaultQuery = query as DefaultHqlQuery;
            var queryHql = defaultQuery.ToHql(true);

            var hql = @"select ticketTable.{0}.Id as GroupKey, COUNT(*) as GroupValue
                        from Orchard.ContentManagement.Records.ContentItemVersionRecord as kiv
                        join kiv.ContentItemRecord as ki
                        join ki.TicketPartRecord as ticketTable
                        where (kiv.Published = True) AND kiv.Id in ({1})
                        group by ticketTable.{0}.Id";

            hql = string.Format(CultureInfo.InvariantCulture, hql, propertyPath, queryHql);

            var session = this.sessionLocator.Value.For(typeof(ContentItem));
            var result = session
                   .CreateQuery(hql)
                   .SetCacheable(false)
                   .SetResultTransformer(Transformers.AliasToEntityMap)
                   .List<IDictionary>();

            List<KeyValuePair<int?, double>> returnValue = new List<KeyValuePair<int?, double>>();

            foreach (var group in result)
            {
                double value = double.Parse(group["GroupValue"].ToString());
                int? key = group["GroupKey"] != null ? (int?)int.Parse(group["GroupKey"].ToString()) : null;

                returnValue.Add(new KeyValuePair<int?, double>(key, value));
            }

            return returnValue;
        }
    }
}