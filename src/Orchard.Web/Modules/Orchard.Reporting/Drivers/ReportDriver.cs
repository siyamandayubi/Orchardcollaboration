using Orchard.ContentManagement.Drivers;
using Orchard.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Projections.Models;
using System.Globalization;
using Orchard.ContentManagement;

namespace Orchard.Reporting.Drivers
{
    public class ReportDriver : ContentPartDriver<ReportPart>
    {
        private readonly IRepository<ReportRecord> reportRecordRepository;
        private readonly IOrchardServices services;
        public ReportDriver(IRepository<ReportRecord> reportRecordRepository, IOrchardServices services)
        {
            this.services = services;
            this.reportRecordRepository = reportRecordRepository;
        }
        protected override void Importing(ReportPart part, ImportContentContext context)
        {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null)
            {
                return;
            }

            string guid = string.Empty;
            context.ImportAttribute(part.PartDefinition.Name, "Guid", g => guid = g);

            var reportRecord = this.reportRecordRepository.Table.FirstOrDefault(c => c.Guid == guid);

            bool isNew = false;
            if (reportRecord == null)
            {
                isNew = true;
                reportRecord = new ReportRecord { Guid = guid };
            }

            // ChartType
            context.ImportAttribute(part.PartDefinition.Name, "ChartType", chartType => reportRecord.ChartType = int.Parse(chartType));

            // State
            context.ImportAttribute(part.PartDefinition.Name, "State", state => reportRecord.State = state);

            // Name
            context.ImportAttribute(part.PartDefinition.Name, "Name", name => reportRecord.Name = name);

            // Title
            context.ImportAttribute(part.PartDefinition.Name, "Title", title => reportRecord.Title = title);

            // State
            context.ImportAttribute(part.PartDefinition.Name, "State", state => reportRecord.State = state);

            // GroupByCategory
            context.ImportAttribute(part.PartDefinition.Name, "GroupByCategory", groupByCategory => reportRecord.GroupByCategory = groupByCategory);

            // GroupByType
            context.ImportAttribute(part.PartDefinition.Name, "GroupByType", groupByType => reportRecord.GroupByType = groupByType);

            // AggregateMethod
            context.ImportAttribute(part.PartDefinition.Name, "AggregateMethod", aggregateMethod => reportRecord.AggregateMethod = int.Parse(aggregateMethod));

            var queryId = context.Attribute(part.PartDefinition.Name, "QueryId");
            if (queryId != null)
            {
                var query = context.GetItemFromSession(queryId);
                if (query != null)
                {
                    reportRecord.Query = query.As<QueryPart>().Record;
                }
            }
            else
            {
                throw new ArgumentNullException("QueryId is null");
            }

            if (isNew)
            {
                this.reportRecordRepository.Create(reportRecord);
            }

            this.reportRecordRepository.Flush();

            part.ReportId = reportRecord.Id;
        }

        protected override void Exporting(ReportPart part, ExportContentContext context)
        {
            var partElement = context.Element(part.PartDefinition.Name);

            var reportRecord = this.reportRecordRepository.Table.FirstOrDefault(c => c.Id == part.ReportId);

            if (reportRecord == null)
            {
                return;
            }

            // Guid
            partElement.SetAttributeValue("Guid", reportRecord.Guid);

            // QueryId
            if (reportRecord.Query != null)
            {
                var queryPart = services.ContentManager.Query<QueryPart, QueryPartRecord>("Query").Where(x => x.Id == reportRecord.Query.Id).List().FirstOrDefault();
                if (queryPart != null)
                {
                    var queryIdentity = services.ContentManager.GetItemMetadata(queryPart).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("QueryId", queryIdentity.ToString());
                }
            }

            // ChartType
            partElement.SetAttributeValue("ChartType", reportRecord.ChartType.ToString(CultureInfo.InvariantCulture));

            // Name
            partElement.SetAttributeValue("Name", reportRecord.Name);

            // Title
            partElement.SetAttributeValue("Title", reportRecord.Name);

            // State
            partElement.SetAttributeValue("State", reportRecord.State);

            // GroupByCategory
            partElement.SetAttributeValue("GroupByCategory", reportRecord.GroupByCategory);

            // GroupByType
            partElement.SetAttributeValue("GroupByType", reportRecord.GroupByType);

            // AggregateMethod
            partElement.SetAttributeValue("AggregateMethod", reportRecord.AggregateMethod);
        }
    }
}