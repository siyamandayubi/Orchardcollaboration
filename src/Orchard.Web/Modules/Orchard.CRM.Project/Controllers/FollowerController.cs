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
using Orchard.Security;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Controllers
{
    public class FollowerController : BaseController
    {
        public FollowerController(
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
            : base(ContentTypes.ProjectContentType, "Followers_Edit", indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, activityStreamService, contentItemDescriptorManager)
        {
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Follow(int contentItemId, string returnUrl)
        {
            // check user is operator or customer
            if (!this.crmContentOwnershipService.IsCurrentUserOperator() &&
                !this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                return HttpNotFound();
            }

            var contentItem = this.services.ContentManager.Get(contentItemId);

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(contentItem))
            {
                ModelState.AddModelError("contentItemId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var followerPart = contentItem.As<FollowerPart>();

            if (followerPart == null)
            {
                return HttpNotFound();
            }

            string currentUserId = this.services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);

            string followers = followerPart.Followers;
            followers = string.IsNullOrEmpty(followers) ?
                currentUserId :
                string.Format("{0}, {1}", followers, currentUserId);

            followerPart.Followers = followers;

            dynamic model = new ExpandoObject();
            model.Message = T("Set as followd").Text;
            return this.CreateActionResultBasedOnAjaxRequest(model, null, null, returnUrl);
        }

        public ActionResult UnFollow(int contentItemId, string returnUrl)
        {
            // check user is operator or customer
            if (!this.crmContentOwnershipService.IsCurrentUserOperator() &&
                !this.crmContentOwnershipService.IsCurrentUserCustomer())
            {
                return HttpNotFound();
            }

            var contentItem = this.services.ContentManager.Get(contentItemId);

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(contentItem))
            {
                ModelState.AddModelError("contentItemId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var followerPart = contentItem.As<FollowerPart>();

            if (followerPart == null)
            {
                return HttpNotFound();
            }

            string currentUserId = this.services.WorkContext.CurrentUser.Id.ToString(CultureInfo.InvariantCulture);

            string followersString = followerPart.Followers;

            if (followersString != null)
            {
                var followers = followersString.Split(',').Where(c => c != currentUserId);
                followerPart.Followers = string.Join(",", followers.ToArray());
            }

            dynamic model = new ExpandoObject();
            model.Message = T("Set as un-followd").Text;
            return this.CreateActionResultBasedOnAjaxRequest(model, null, null, returnUrl);
        }


        protected override bool IsCreateAuthorized()
        {
            throw new NotImplementedException();
        }

        protected override bool IsDisplayAuthorized()
        {
            throw new NotImplementedException();
        }
    }
}