using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Security;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Notification.Services;
using Orchard.CRM.Core.ViewModels;

namespace Orchard.CRM.Notification.Controllers
{
    public class VisitController : Controller
    {
        private readonly IOrchardServices services;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IActivityNotificationService activityNotificationService;

        public VisitController(
            IOrchardServices services, 
            ICRMContentOwnershipService crmContentOwnershipService, 
            IActivityNotificationService activityNotificationService) {
            this.activityNotificationService = activityNotificationService;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        // GET: Visit
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult QueryCount()
        {
            int count = 0;

            if (services.WorkContext.CurrentUser != null &&
                (this.crmContentOwnershipService.IsCurrentUserCustomer() || this.crmContentOwnershipService.IsCurrentUserOperator()))
            {
                count = this.activityNotificationService.NewItemsCount();
            }

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true, Data = new {Count = count} };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult UpdateVisit(int activityStreamId)
        {
            if (services.WorkContext.CurrentUser == null)
            {
                throw new OrchardSecurityException(T("You don't have authorization to do the query"));
            }

            if (!this.crmContentOwnershipService.IsCurrentUserCustomer() && !this.crmContentOwnershipService.IsCurrentUserOperator())
            {
                throw new OrchardSecurityException(T("You don't have authorization to do the query"));
            }

            this.activityNotificationService.UpdateLastVisitActivity(activityStreamId);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }
    }
}