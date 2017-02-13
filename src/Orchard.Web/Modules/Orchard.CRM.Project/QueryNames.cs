using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project
{
    public static class QueryNames
    {
        public const string ProjectActivityStreamQueryName = "System_ProjectActivityStreamQuery";
        public const string WikiActivityStreamQueryName = "System_WikiActivityStreamQuery";
        public const string ProjectTicketsQueryName = "System_ProjectTicketsQuery";
        public const string ProjectNoneClosedTicketsQueryName = "System_ProjectNoneClosedTicketsQuery";
        public const string ProjectCriticalNoneClosedTicketsQueryName = "System_ProjectCriticalNoneClosedTicketsQuery";
        public const string ProjectUnassignedTicketsQueryName = "System_ProjectUnassignedTicketsQuery";
        public const string ProjectDiscussionsQueryName = "System_ProjectDiscussionsQuery";
        public const string ProjectMilestonesQueryName = "System_ProjectMilestonesQuery";
        public const string FolderItemsQueryName = "System_FolderItemsQuery";
        public const string ProjectDiscussionsSummaryQueryName = "System_ProjectDiscussionsSummaryQuery";
        public const string ProjectWikiesSummaryQueryName = "System_ProjectWikiesSummaryQuery";
        public const string ProjectTicketGroupByStatusDataReportName = "System_ProjectTicketGroupByStatusDataReport";
        public const string ProjectTicketGroupByStatusChartDataReportName = "System_ProjectTicketGroupByStatusChartDataReport";
        public const string ProjectNotClosedTicketsGroupByPriorityDataReportName = "System_ProjectNotClosedTicketsGroupByPriorityDataReport";

        public const string ProjectOverdueTicketGroupByTicketTypeChartDataReportName = "System_ProjectOverdueTicketGroupByTicketTypeChartDataReport";
        public const string ProjectOverdueTicketGroupByTicketTypeDataReportName = "System_ProjectOverdueTicketGroupByTicketTypeDataReport";

        public const string ProjectOverdueTicketsQueryName = "System_ProjectOverrudeTicketsQuery";

        public const string ProjectSummaryProjectionPortletShapeName = "ProjectSummaryProjectionPortlet";
        public const string ProjectTicketsSummaryShapeName = "ProjectTicketsSummary";
        public const string ProjectDiscussionsShapeName = "ProjectDiscussions";
        public const string ProjectProjectionShapeName = "ProjectProjection";
        public const string ProjectDashboardListLayout = "Project Dashboard Portlet List";
    }
}