namespace Orchard.CRM.TimeTracking.Controllers
{
    using Core.Services;
    using Core.ViewModels;
    using Localization;
    using Services;
    using System;
    using System.Web.Mvc;
    using System.Linq;
    using ViewModels;
    public class ItemController : Controller
    {
        private readonly ITimeTrackingService timeTrackingService;
        private readonly IOrchardServices sercices;
        private readonly ICRMContentOwnershipService contentOwnershipService;

        public ItemController(ITimeTrackingService timeTrackingService, ICRMContentOwnershipService contentOwnershipService, IOrchardServices sercices)
        {
            this.sercices = sercices;
            this.contentOwnershipService = contentOwnershipService;
            this.timeTrackingService = timeTrackingService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Add(TimeTrackingViewModel model)
        {
            var contentItem = this.sercices.ContentManager.Get(model.ContentItemId);

            if (!this.contentOwnershipService.IsCurrentUserOperator() || !this.contentOwnershipService.CurrentUserCanViewContent(contentItem))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            if (!ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = new AjaxMessageViewModel { IsDone = true };
                CRMHelper.AddModelStateErrors(this.ModelState, errorModel);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            this.timeTrackingService.Add(model);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(TimeTrackingViewModel model)
        {
            var contentItem = this.sercices.ContentManager.Get(model.ContentItemId);

            if (!this.contentOwnershipService.IsCurrentUserAdvanceOperator() || !this.contentOwnershipService.CurrentUserIsContentItemAssignee(contentItem))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            if (!ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = new AjaxMessageViewModel { IsDone = true };
                CRMHelper.AddModelStateErrors(this.ModelState, errorModel);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            this.timeTrackingService.Edit(model);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int timeTrackId, int contentItemId)
        {
            var contentItem = this.sercices.ContentManager.Get(contentItemId);

            if (!this.contentOwnershipService.IsCurrentUserAdvanceOperator() || !this.contentOwnershipService.CurrentUserIsContentItemAssignee(contentItem))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            this.timeTrackingService.Delete(timeTrackId);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }
    }
}