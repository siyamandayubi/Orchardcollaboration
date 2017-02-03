using Orchard.Commands;
using Orchard.Core.Common.Models;
using Orchard.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.CRM.Core.Models;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.CRM.Project.Commands
{
    public class PortletCommand : DefaultOrchardCommandHandler
    {
        private readonly IOrchardServices services;
        private readonly IRepository<ReportRecord> reportRepository;
        private readonly IRepository<LayoutRecord> layoutRepository;

        public PortletCommand(
            IOrchardServices services,
            IRepository<ReportRecord> reportRepository,
            IRepository<LayoutRecord> layoutRepository)
        {
            this.layoutRepository = layoutRepository;
            this.reportRepository = reportRepository;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [CommandName("create-project-portlets")]
        public void Create()
        {
            var contentManager = this.services.ContentManager;

            // project tickets group by status
            this.CreateReportPortlet(
                QueryNames.ProjectTicketGroupByStatusDataReportName,
                ContentTypes.ProjectDashboardReportViewerPortletTemplateContentType,
                T("Tickets group by status").Text,
                true);

            // project not closed tickets group by priority
            this.CreateReportPortlet(
                QueryNames.ProjectNotClosedTicketsGroupByPriorityDataReportName,
                ContentTypes.ProjectDashboardReportViewerPortletTemplateContentType,
                T("Active Tickets group by priority").Text,
                false);

            // over due project tickets group by status
            this.CreateReportPortlet(
                QueryNames.ProjectOverdueTicketGroupByTicketTypeChartDataReportName,
                ContentTypes.ProjectDashboardReportViewerPortletTemplateContentType,
                T("Over due Tickets group by Type (Chart)").Text,
                false);

            // over due project tickets group by status list
            this.CreateReportPortlet(
                QueryNames.ProjectOverdueTicketGroupByTicketTypeDataReportName,
                ContentTypes.ProjectDashboardReportViewerPortletTemplateContentType,
                T("Over due Tickets group by Type (list)").Text,
                false);

            this.CreateSummaryContentItem(QueryNames.ProjectDiscussionsSummaryQueryName, ContentTypes.ProjectDashboardProjectionPortletTemplateContentType, T("Latest Discussions").Text,true);
            this.CreateSummaryContentItem(QueryNames.ProjectWikiesSummaryQueryName, ContentTypes.ProjectDashboardProjectionPortletTemplateContentType, T("Latest Wiki pages").Text, false);
            this.CreateSummaryContentItem(QueryNames.ProjectUnassignedTicketsQueryName, ContentTypes.ProjectDashboardProjectionPortletTemplateContentType, T("Unassigned Tickets").Text, true);
            this.CreateSummaryContentItem(QueryNames.ProjectCriticalNoneClosedTicketsQueryName, ContentTypes.ProjectDashboardProjectionPortletTemplateContentType, T("Active Critical Tickets").Text, false);
            this.CreateSummaryContentItem(QueryNames.ProjectOverdueTicketsQueryName, ContentTypes.ProjectDashboardProjectionPortletTemplateContentType, T("Latest Over Due Tickets").Text, false);

            // create activity stream summary
            var activityStreamQuery = this.GetProjectActivityStreamQueryPart();
            var projectActivityStreamSummaryItem = contentManager.New(ContentTypes.ProjectLastActivityStreamTemplateContentType);
            contentManager.Create(projectActivityStreamSummaryItem);
            projectActivityStreamSummaryItem.As<TitlePart>().Title = T("Project Latest Activities").Text;
            var activityStreamPart = projectActivityStreamSummaryItem.As<ActivityStreamPart>();
            if (activityStreamQuery != null)
            {
                activityStreamPart.QueryId = activityStreamQuery.Record.Id;
            }

            InfosetPart infosetPart = projectActivityStreamSummaryItem.As<InfosetPart>();
            infosetPart.Store<bool>(FieldNames.IsDefaultPortletFieldName, true);

            contentManager.Publish(projectActivityStreamSummaryItem);
        }

        public QueryPart GetProjectActivityStreamQueryPart()
        {
            return this.GetQuery(QueryNames.ProjectActivityStreamQueryName);
        }

        private void CreateReportPortlet(string reportName, string contentType, string title, bool isDefaultPortlet)
        {
            var contentManager = this.services.ContentManager;

            // project tickets group by status
            var report = this.reportRepository.Table.FirstOrDefault(c => c.Name == reportName);
            if (report != null)
            {
                var projectTicketsGroupByStats = contentManager.New(contentType);
                contentManager.Create(projectTicketsGroupByStats);

                TitlePart titlePart = projectTicketsGroupByStats.As<TitlePart>();
                titlePart.Title = title;
                var reportViewerPart = projectTicketsGroupByStats.As<DataReportViewerPart>();
                reportViewerPart.Record.Report = report;

                InfosetPart infosetPart = projectTicketsGroupByStats.As<InfosetPart>();
                infosetPart.Store<bool>(FieldNames.IsDefaultPortletFieldName, isDefaultPortlet);
                
                contentManager.Publish(projectTicketsGroupByStats);
            }
        }

        private QueryPart GetQuery(string title)
        {
            var query = this.services
                .ContentManager
                .HqlQuery()
                .ForType("Query")
                .Where(c => c.ContentPartRecord<TitlePartRecord>(), c => c.Eq("Title", title))
                .Slice(0, 1)
                .FirstOrDefault();

            return query != null ? query.As<QueryPart>() : null;
        }

        private void CreateSummaryContentItem(string queryName, string contentType, string title, bool isDefaultPortlet)
        {
            // project discussions Summary
            var query = this.GetQuery(queryName);
            if (query != null)
            {
                var summaryContentItem = this.services.ContentManager.Create(contentType, VersionOptions.Draft);
                var projection = summaryContentItem.As<ProjectionWithDynamicSortPart>();
                projection.Record.QueryPartRecord = query.Record;
                projection.Record.ItemsPerPage = 5;
                projection.Record.Items = 5;
                var layout = this.layoutRepository.Table.FirstOrDefault(c => c.QueryPartRecord.Id == query.Id && c.Category == "Html" && c.Type == "Shape");
                if (layout != null)
                {
                    projection.Record.LayoutRecord = layout;
                }

                TitlePart titlePart = summaryContentItem.As<TitlePart>();

                if (titlePart != null && !string.IsNullOrEmpty(title))
                {
                    titlePart.Title = title;
                }

                InfosetPart infosetPart = summaryContentItem.As<InfosetPart>();
                infosetPart.Store<bool>(FieldNames.IsDefaultPortletFieldName, isDefaultPortlet);

                this.services.ContentManager.Publish(summaryContentItem);
            }
        }
    }
}