namespace Orchard.CRM.Core.Controllers
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.DisplayManagement;
    using Orchard.DisplayManagement.Shapes;
    using Orchard.Indexing;
    using Orchard.Localization;
    using Orchard.Mvc.ViewEngines.ThemeAwareness;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class CRMCommentController : Controller, IUpdateModel
    {
        private readonly IContentManager contentManager;
        private readonly IOrchardServices services;
        private readonly IThemeAwareViewEngine themeAwareViewEngine;
        private readonly IDisplayHelperFactory displayHelperFactory;
        private readonly ICRMContentOwnershipService contentOwnershipService;
        private readonly IIndexProvider indexProvider;
        private readonly IActivityStreamService activityStreamService;
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public CRMCommentController(
            IContentItemDescriptorManager contentItemDescriptorManager,
            IActivityStreamService activityStreamService,
            ICRMContentOwnershipService contentOwnershipService,
            IContentManager contentManager,
            IOrchardServices services,
            IThemeAwareViewEngine themeAwareViewEngine,
            IShapeFactory shapeFactory,
            IDisplayHelperFactory displayHelperFactory,
            IIndexProvider indexProvider)
        {
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.activityStreamService = activityStreamService;
            this.indexProvider = indexProvider;
            this.contentOwnershipService = contentOwnershipService;
            this.themeAwareViewEngine = themeAwareViewEngine;
            this.contentManager = contentManager;
            this.services = services;
            this.displayHelperFactory = displayHelperFactory;
            this.Shape = shapeFactory;
            this.T = NullLocalizer.Instance;
        }

        public dynamic Shape { get; set; }
        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Add(int contentId, string comment, string returnUrl)
        {
            var contentItem = this.contentManager.Get(contentId);

            var commentsPart = contentItem.As<CRMCommentsPart>();

            if (commentsPart == null)
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.contentOwnershipService.CurrentUserCanViewContent(contentItem))
            {
                return new HttpUnauthorizedResult();
            }

            int userId = this.services.WorkContext.CurrentUser.Id;

            // create new comment part
            var crmCommentItem = this.contentManager.New("CRMComment");
            this.contentManager.Create(crmCommentItem, VersionOptions.Draft);
            var crmCommentPart = crmCommentItem.As<CRMCommentPart>();
            crmCommentPart.Record.CommentText = comment;
            crmCommentPart.Record.CRMCommentsPartRecord = commentsPart.Record;
            crmCommentPart.Record.User = new Users.Models.UserPartRecord { Id = userId };
            dynamic model = this.contentManager.UpdateEditor(crmCommentItem, this);

            this.contentManager.Publish(crmCommentItem);

            // Add to activity stream
            string contentDescription = this.contentItemDescriptorManager.GetDescription(contentItem);
            contentDescription = this.T("commented on the '{0}'", contentDescription).Text;
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("action", "Display");
            routeValueDictionary.Add("controller", "Item");
            routeValueDictionary.Add("area", "Orchard.CRM.Core");
            routeValueDictionary.Add("id", contentItem.Id);
            this.activityStreamService.WriteChangesToStreamActivity(userId, contentItem.Id, contentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, contentDescription, routeValueDictionary);

            var documentIndex = this.indexProvider.New(contentItem.Id);
            this.contentManager.Index(contentItem, documentIndex);
            this.indexProvider.Store(TicketController.SearchIndexName, documentIndex);

            bool isAjaxRequest = Request.IsAjaxRequest();

            if (isAjaxRequest)
            {
                return this.Json(this.CreateAjaxMessageModel(crmCommentItem, string.Empty), JsonRequestBehavior.AllowGet);
            }
            else if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else if (contentItem.TypeDefinition.Name == "Ticket")
            {
                return this.RedirectToAction("Edit", "Ticket", new { id = contentId, area = "Orchard.CRM.Core" });
            }
            else if (contentItem.TypeDefinition.Name == "Request")
            {
                return this.RedirectToAction("Edit", "Request", new { id = contentId, area = "Orchard.CRM.Core" });
            }
            else
            {
                return this.RedirectToAction("Edit", "Item", new { area = "Orchard.CRM.Core", id = contentId });
            }
        }

        private AjaxMessageViewModel CreateAjaxMessageModel(ContentItem contentItem, string returnedShape)
        {
            AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { Id = contentItem.Id, IsDone = true, Data = Newtonsoft.Json.JsonConvert.SerializeObject(contentItem) };

            if (!string.IsNullOrEmpty(returnedShape))
            {
                dynamic returnedShapeModel = new Composite();
                returnedShapeModel.ContentItem = contentItem;
                returnedShapeModel.IsAlternative = false;
                ajaxMessageModel.Html = this.RenderShapePartial(returnedShapeModel, returnedShape);
            }

            return ajaxMessageModel;
        }

        private string RenderShapePartial(dynamic model, string shapeType)
        {
            var shape = this.Shape.Partial(TemplateName: shapeType, Model: model);
            var display = this.GetDisplayHelper();
            return Convert.ToString(display(shape));
        }

        private dynamic GetDisplayHelper()
        {
            // We can specify any view name, just to get a View only, the shape template finding will be taken care by DisplayHelperFactory.
            // Here the "Brandking" view is always existed, we can also use something like "Layout" ...
            var viewResult = themeAwareViewEngine.FindPartialView(this.ControllerContext, "Layout", false, false);
            var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, new StringWriter());
            return displayHelperFactory.CreateHelper(viewContext, new ViewDataContainer { ViewData = viewContext.ViewData });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private class ViewDataContainer : IViewDataContainer
        {
            public ViewDataDictionary ViewData { get; set; }
        }
    }
}