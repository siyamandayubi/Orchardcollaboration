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
    using Newtonsoft.Json;

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

            if (!this.contentOwnershipService.IsCurrentUserOperator() && !this.contentOwnershipService.CurrentUserCanViewContent(contentItem))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            if (!ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = new AjaxMessageViewModel { IsDone = true };
                CRMHelper.AddModelStateErrors(this.ModelState, errorModel);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            model.UserId = this.sercices.WorkContext.CurrentUser.Id;
            this.timeTrackingService.Add(model);

            model.FullUsername = Orchard.CRM.Core.Services.CRMHelper.GetFullNameOfUser(this.sercices.WorkContext.CurrentUser);

            model.UserCanEdit = true;

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { Data = JsonConvert.SerializeObject(model), IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(TimeTrackingViewModel model)
        {
            var contentItem = this.sercices.ContentManager.Get(model.ContentItemId);

            var currentUser = this.sercices.WorkContext.CurrentUser;
            if (currentUser == null)
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            if (!this.contentOwnershipService.IsCurrentUserAdvanceOperator() && currentUser.Id != model.UserId)
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            if (model.UserId == default(int))
            {
                ModelState.AddModelError("UserId", T("UserId is not provided").Text);
            }

            if (!ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = new AjaxMessageViewModel { IsDone = true };
                CRMHelper.AddModelStateErrors(this.ModelState, errorModel);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            model.UserCanEdit = this.contentOwnershipService.IsCurrentUserAdvanceOperator() || model.UserId == currentUser.Id;
            model.FullUsername = Orchard.CRM.Core.Services.CRMHelper.GetFullNameOfUser(this.sercices.WorkContext.CurrentUser);

            this.timeTrackingService.Edit(model);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { Data = JsonConvert.SerializeObject(model), IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int timeTrackId, int contentItemId)
        {
            var contentItem = this.sercices.ContentManager.Get(contentItemId);

            var currentUser = this.sercices.WorkContext.CurrentUser;
            if (currentUser == null)
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            var timeTrackingItem = this.timeTrackingService.GetTimeTrackingItem(timeTrackId);

            if(timeTrackingItem == null)
            {
                throw new System.Data.RowNotInTableException();
            }

            if (!this.contentOwnershipService.IsCurrentUserAdvanceOperator() && currentUser.Id != timeTrackingItem.UserId)
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to do this operation"));
            }

            this.timeTrackingService.Delete(timeTrackId);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }
    }
}