
namespace Orchard.CRM.Core.Providers
{
    using Newtonsoft.Json.Linq;
    using NHibernate.Transform;
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Services;
    using Orchard.Data;
    using Orchard.Localization;
    using Orchard.Reporting.Models;
    using Orchard.Reporting.Providers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class TicketGroupByParameterProvider : IGroupByParameterProvider
    {
        public const string CategoryName = "TicketProperties";
        public const string GroupingByTicketType = "TicketType";
        public const string GroupingByTicketStatus = "TicketStatus";
        public const string GroupingByTicketPriority = "TicketPriority";

        private readonly Lazy<ITransactionManager> transactionManager;
        private readonly IBasicDataService basicDataService;

        public TicketGroupByParameterProvider(Lazy<ITransactionManager> transactionManager, IBasicDataService basicDataService)
        {
            this.transactionManager = transactionManager;
            this.basicDataService = basicDataService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeGroupByContext describe)
        {
            var descriptor = describe.For(CategoryName, T("Ticket properties"), T("Ticket properties"));

            List<AggregateMethods> methods = new List<AggregateMethods>();
            methods.Add(AggregateMethods.Count);
            methods.Add(AggregateMethods.Sum);
            methods.Add(AggregateMethods.Average);
            methods.Add(AggregateMethods.Maximum);
            methods.Add(AggregateMethods.Minimum);

            // Ticket Type
            descriptor.Element(
                type: GroupingByTicketType,
                name: T("Ticket Type"),
                description: T("group the result by {0} values", "Ticket Type"),
                run: (query, method, state) => RunTicketTypesGroupingQuery(query, method, state),
                aggregateMethods: methods,
                display: context => T("Ticket Type"));

            // Status
            descriptor.Element(
                type: GroupingByTicketStatus,
                name: T("Ticket Status"),
                description: T("group the result by {0} values", "Ticket states"),
                run: (query, state, method) => RunTicketStatusGroupingQuery(query, state, method),
                aggregateMethods: methods,
                display: context => T("Ticket Status"));

            // Priority
            descriptor.Element(
                type: GroupingByTicketPriority,
                name: T("Ticket Priority"),
                description: T("group the result by {0} values", "Ticket Priorities"),
                run: (query, method, state) => RunTicketPriorityGroupingQuery(query, method, state),
                aggregateMethods: methods,
                display: context => T("Ticket Priority"));
        }

        private IEnumerable<AggregationResult> RunTicketTypesGroupingQuery(IHqlQuery query, AggregateMethods aggregation, string state)
        {
            var queryResult = CRMHelper.RunGroupByQuery(transactionManager.Value, query, aggregation, "TicketPartRecord.TicketType", state, "ticket");

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

        private IEnumerable<AggregationResult> RunTicketStatusGroupingQuery(IHqlQuery query, AggregateMethods aggregation, string state)
        {
            var queryResult = CRMHelper.RunGroupByQuery(transactionManager.Value, query, aggregation, "TicketPartRecord.StatusRecord", state, "ticket");

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

        private IEnumerable<AggregationResult> RunTicketPriorityGroupingQuery(IHqlQuery query, AggregateMethods aggregation, string state)
        {
            var queryResult = CRMHelper.RunGroupByQuery(transactionManager.Value, query, aggregation, "TicketPartRecord.PriorityRecord", state, "ticket");

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
    }
}