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
        public const string WikiContentType = "ProjectWiki";
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