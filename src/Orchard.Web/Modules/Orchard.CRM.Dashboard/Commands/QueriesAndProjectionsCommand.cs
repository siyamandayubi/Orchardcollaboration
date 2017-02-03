using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentPicker.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Dashboard.Models;
using Orchard.CRM.Dashboard.Services;
using Orchard.CRM.Project;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Dashboard.Commands
{
    public class QueriesAndProjectionsCommand : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWidgetsService _widgetsService;
        private readonly IQueriesAndProjectionsGenerator _queriesAndProjectionsGenerator;

        private const int ItemsPerSidebarPortlet = 5;

        public QueriesAndProjectionsCommand(
            IQueriesAndProjectionsGenerator queriesAndProjectionsGenerator,
            IWidgetsService widgetsService,
            IContentManager contentManager,
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            ISiteService siteService,
            IMenuService menuService,
            INavigationManager navigationManager)
        {
            _queriesAndProjectionsGenerator = queriesAndProjectionsGenerator;
            _widgetsService = widgetsService;
            _contentManager = contentManager;
            _membershipService = membershipService;
            _siteService = siteService;
            _menuService = menuService;
            _navigationManager = navigationManager;
            _authenticationService = authenticationService;
        }


        [CommandName("dashboard-create-queries")]
        [CommandHelp("dashboard-create-queries")]
        public void CreateQueries()
        {
            CreateOpenTicketsQueryAndProjection();
            CreateUnassignedTicketsQueryAndProjection();
            CreateReportedByMeTicketsQueryAndProjection();
            ClosedRecentlyTicketsQueryAndProjection();
            AddedRecentlyTicketsQueryAndProjection();
            AddedRecentlyDiscussionsQueryAndProjection();
            UpdaedRecentlyWikisQueryAndProjection();
            MyProjectsQueryAndProjection();
        }

        private void CreateOpenTicketsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.MyOpenTickets) != null)
            {
                return;
            }

            var query = _queriesAndProjectionsGenerator.CreateQuery(QueryNames.MyOpenTickets, "Ticket", "SidebarTickets", "Shape", "TitleOnly", true, false);

            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><NotEqual>true</NotEqual><StatusType_Id>{0}</StatusType_Id></Form>", StatusRecord.ClosedStatus);
            _queriesAndProjectionsGenerator.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state, query.Record.FilterGroups.First());

            // assigned to current user
            state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><AccessType>{0}</AccessType><Users>{1}</Users></Form>", ContentItemPermissionAccessTypes.Assignee, "{LoggedOnUser.Id}");
            _queriesAndProjectionsGenerator.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.CurrentUserPermissions, state, query.Record.FilterGroups.First());

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.MyOpenTickets, T(ProjectionNames.MyOpenTicketsProjectionTitle).Text, "Ticket", ItemsPerSidebarPortlet);
        }

        private void CreateReportedByMeTicketsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.ReportedByMeTickets) != null)
            {
                return;
            }

            var query = _queriesAndProjectionsGenerator.CreateQuery(QueryNames.ReportedByMeTickets, "Ticket", "SidebarTickets", "Shape", "TitleOnly", true, false);

            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><RequestingUser_Id>{0}</RequestingUser_Id></Form>", "{LoggedOnUser.Id}");
            _queriesAndProjectionsGenerator.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.RequestingUserType, state, query.Record.FilterGroups.First());

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.ReportedByMeTickets, T(ProjectionNames.ReportedByMeTicketsTitle).Text, "Ticket", ItemsPerSidebarPortlet);
        }

        private void CreateUnassignedTicketsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.UnassignedTickets) != null)
            {
                return;
            }

            var query = _queriesAndProjectionsGenerator.CreateQuery(QueryNames.UnassignedTickets, "Ticket", "SidebarTickets", "Shape", "TitleOnly", true, false);

            // assigned to current user
            string state = "<Form><Description></Description></Form>";
            _queriesAndProjectionsGenerator.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.UnassignedItems, state, query.Record.FilterGroups.First());

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.UnassignedTickets, T(ProjectionNames.UnassignedTicketsProjectionTitle).Text, "Ticket", ItemsPerSidebarPortlet);
        }

        private void ClosedRecentlyTicketsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.ClosedRecentlyTickets) != null)
            {
                return;
            }

            var query = _queriesAndProjectionsGenerator.CreateQuery(QueryNames.ClosedRecentlyTickets, "Ticket", "SidebarTickets", "Shape", "TitleOnly", true, true);

            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><NotEqual>false</NotEqual><StatusType_Id>{0}</StatusType_Id></Form>", StatusRecord.ClosedStatus);
            _queriesAndProjectionsGenerator.CreateFilter(TicketFieldsFilter.CategoryName, TicketFieldsFilter.StatusTypeFilter, state, query.Record.FilterGroups.First());

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.ClosedRecentlyTickets, T(ProjectionNames.ClosedRecentlyTicketsProjectionTitle).Text, "Ticket", ItemsPerSidebarPortlet);
        }

        private void AddedRecentlyTicketsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.AddedRecentlyTickets) != null)
            {
                return;
            }

            _queriesAndProjectionsGenerator.CreateQuery(QueryNames.AddedRecentlyTickets, "Ticket", "SidebarTickets", "Shape", "TitleOnly", true, true);

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.AddedRecentlyTickets, T(ProjectionNames.AddedRecentlyTicketsProjectionTitle).Text, "Ticket", ItemsPerSidebarPortlet);
        }

        private void AddedRecentlyDiscussionsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.AddedRecentlyDiscussions) != null)
            {
                return;
            }

            _queriesAndProjectionsGenerator.CreateQuery(QueryNames.AddedRecentlyDiscussions, ContentTypes.DiscussionContentType, "SidebarItems", "Shape", true, true);

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.AddedRecentlyDiscussions, T(ProjectionNames.AddedRecentlyDiscussionsProjectionTitle).Text, ContentTypes.DiscussionContentType, ItemsPerSidebarPortlet);
        }

        private void UpdaedRecentlyWikisQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.UpdatedRecentlyWikis) != null)
            {
                return;
            }

            _queriesAndProjectionsGenerator.CreateQuery(QueryNames.UpdatedRecentlyWikis, ContentTypes.WikiItemType, "SidebarItems", "Shape", true, true);

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.UpdatedRecentlyWikis, T(ProjectionNames.UpdatedRecentlyWikisProjectionTitle).Text, ContentTypes.ProjectWikiContentType, ItemsPerSidebarPortlet);
        }

        private void MyProjectsQueryAndProjection()
        {
            if (_queriesAndProjectionsGenerator.GetQuery(QueryNames.MyProjects) != null)
            {
                return;
            }

            var query = _queriesAndProjectionsGenerator.CreateQuery(QueryNames.MyProjects, ContentTypes.ProjectContentType, "SidebarItems", "Shape", "TitleOnly", true, false);

            // assigned to current user
            string state = string.Format(CultureInfo.InvariantCulture, "<Form><Description></Description><Users>{0}</Users></Form>", "{LoggedOnUser.Id}");
            _queriesAndProjectionsGenerator.CreateFilter(ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.CurrentUserPermissions, state, query.Record.FilterGroups.First());

            _queriesAndProjectionsGenerator.CreateProjection(Consts.SidebarProjectionPortletTemplateType, QueryNames.MyProjects, T(ProjectionNames.MyProjectsProjectionTitle).Text, ContentTypes.ProjectContentType, ItemsPerSidebarPortlet);
        }
    }
}