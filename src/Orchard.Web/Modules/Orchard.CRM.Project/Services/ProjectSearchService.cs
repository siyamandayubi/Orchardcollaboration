using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
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

namespace Orchard.CRM.Project.Services
{
    public class ProjectSearchService : IProjectSearchService
    {
        private readonly IIndexManager indexManager;
        private readonly IOrchardServices services;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly ISiteService siteService;
        private readonly IProjectService projectService;
        private readonly IBasicDataService basicDataService;
        private readonly ISearchTicketService searchTicketService;

        public ProjectSearchService(
            ISearchTicketService searchTicketService,
            IProjectService projectService,
            IIndexManager indexManager,
            IBasicDataService basicDataService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IOrchardServices services,
            ISiteService siteService)
        {
            this.searchTicketService = searchTicketService;
            this.projectService = projectService;
            this.basicDataService = basicDataService;
            this.siteService = siteService;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.services = services;
            this.indexManager = indexManager;
            this.Logger = NullLogger.Instance;
            this.T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IPageOfItems<IContent> Query(PagerParametersWithSortFields pagerParameters, string searchPhrase, int? projectId, string[] contentTypes, string[] searchFields)
        {
            Pager pager = new Pager(siteService.GetSiteSettings(), pagerParameters);

            List<string> queries = new List<string>();
            List<string> fields = new List<string>();

            var builder = indexManager.GetSearchIndexProvider().CreateSearchBuilder(TicketController.SearchIndexName);

            // Type
            var typeQuery = string.Join(" OR ", contentTypes.Select(c => string.Format(CultureInfo.InvariantCulture, "type:\"{0}\"", c)).ToArray());
            queries.Add(typeQuery);
            fields.Add("type");


            // Term
            if (!String.IsNullOrWhiteSpace(searchPhrase))
            {
                string searchText = searchPhrase.Replace("\"", " ");
                foreach (var character in searchText.ToArray())
                {
                    if (!char.IsLetterOrDigit(character))
                    {
                        searchText = searchText.Replace(character, ' ');
                    }
                }

                searchText = searchText.Trim();
                searchPhrase = searchText;

                if (!string.IsNullOrEmpty(searchText))
                {
                    var temp = searchFields.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}:\"{1}*\"", c, searchText));
                    string query = string.Join(" OR ", temp.ToArray());
                    queries.Add(query);
                    fields.AddRange(searchFields);
                }
            }

            // restrict based on the project
            if (projectId.HasValue)
            {
                var projectIds = this.projectService
                    .GetProjects(null)
                    .AsPart<ProjectPart>()
                    .Select(c => c.Record.Id)
                    .ToList();

                if (projectIds.Any(c => c == projectId.Value))
                {
                    string query = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", AttachToProjectPart.ProjectIdFieldName, projectId.Value.ToString(CultureInfo.InvariantCulture));
                    queries.Add(query);
                    fields.Add(AttachToProjectPart.ProjectIdFieldName);
                }
            }

            string permissionsQuery = this.searchTicketService.CreateLucenePermissionQuery(new PostedTicketSearchViewModel { Users = new int[] { }, BusinessUnits = new int[] { }, IncludeAllVisibleItemsBySelectedGroupsAndUsers = true });

            if (!string.IsNullOrEmpty(permissionsQuery))
            {
                queries.Add(permissionsQuery);
                fields.Add(ContentItemPermissionPart.PermissionsSearchFieldName);
                fields.Add(ContentItemPermissionPart.OwnerSearchFieldName);
            }

            string mainQuery = string.Join(" AND ", queries.Select(c => "(" + c + ")").ToArray());
            builder.Parse(new[] { "type" }, mainQuery, false);
            builder.ExactMatch();
            int count = 0;

            int[] foundIds = new int[0];

            try
            {
                count = builder.Count();
                builder = builder.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize);
                var searchResults = builder.Search();

                foundIds = searchResults.Select(searchHit => searchHit.ContentItemId).ToArray();
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
            }

            var contentItems = this.services.ContentManager.GetMany<IContent>(foundIds, VersionOptions.Published, QueryHints.Empty);

            IPageOfItems<IContent> returnVaue = new PageOfItems<IContent>(contentItems);
            returnVaue.TotalItemCount = count;
            return returnVaue;
        }
    }
}