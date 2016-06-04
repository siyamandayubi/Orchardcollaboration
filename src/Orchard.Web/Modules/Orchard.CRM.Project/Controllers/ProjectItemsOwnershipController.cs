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

using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.PermissionProviders;
using Orchard.CRM.Core.Services;
using Orchard.Data;
using Orchard.Indexing;
using Orchard.Roles.Models;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Controllers
{
    [Themed]
    public class ProjectItemsOwnershipController : ContentOwnershipController
    {
        public ProjectItemsOwnershipController(
             IMasterDetailPermissionManager masterDetailPermissionManager,
             IBasicDataService basicDataService,
             IActivityStreamService activityStreamService,
             IWorkContextAccessor workContextAccessor,
             IIndexProvider indexProvider,
             IOrchardServices orchardServices,
             IWidgetService widgetService,
             ICRMContentOwnershipService contentOwnershipService,
             IContentOwnershipHelper contentOwnershipHelper,
             IRepository<UserRolesPartRecord> userRolesRepository,
             IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository) :
            base(masterDetailPermissionManager, basicDataService, activityStreamService, workContextAccessor, indexProvider, orchardServices, widgetService, contentOwnershipService, contentOwnershipHelper, userRolesRepository, permissionDetailRecordRepository)
        {
            this.DefaultDisplayType = "TitleAndMenu";
            this.ShowCustomers = true;
        }

        public override ActionResult Edit(int[] ids, string returnUrl)
        {
            if (ids == null)
            {
                ModelState.AddModelError("ids", T("ids parameter is required").Text);
                return this.View("PermissionError");
            }

            // This controller only work with one item
            if (ids.Length != 1)
            {
                ModelState.AddModelError("ids", T("Too many ids parameters").Text);
                return this.View("PermissionError");
            }

            return base.Edit(ids, returnUrl);
        }
    }
}