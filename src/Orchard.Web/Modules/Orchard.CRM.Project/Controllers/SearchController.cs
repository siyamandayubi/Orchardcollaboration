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

using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Controllers
{
    [ValidateInput(false), Themed]
    public class SearchController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IProjectService projectService;
        private readonly IProjectSearchService _projectSearchService;
        private readonly ICRMContentOwnershipService contentOwnershipService;

        private Dictionary<string, string[]> searchFieldsBasedOnContentType = new Dictionary<string, string[]>();

        public SearchController(
            IProjectService projectService,
            IContentDefinitionManager contentDefinitionManager,
            ICRMContentOwnershipService contentOwnershipService,
            IProjectSearchService projectSearchService,
            IOrchardServices services,
            IContentManager contentManager,
            ISiteService siteService,
            IShapeFactory shapeFactory)
        {
            this.contentDefinitionManager = contentDefinitionManager;
            this.projectService = projectService;
            this.contentOwnershipService = contentOwnershipService;
            Services = services;
            _projectSearchService = projectSearchService;
            _contentManager = contentManager;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;

            searchFieldsBasedOnContentType.Add("Ticket", new[] { CRMCommentsPart.CommentsFieldName, TicketPart.DescriptionFieldName, TicketPart.TitleFieldName });
            searchFieldsBasedOnContentType.Add(ContentTypes.DiscussionContentType, new[] { CRMCommentsPart.CommentsFieldName, FieldNames.BodyFieldName, FieldNames.TitleFieldName });
            searchFieldsBasedOnContentType.Add(ContentTypes.WikiItemType, new[] { CRMCommentsPart.CommentsFieldName, FieldNames.BodyFieldName, FieldNames.TitleFieldName });
            searchFieldsBasedOnContentType.Add(ContentTypes.FolderContentType, new[] { CRMCommentsPart.CommentsFieldName, FieldNames.TitleFieldName });
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(PagerParametersWithSortFields pagerParameters, string searchPhrase, string contentTypes, int? projectId)
        {
            if (!this.contentOwnershipService.IsCurrentUserCustomer() &&
                !this.contentOwnershipService.IsCurrentUserOperator())
            {
                return new HttpUnauthorizedResult();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var matchedContentTypes = string.IsNullOrEmpty(contentTypes) ?
                contentTypes.Split(',').Where(c => this.searchFieldsBasedOnContentType.Any(d => d.Key == c)) :
                this.searchFieldsBasedOnContentType.Keys;

            if (matchedContentTypes.Count() == 0)
            {
                matchedContentTypes = this.searchFieldsBasedOnContentType.Keys;
            }

            var fields = this.searchFieldsBasedOnContentType
                .Where(c => matchedContentTypes.Any(d => d == c.Key))
                .SelectMany(d => d.Value)
                .Distinct()
                .ToArray();

            IPageOfItems<IContent> searchHits = null;
            try
            {
                searchHits = _projectSearchService.Query(pagerParameters, searchPhrase, projectId, matchedContentTypes.ToArray(), fields);
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
            }

            var list = Shape.List();

            foreach (var contentItem in searchHits)
            {
                var contentType = contentDefinitionManager.GetTypeDefinition(contentItem.ContentItem.ContentType);
                dynamic model = new ExpandoObject();
                model.Shape = _contentManager.BuildDisplay(contentItem, "Summary");
                model.ContentTypeDisplayName = contentType.DisplayName;
                list.Add(model);
            }

            

            var pagerShape = Shape.Pager(pager).TotalItemCount(searchHits.TotalItemCount);
            var searchViewModel = new ProjectSearchViewModel
            {
                Query = searchPhrase,
                TotalItemCount = searchHits.TotalItemCount,
                StartPosition = (pager.Page - 1) * pager.PageSize + 1,
                EndPosition = pager.Page * pager.PageSize > searchHits.TotalItemCount ? searchHits.TotalItemCount : pager.Page * pager.PageSize,
                ContentItems = list,
                ProjectId = projectId,
                ContentTypes = contentTypes,
                Pager = pagerShape
            };

            if (projectId.HasValue)
            {
                var project = this.projectService.GetProject(projectId.Value);
                searchViewModel.ProjectName = project != null ? project.Title : string.Empty;
            }

            return View(searchViewModel);
        }
    }
}