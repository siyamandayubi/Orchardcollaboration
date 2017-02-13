using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Controllers;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
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
using System.Web.Routing;

namespace Orchard.CRM.Project.Controllers
{
    public class MilestoneController : BaseController
    {
        private readonly IMilestoneService milestoneService;
        private readonly UrlHelper urlHelper;

        public MilestoneController(
            IMilestoneService milestoneService,
            UrlHelper urlHelper,
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
            : base(ContentTypes.ProjectContentType, "Milestone_Edit", indexProvider, services, crmContentOwnershipService, transactionManager, extendedContentManager, contentManager, widgetService, themeAwareViewEngine, shapeFactory, displayHelperFactory, basicDataService, contentOwnershipHelper, activityStreamService, contentItemDescriptorManager)
        {
            this.urlHelper = urlHelper;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
            this.milestoneService = milestoneService;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult ProjectMilestones(int projectId)
        {
            // check user is operator
            if (!this.crmContentOwnershipService.IsCurrentUserOperator())
            {
                return HttpNotFound();
            }

            var project = this.services.ContentManager.Get(projectId);

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(project))
            {
                ModelState.AddModelError("ProjectId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var milestones = this.milestoneService.GetOpenMilestones(projectId);

            EditAttachToViewModel model = new EditAttachToViewModel();
            Converter.Fill(model.Items, milestones);

            AjaxMessageViewModel returnValue = new AjaxMessageViewModel { IsDone = true, Data = model };
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateMilestoneItems(EditMilestoneItemsOrderViewModel model)
        {
            // check user is operator
            if (!this.crmContentOwnershipService.IsCurrentUserOperator())
            {
                return HttpNotFound();
            }

            if (model == null)
            {
                ModelState.AddModelError("ProjectId", new OrchardSecurityException(T("ProjectId is not provided. It is mandatory field.")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var project = this.services.ContentManager.Get(model.ProjectId);

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(project))
            {
                ModelState.AddModelError("ProjectId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var items = this.contentManager.GetMany<AttachToMilestonePart>(
                model.Items.Select(c => c.ContentItemId),
                VersionOptions.Published,
                new QueryHints().ExpandRecords<AttachToMilestonePartRecord>());

            var milestones = this.contentManager.GetMany<MilestonePart>(
                model.Items.Select(c => c.MilestoneId),
                VersionOptions.Published,
                new QueryHints().ExpandRecords<TitlePartRecord, MilestonePartRecord>());

            int userId = this.services.WorkContext.CurrentUser.Id;

            foreach (var item in items)
            {
                var itemInModel = model.Items.FirstOrDefault(c => c.ContentItemId == item.Id);

                if (itemInModel == null)
                {
                    continue;
                }

                var milestone = milestones.FirstOrDefault(c => c.Id == itemInModel.MilestoneId);
                if (milestone == null)
                {
                    continue;
                }

                RouteValueDictionary activityStreamRouteValues = new RouteValueDictionary();
                activityStreamRouteValues.Add("action", "Display");
                activityStreamRouteValues.Add("controller", "Item");
                activityStreamRouteValues.Add("area", "orchard.crm.core");
                activityStreamRouteValues.Add("id", item.ContentItem.Id);

                bool changeIsWritten = false;
                if (item.Record.MilestoneId != itemInModel.MilestoneId)
                {
                    item.Record.MilestoneId = itemInModel.MilestoneId;
                    string description = string.Format(
                        "Move the '{0}' into the milestone '{1}' at the position {2}",
                        this.contentItemDescriptorManager.GetDescription(item),
                        this.contentItemDescriptorManager.GetDescription(milestone),
                        itemInModel.OrderId.ToString(CultureInfo.InvariantCulture));

                    this.streamService.WriteChangesToStreamActivity(userId, item.ContentItem.Id, item.ContentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, description, activityStreamRouteValues);
                    changeIsWritten = true;
                }

                if (item.Record.OrderId != itemInModel.OrderId)
                {
                    item.Record.OrderId = itemInModel.OrderId;

                    if (!changeIsWritten)
                    {
                        string description = string.Format(
                            "Move the '{0}' of the milestone '{1}' to the position {2}",
                            this.contentItemDescriptorManager.GetDescription(item),
                            this.contentItemDescriptorManager.GetDescription(milestone),
                            itemInModel.OrderId.ToString(CultureInfo.InvariantCulture));

                        this.streamService.WriteChangesToStreamActivity(userId, item.ContentItem.Id, item.ContentItem.VersionRecord.Id, new ActivityStreamChangeItem[] { }, description, activityStreamRouteValues);
                    }
                }
            }

            int? returnUrlContentItem = model.Items.Select(c => c.MilestoneId).FirstOrDefault();
            returnUrlContentItem = returnUrlContentItem ?? model.ProjectId;

            string returnUrl = urlHelper.Action("Display", "Item", new { area = "Orchard.CRM.Core", id = returnUrlContentItem.Value });

            return this.CreateActionResultBasedOnAjaxRequest(model, null, null, returnUrl);
        }

        public ActionResult Planner(int projectId, int milestoneId)
        {
            // check user is operator
            if (!this.crmContentOwnershipService.IsCurrentUserOperator())
            {
                return HttpNotFound();
            }

            var project = this.services.ContentManager.Get(projectId);

            if (project == null)
            {
                return HttpNotFound();
            }

            if (!this.crmContentOwnershipService.CurrentUserCanViewContent(project))
            {
                ModelState.AddModelError("projectId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var milestone = this.milestoneService.GetMilestone(milestoneId);

            if (milestone == null)
            {
                return HttpNotFound();
            }

            if (!this.crmContentOwnershipService.CurrentUserCanEditContent(milestone))
            {
                ModelState.AddModelError("MilestoneId", new OrchardSecurityException(T("You are not authorized to view the item")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            var backlog = this.milestoneService.GerProjectBacklog(projectId);

            if (backlog == null)
            {
                ModelState.AddModelError("projectId", new OrchardSecurityException(T("Project doesn't have any Backlog")));
                return this.CreateActionResultBasedOnAjaxRequest(null, null);
            }

            dynamic milestoneShape = this.contentManager.BuildDisplay(milestone, displayType: "Planner");
            var backLogShape = this.contentManager.BuildDisplay(backlog, displayType: "Planner");
            milestoneShape.Backlog = backLogShape;

            dynamic model = new ExpandoObject();
            model.Milestone = milestoneShape;
            model.Backlog = backLogShape;
            return View(model);
        }

        protected override bool IsCreateAuthorized()
        {
            return false;
        }

        protected override bool IsDisplayAuthorized()
        {
            return false;
        }
    }
}