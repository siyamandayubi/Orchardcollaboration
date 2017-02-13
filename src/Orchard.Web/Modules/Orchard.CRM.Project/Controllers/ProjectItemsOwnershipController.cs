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