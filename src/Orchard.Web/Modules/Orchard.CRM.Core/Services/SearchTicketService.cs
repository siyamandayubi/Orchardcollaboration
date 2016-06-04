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

using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.ViewModels;
using Orchard.Data;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Core.Services
{
    public class SearchTicketService : ISearchTicketService
    {
        private readonly IIndexManager indexManager;
        private readonly IOrchardServices services;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly ISiteService siteService;
        private readonly IProjectService projectService;
        private readonly IBasicDataService basicDataService;
        private readonly IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort;

        public SearchTicketService(
            IProjectService projectService,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort,
            IIndexManager indexManager,
            IBasicDataService basicDataService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IOrchardServices services,
            ISiteService siteService)
        {
            this.projectService = projectService;
            this.basicDataService = basicDataService;
            this.projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            this.siteService = siteService;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.services = services;
            this.indexManager = indexManager;
            this.Logger = NullLogger.Instance;
            this.T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public int CountByIndexProvider(PostedTicketSearchViewModel searchModel)
        {
            var builder = this.CreateLuceneBuilder(searchModel);
            builder.ExactMatch();

            try
            {
                builder.ExactMatch();
                return builder.Count();
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                throw exception;
            }
        }

        public int CountByDatabase(PostedTicketSearchViewModel searchModel)
        {
            var query = this.CreateQuery(searchModel);

            return query.Count();
        }

        public IContent[] SearchByDatabase(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            Pager pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var contentQuery = this.CreateQuery(searchModel);

            // apply sort
            if (!string.IsNullOrEmpty(pagerParameters.SortField))
            {
                string type = pagerParameters.SortField ?? TicketPart.IdentityFieldName;
                string category = "Ticket";

                dynamic sortState = new { Sort = !pagerParameters.Descending };

                contentQuery = this.projectionManagerWithDynamicSort.AddSortCriterion(category, type, sortState, contentQuery);
            }

            contentQuery = contentQuery.Include(new[] { "TicketPartRecord", "ContentItemPermissionPartRecord" });
            IEnumerable<ContentItem> contentItems = pager.PageSize == 0 ?
                contentQuery.List() :
                contentQuery.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize);

            return contentItems.ToArray();
        }

        public IContent[] SearchByIndexProvider(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            Pager pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var builder = this.CreateLuceneBuilder(searchModel);

            // apply sort
            if (!string.IsNullOrEmpty(pagerParameters.SortField))
            {
                string[] integerFields = new string[] { TicketPart.IdentityFieldName, TicketPart.PriorityFieldName, TicketPart.ServiceFieldName, TicketPart.StatusFieldName, TicketPart.TypeFieldName };

                switch (pagerParameters.SortField)
                {
                    case TicketPart.IdentityFieldName:
                    case TicketPart.ServiceFieldName:
                    case TicketPart.TypeFieldName:
                        builder.SortByInteger(pagerParameters.SortField);
                        break;
                    case TicketPart.DueDateFieldName:
                        builder.SortByDateTime(pagerParameters.SortField);
                        break;
                    case TicketPart.PriorityFieldName:
                    case TicketPart.PriorityOrderFieldName:
                        builder.SortByInteger(TicketPart.PriorityOrderFieldName);
                        break;
                    case TicketPart.StatusOrderFieldName:
                    case TicketPart.StatusFieldName:
                        builder.SortByInteger(TicketPart.StatusOrderFieldName);
                        break;
                    default:
                        builder.SortBy(pagerParameters.SortField);
                        break;
                }

                if (!pagerParameters.Descending)
                {
                    builder.Ascending();
                }
            }

            int[] foundIds = new int[0];

            try
            {
                builder.ExactMatch();
                builder = builder.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize);
                var searchResults = builder.Search();

                foundIds = searchResults.Select(searchHit => searchHit.ContentItemId).ToArray();
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
            }

            var includedPartRecords = new[] { "TicketPartRecord", "ContentItemPermissionPartRecord" };
            var contentItems = this.services.ContentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints().ExpandRecords(includedPartRecords));

            return contentItems.ToArray();
        }

        public IHqlQuery CreateQuery(PostedTicketSearchViewModel searchModel)
        {
            var contentQuery = this.services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);

            dynamic state = new JObject();

            if (this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                // filter by items created by current user
                state.RequestingUser_Id = this.services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Ticket", "RequestingUser", state);
            }

            // Ticket contentType
            state.ContentTypes = "Ticket";
            contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

            // RelatedContentItem
            if (searchModel.RelatedContentItemId.HasValue)
            {
                state.RelatedContentItemId = searchModel.RelatedContentItemId.Value.ToString(CultureInfo.InvariantCulture);
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.RelatedContentItem, state);
            }

            // restrict based on the project
            if (this.projectService.IsTicketsRelatedToProjects())
            {
                var projectIds = this.projectService
                    .GetProjects(null)
                    .AsPart<ProjectPart>()
                    .Select(c => c.Record.Id)
                    .ToList();

                if (searchModel.ProjectId.HasValue && projectIds.Any(c => c == searchModel.ProjectId.Value))
                {
                    state.Project_Id = searchModel.ProjectId.Value.ToString(CultureInfo.InvariantCulture);
                    contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state);
                }
            }

            // Ticket status
            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                state.Status_Id = searchModel.Status;
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Ticket", "Status", state);
            }
            else if (searchModel.UnStatus)
            {
                state.UnStatus = true;
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Ticket", "Status", state);
            }

            // Due Date
            if (!string.IsNullOrEmpty(searchModel.DueDate))
            {
                state = new JObject();
                if (searchModel.DueDate == PostedTicketSearchViewModel.OverDueDate)
                {
                    state.MaxDueDate = DateTime.UtcNow.Date;

                    // we are not consider closed overdue items
                    var statusRecords = this.basicDataService.GetStatusRecords().ToList();
                    var closedStatus = statusRecords.FirstOrDefault(c => c.StatusTypeId == StatusRecord.ClosedStatus);
                    if (searchModel.Status != closedStatus.Id.ToString(CultureInfo.InvariantCulture))
                    {
                        state.State_Id = closedStatus.Id;
                        contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Ticket", "NotEqualStatus", state);
                    }
                }
                else
                {
                    DateTime maxDateTime;
                    if (DateTime.TryParse(searchModel.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out maxDateTime))
                    {
                        state.MaxDueDate = maxDateTime;
                    }

                    if (maxDateTime > DateTime.UtcNow.Date)
                    {
                        state.MinDueDate = DateTime.UtcNow.Date;
                    }
                }

                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Ticket", "TicketDueDate", state);
            }

            // TeamIds of the search
            var teams = new List<int>();

            // BusinessUnitIds of the search
            List<int> businessUnits = searchModel.BusinessUnits != null ? searchModel.BusinessUnits.ToList() : new List<int>();

            // Ids of the searched users
            List<int> users = searchModel.Users != null ? searchModel.Users.ToList() : new List<int>();

            if (searchModel.Unassigned)
            {
                dynamic temp = new JObject();
                contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.UnassignedItems, temp);
            }
            else
            {
                // restrict the list for none admin users
                if (!this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
                {
                    int? accessType = ContentItemPermissionAccessTypes.Assignee;
                    bool allItemsUserCanSee;
                    this.RestrictListsBasedOnCurrentUserAccess(ref teams, ref users, ref businessUnits, out allItemsUserCanSee, searchModel);

                    // if allItemsUserCanSee then the accessType is useless and the result must include all of the items that user can see
                    if (allItemsUserCanSee)
                    {
                        accessType = null;
                    }

                    dynamic permissionsState = new
                    {
                        Users = users,
                        Teams = teams,
                        BusinessUnits = businessUnits,
                        AccessType = accessType
                    };

                    contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, permissionsState);
                }
                else if (teams.Count > 0 || businessUnits.Count > 0 || users.Count > 0)
                {
                    int? accessType = searchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers ? null : (int?)ContentItemPermissionAccessTypes.Assignee;
                    dynamic permissionsState = new
                    {
                        Users = users,
                        Teams = teams,
                        BusinessUnits = businessUnits,
                        AccessType = accessType
                    };

                    contentQuery = this.projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, permissionsState);
                }
            }

            return contentQuery;
        }

        public string CreateLucenePermissionQuery(PostedTicketSearchViewModel searchModel)
        {
            // TeamIds of the search
            var teams = new List<int>();

            // BusinessUnitIds of the search
            List<int> businessUnits = searchModel.BusinessUnits != null ? searchModel.BusinessUnits.ToList() : new List<int>();

            // Ids of the searched users
            List<int> users = searchModel.Users != null ? searchModel.Users.ToList() : new List<int>();

            bool allItemsUserCanSee;
            this.RestrictListsBasedOnCurrentUserAccess(ref teams, ref users, ref businessUnits, out allItemsUserCanSee, searchModel);

            string searchField = ContentItemPermissionPart.OwnerSearchFieldName;
            if (!this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission) && allItemsUserCanSee)
            {
                searchField = ContentItemPermissionPart.PermissionsSearchFieldName;
            }
            else
            {
                if (teams.Count == 0 && users.Count == 0 && businessUnits.Count == 0)
                {
                    return string.Empty;
                }

                if (searchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers)
                {
                    searchField = ContentItemPermissionPart.PermissionsSearchFieldName;
                }
            }

            // users
            List<string> searchItems = users.Select(c =>
                string.Format(
                CultureInfo.InvariantCulture,
                "{0}:\"U{1}\"",
                searchField,
                c.ToString(CultureInfo.InvariantCulture))).ToList();

            // business Units
            if (businessUnits.Count > 0)
            {
                var businessUnitSearchTerms = businessUnits.Select(c =>
                        string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:\"B{1}\"",
                        searchField,
                        c.ToString(CultureInfo.InvariantCulture))).ToList();

                searchItems = searchItems.Union(businessUnitSearchTerms).ToList();
            }

            // Teams
            if (teams.Count > 0)
            {
                var teamSearchTerms = teams.Select(c =>
                        string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:\"T{1}\"",
                        searchField,
                        c.ToString(CultureInfo.InvariantCulture))).ToList();

                searchItems = searchItems.Union(teamSearchTerms).ToList();
            }

            return string.Join(" OR ", searchItems);
        }

        private void RestrictListsBasedOnCurrentUserAccess(ref List<int> teams, ref List<int> users, ref List<int> businessUnits, out bool allItemsUserCanSee, PostedTicketSearchViewModel searchModel)
        {
            int userId = this.services.WorkContext.CurrentUser.Id;
            allItemsUserCanSee = false;
            if (searchModel.Unassigned)
            {
                searchModel.Users = new int[] { };
                searchModel.BusinessUnits = new int[] { };

                users.Clear();
                teams.Clear();
                businessUnits.Clear();

                return;
            }

            if (!this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission))
            {
                var userBusinessUnits = this.basicDataService
                    .GetBusinessUnitMembers()
                    .Where(c => c.UserPartRecord.Id == userId);

                var userTeams = this.basicDataService
                    .GetTeamMembers()
                    .Where(c => c.UserPartRecord.Id == userId);

                // Restrict to teams and businessUnits of the user
                teams = teams.Where(c => userTeams.Count(d => d.TeamPartRecord.Id == c) > 0).ToList();
                businessUnits = businessUnits.Where(c => userBusinessUnits.Count(d => d.BusinessUnitPartRecord.Id == c) > 0).ToList();
                users = users.Where(c => c == userId).ToList();

                if (teams.Count == 0 && businessUnits.Count == 0 && users.Count == 0)
                {
                    allItemsUserCanSee = true;
                    users.Add(userId);
                    users = users.Distinct().ToList();
                    var userTeamIds = userTeams.Select(c => c.TeamPartRecord.Id);
                    teams.AddRange(userTeamIds);
                    teams = teams.Distinct().ToList();

                    var userBusinessUnitIds = userBusinessUnits.Select(c => c.BusinessUnitPartRecord.Id);
                    businessUnits.AddRange(userBusinessUnitIds);
                    businessUnits = businessUnits.Distinct().ToList();
                }
            }
        }

        private ISearchBuilder CreateLuceneBuilder(PostedTicketSearchViewModel searchModel)
        {
            List<string> queries = new List<string>();
            List<string> fields = new List<string>();

            var builder = indexManager.GetSearchIndexProvider().CreateSearchBuilder(TicketController.SearchIndexName);

            // Ticket type
            queries.Add("type:\"ticket\"");
            fields.Add("type");

            if (this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                // filter by items created by current user
                string userId = this.services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);
                string query = string.Format(CultureInfo.InvariantCulture, "{0}:\"{1}\"", TicketPart.RequestingUserFieldName, userId);
                queries.Add(query);
                fields.Add(TicketPart.RequestingUserFieldName);
            }

            // Term
            if (!String.IsNullOrWhiteSpace(searchModel.Term))
            {
                string searchText = searchModel.Term.Replace("\"", " ");
                foreach (var character in searchText.ToArray())
                {
                    if (!char.IsLetterOrDigit(character))
                    {
                        searchText = searchText.Replace(character, ' ');
                    }
                }

                searchText = searchText.Trim();
                searchModel.Term = searchText;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string query = string.Format(CultureInfo.InvariantCulture, "{0}:\"{1}*\" OR {2}:\"{1}*\" OR {3}:\"{1}*\" OR {4}:{1}", TicketPart.DescriptionFieldName, searchText, TicketPart.TitleFieldName, CRMCommentsPart.CommentsFieldName, TicketPart.IdentityFieldName);
                    queries.Add(query);
                    fields.AddRange(new string[] { TicketPart.DescriptionFieldName, TicketPart.TitleFieldName });
                }
            }

            // Due date
            if (!string.IsNullOrEmpty(searchModel.DueDate))
            {
                string maxDueDate = string.Empty;
                string minDueDate = string.Empty;

                if (searchModel.DueDate != PostedTicketSearchViewModel.OverDueDate)
                {
                    minDueDate = "19000101000000";
                    DateTime maxDateTime;
                    if (DateTime.TryParse(searchModel.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out maxDateTime))
                    {
                        maxDueDate = maxDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "235959";
                    }

                    if (maxDateTime > DateTime.UtcNow.Date)
                    {
                        minDueDate = DateTime.UtcNow.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "000000";
                    }
                }
                else
                {
                    maxDueDate = DateTime.UtcNow.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "235959"; ;
                    minDueDate = "19000101000000";
                }

                string query = string.Format(CultureInfo.InvariantCulture, "{0}:[{1} TO {2}]", TicketPart.DueDateFieldName, minDueDate, maxDueDate);
                queries.Add(query);
                fields.Add(TicketPart.DueDateFieldName);
            }

            // Ticket status
            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                string status = searchModel.Status.Replace("\"", " ").Trim();
                if (!string.IsNullOrEmpty(status))
                {
                    string query = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", TicketPart.StatusFieldName, status);
                    queries.Add(query);
                    fields.Add(TicketPart.StatusFieldName);
                }
            }
            else if (searchModel.UnStatus)
            {
                string query = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", TicketPart.StatusFieldName, TicketPart.NullValueForIntegers);
                queries.Add(query);
                fields.Add(TicketPart.StatusFieldName);
            }

            // restrict based on the project
            if (this.projectService.IsTicketsRelatedToProjects())
            {
                var projectIds = this.projectService
                    .GetProjects(null)
                    .AsPart<ProjectPart>()
                    .Select(c => c.Record.Id)
                    .ToList();

                if (searchModel.ProjectId.HasValue && projectIds.Any(c => c == searchModel.ProjectId.Value))
                {
                    string query = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", AttachToProjectPart.ProjectIdFieldName, searchModel.ProjectId.Value.ToString(CultureInfo.InvariantCulture));
                    queries.Add(query);
                    fields.Add(AttachToProjectPart.ProjectIdFieldName);
                }
            }

            // Permissions Query
            if (searchModel.Unassigned)
            {
                queries.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:\"{1}\"",
                        ContentItemPermissionPart.PermissionsSearchFieldName,
                        ContentItemPermissionPart.EmptyPermissionSearchFieldName
                    ));
                fields.Add(ContentItemPermissionPart.PermissionsSearchFieldName);
            }
            else
            {
                string permissionsQuery = this.CreateLucenePermissionQuery(searchModel);

                if (!string.IsNullOrEmpty(permissionsQuery))
                {
                    queries.Add(permissionsQuery);
                    fields.Add(ContentItemPermissionPart.PermissionsSearchFieldName);
                    fields.Add(ContentItemPermissionPart.OwnerSearchFieldName);
                }
            }

            string mainQuery = string.Join(" AND ", queries.Select(c => "(" + c + ")").ToArray());
            builder.Parse(fields.ToArray(), mainQuery, false);

            return builder;
        }
    }
}