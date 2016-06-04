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

using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.CRM.Core;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using Orchard.Settings;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Controllers
{
    [Themed()]
    [ValidateInput(false)]
    public class ProjectController : BaseController
    {
        private readonly IExtendedProjectService projectService;
        public ProjectController(
            IExtendedProjectService projectService,
            IIndexProvider indexProvider,
            IContentOwnershipHelper contentOwnershipHelper,
            ICRMContentOwnershipService crmContentOwnershipService,
            IExtendedContentManager extendedContentManager,
            ITransactionManager transactionManager,
            IWidgetService widgetService,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IOrchardServices services,
            IDisplayHelperFactory displayHelperFactory,
            IBusinessUnitService businessUnitService,
            ISiteService siteService,
            IBasicDataService basicDataService,
            IContentDefinitionManager contentDefinitionManager,
            IIndexManager indexManager,
            IWorkContextAccessor workContextAccessor,
            IActivityStreamService activityStreamService,
            IContentItemDescriptorManager contentItemDescriptorManager)
            : base(ContentTypes.ProjectContentType, "Project_Edit", indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, activityStreamService, contentItemDescriptorManager)
        {
            this.projectService = projectService;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult DisplayProjectRelatedItem(string contentType, int projectId)
        {
            var project = this.projectService.GetProject(projectId);

                        if (project == null)
                return HttpNotFound();

            var contentItem = this.projectService.GetProjectRelatedItem(contentType, projectId);

            if (contentItem == null)
                return HttpNotFound();

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(project))
            {
                return HttpNotFound();
            }

            if (!this.IsDisplayAuthorized())
            {
                return new HttpUnauthorizedResult();
            }

            dynamic model = contentManager.BuildDisplay(contentItem);

            this.OnDisplay(contentItem, model);
            return this.CreateActionResultBasedOnAjaxRequest(model, "DisplayAjax");
        }

        protected override bool IsCreateAuthorized()
        {
            return this.crmContentOwnershipService.IsCurrentUserOperator() || this.crmContentOwnershipService.IsCurrentUserCustomer();
        }

        protected override bool IsDisplayAuthorized()
        {
            return this.services.Authorizer.Authorize(Permissions.OperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission) ||
                this.services.Authorizer.Authorize(Permissions.CustomerPermission);
        }
    }
}