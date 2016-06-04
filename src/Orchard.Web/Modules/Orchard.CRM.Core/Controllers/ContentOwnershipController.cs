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

namespace Orchard.CRM.Core.Controllers
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Providers.PermissionProviders;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.ViewModels;
    using Orchard.Data;
    using Orchard.Indexing;
    using Orchard.Localization;
    using Orchard.Roles.Models;
    using Orchard.Security;
    using Orchard.Themes;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;

    [Themed]
    public class ContentOwnershipController : Controller
    {
        private readonly IWidgetService widgetService;
        protected IOrchardServices orchardServices;
        private readonly IIndexProvider indexProvider;
        protected ICRMContentOwnershipService contentOwnershipService;
        protected IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository;
        protected IRepository<UserRolesPartRecord> userRolesRepository;
        protected readonly IContentOwnershipHelper contentOwnershipHelper;
        protected readonly IMasterDetailPermissionManager masterDetailPermissionManager;
        private readonly IWorkContextAccessor workContextAccessor;
        private readonly IActivityStreamService activityStreamService;
        private readonly IBasicDataService basicDataService;

        public Localizer T { get; set; }
        public string DefaultDisplayType = "Summary";
        public bool ShowCustomers { get; set; }

        public ContentOwnershipController(
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
            IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository)
        {
            this.masterDetailPermissionManager = masterDetailPermissionManager;
            this.basicDataService = basicDataService;
            this.activityStreamService = activityStreamService;
            this.workContextAccessor = workContextAccessor;
            this.indexProvider = indexProvider;
            this.T = NullLocalizer.Instance;
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.userRolesRepository = userRolesRepository;
            this.contentOwnershipService = contentOwnershipService;
            this.orchardServices = orchardServices;
            this.widgetService = widgetService;
            this.permissionDetailRecordRepository = permissionDetailRecordRepository;
        }

        //
        // GET: /ContentOwnership/
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public virtual ActionResult Edit(int[] ids, string returnUrl)
        {
            var model = this.CreateEditPermissionsModel(ids);

            model.ReturnUrl = returnUrl;

            if (Request.IsAjaxRequest())
            {
                AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { IsDone = true };
                this.widgetService.GetWidgets(model, this.HttpContext);
                ajaxMessageModel.Html = CRMHelper.RenderPartialViewToString(this, "Edit", model);

                return this.Json(ajaxMessageModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Delete(int id, int contentId)
        {
            var contentManager = this.orchardServices.ContentManager;
            var contentItem = contentManager.Get(contentId);

            if (contentItem == null)
            {
                throw new ArgumentNullException("There is no contentItem with the Id: " + contentId.ToString(CultureInfo.InvariantCulture));
            }

            if (this.contentOwnershipService.CurrentUserCanDeletePermission(id, contentItem, this.ModelState))
            {
                var snapshot = this.activityStreamService.TakeSnapshot(contentItem);

                var contentPermissionPart = contentItem.As<ContentItemPermissionPart>();
                var allPermissionRecords = contentPermissionPart.Record.Items;
                var permissionRecord = allPermissionRecords.FirstOrDefault(c => c.Id == id);

                this.permissionDetailRecordRepository.Delete(permissionRecord);
                contentPermissionPart.Record.Items.Remove(permissionRecord);
                this.permissionDetailRecordRepository.Flush();

                this.masterDetailPermissionManager.DeleteChildrenPermissions(contentItem, permissionRecord);

                this.activityStreamService.WriteChangesToStreamActivity(contentItem, snapshot, StreamWriters.ContentItemPermissionStreamWriter);
                var documentIndex = this.indexProvider.New(contentItem.Id);
                contentManager.Index(contentItem, documentIndex);
                this.indexProvider.Store(TicketController.SearchIndexName, documentIndex);
            }
            else
            {
                return this.CreateActionResult(contentId, "PermissionError", null, null);
            }

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary["area"] = "Orchard.CRM.Core";
            routeValueDictionary["ids[0]"] = contentId;

            if (this.Request.IsAjaxRequest())
            {
                return this.CreateActionResult(contentId, string.Empty, null, null);
            }
            else
            {
                return this.RedirectToAction("Edit", "ContentOwnership", routeValueDictionary);
            }
        }

        public ActionResult EditPost(PostEditContentPermissionViewModel inputModel, string returnUrl)
        {
            if (inputModel == null)
            {
                inputModel = new PostEditContentPermissionViewModel();
            }

            EditContentPermissionViewModel model = PostEditContentPermissionViewModel.Convert(inputModel);

            var contentManager = this.orchardServices.ContentManager;
            var contentItems = contentManager.GetMany<ContentItem>(model.ContentIds, VersionOptions.Published, QueryHints.Empty).ToList();

            if (contentItems.Count() == 0)
            {
                throw new ArgumentNullException("There is no contentItem with the given Ids");
            }

            if (this.contentOwnershipHelper.IsChangingPermissionsValid(model, contentItems, this.ModelState))
            {
                this.contentOwnershipHelper.Update(model, contentItems);

                foreach (var contentItem in contentItems)
                {
                    if (this.masterDetailPermissionManager.HasChildItems(contentItem) && inputModel.ApplyToChildren)
                    {
                        this.masterDetailPermissionManager.GrantPermissionToChildren(model, contentItem);
                    }

                    var documentIndex = this.indexProvider.New(contentItem.Id);
                    contentManager.Index(contentItem, documentIndex);
                    this.indexProvider.Store(TicketController.SearchIndexName, documentIndex);
                }
            }
            else
            {
                return this.View("Edit", this.CreateEditPermissionsModel(model.ContentIds));
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                return this.View("Edit", this.CreateEditPermissionsModel(model.ContentIds));
            }

            return this.Redirect(returnUrl);
        }

        public ActionResult PermissionError()
        {
            return this.View();
        }

        private ContentItemSetPermissionsViewModel CreateEditPermissionsModel(IEnumerable<int> ids)
        {
            var contentManager = this.orchardServices.ContentManager;
            var contentItems = contentManager.GetMany<ContentItem>(ids, VersionOptions.Published, QueryHints.Empty).ToList();

            if (contentItems.Count() == 0)
            {
                throw new OrchardCoreException(T("There is no item with the given Id"));
            }

            if (contentItems.Any(c => !this.contentOwnershipService.CurrentUserCanChangePermission(c)))
            {
                throw new OrchardSecurityException(T("You do not have permission to do the operation"));
            }

            var model = this.contentOwnershipHelper.CreateModel();
            this.contentOwnershipHelper.FillPermissions(model, contentItems);

            if (this.ShowCustomers)
            {
                List<IUser> customers = new List<IUser>();
                foreach (var contentItem in contentItems)
                {
                    AttachToProjectPart attachToProjectPart = contentItem.As<AttachToProjectPart>();
                    if (attachToProjectPart != null && attachToProjectPart.Record.Project != null)
                    {
                        var project = contentManager.Get(attachToProjectPart.Record.Project.Id);

                        var temp = this.contentOwnershipHelper.GetCustomersWhoHaveAccessToThisContent(project);
                        customers = customers.Union(temp.Where(c => !customers.Any(d => d.Id == c.Id)).ToList()).ToList();

                        this.contentOwnershipHelper.RestrictToPeopleWhoHavePermissionInGivenItem(model, project);
                    }
                }

                model.Customers.AddRange(customers.Select(c => new SelectListItem { Text = CRMHelper.GetFullNameOfUser(c), Value = c.Id.ToString(CultureInfo.InvariantCulture) }));
            }

            foreach (var item in model.ContentItems)
            {
                if (this.masterDetailPermissionManager.HasChildItems(item.ContentItem))
                {
                    model.HasChildren = true;
                }

                if (!item.CurrentUserHasRightToChangePermissions)
                {
                    throw new Security.OrchardSecurityException(T("You don't have permission to change access to the contentItem"));
                }

                item.ContentItemShape = this.orchardServices.ContentManager.BuildDisplay(item.ContentItem, displayType: this.DefaultDisplayType);
            }

            return model;
        }

        protected ActionResult CreateActionResult(int contentItemId, string view, object ajaxData, Func<object> createFullModel)
        {
            if (Request.IsAjaxRequest())
            {
                AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { Id = contentItemId, IsDone = ModelState.IsValid, Data = ajaxData };
                foreach (var errorGroup in this.ModelState.Where(c => c.Value.Errors.Count > 0))
                {
                    foreach (var error in errorGroup.Value.Errors)
                    {
                        string errorMessage = error.Exception != null ? "An exception happens in server" : error.ErrorMessage;
                        ajaxMessageModel.Errors.Add(new KeyValuePair<string, string>(errorGroup.Key, error.ErrorMessage));
                    }
                }

                return this.Json(ajaxMessageModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                object model = null;

                if (createFullModel != null)
                {
                    model = createFullModel();
                }

                return this.View(view, model);
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var wc = this.workContextAccessor.GetContext();

            // rendering default layout in case notheme flag is true
            var ticketSettings = this.orchardServices.WorkContext.CurrentSite.As<TicketSettingPart>();
            if (ticketSettings.WithoutTheme)
            {
                wc.Layout.Metadata.Alternates.Add("CRMLayout");
            }

            wc.Layout.TicketRelatedPage = true;
        }
    }
}