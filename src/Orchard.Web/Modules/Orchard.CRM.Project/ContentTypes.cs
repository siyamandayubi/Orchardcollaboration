using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project
{
    public class ContentTypes
    {
        public const string EmptyContentType = "EmptyContentItem";
        public const string ProjectContentType = "ProjectItem";
        public const string DiscussionContentType = "DiscussionItem";
        public const string ProjectDiscussionsContentType = "ProjectDiscussions";
        public const string TicketContentType = "Ticket";
        public const string FolderContentType = "FolderItem";
        public const string WikiItemType = "WikiItem";
        public const string ProjectTicketsContentType = "ProjectTickets";
        public const string ProjectWikiContentType = "ProjectWiki";
        public const string RootWikiContentType = "RootWiki";
        public const string ProjectListContentType = "ProjectListItem";
        public const string MilestoneContentType = "MilestoneItem";
        public const string ProjectMenuContentType = "ProjectMenu";

        public const string ProjectProjectionContentType = "ProjectProjection";

        [Obsolete]
        public const string DiscussionsSummaryContentType = "DiscussionsSummary";

        [Obsolete]
        public const string TicketssSummaryContentType = "TicketsSummary";
        
        public const string ProjectDetailContentType = "ProjectDetail";
        
        public const string ProjectActivityStreamType = "ProjectActivityStream";
        
        [Obsolete]
        public const string ProjectTicketsSummaryGroupByStatusContentType = "ProjectTicketsSummaryGroupByStatus";
        public const string ProjectLastActivityStreamContentType = "ProjectLastActivityStream";
        public const string ProjectLastActivityStreamTemplateContentType = "ProjectLastActivityStreamTemplate";

        public const string ProjectDashboardProjectionPortletContentType = "ProjectDashboardProjectionPortlet";
        public const string ProjectDashboardReportViewerPortletContentType = "ProjectDashboardReportViewerPortlet";

        public const string ProjectDashboardProjectionPortletTemplateContentType = "ProjectDashboardProjectionPortletTemplate";
        public const string ProjectDashboardReportViewerPortletTemplateContentType = "ProjectDashboardReportViewerPortletTemplate";
    }
}