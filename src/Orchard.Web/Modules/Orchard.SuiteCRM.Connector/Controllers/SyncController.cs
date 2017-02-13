using Orchard.CRM.Core.ViewModels;
using Orchard.Localization;
using Orchard.SuiteCRM.Connector.Services;
using Orchard.SuiteCRM.Connector.ViewModels;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.CRM.Core;
using Newtonsoft.Json;
using Orchard.Settings;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.CRM.Project.Services;
using Orchard.SuiteCRM.Connector.Models;
using Orchard.CRM.Core.Services;
using System.Dynamic;

namespace Orchard.SuiteCRM.Connector.Controllers
{
    [Themed]
    public class SyncController : Controller, IUpdateModel
    {
        private readonly ISuiteCRMDataService suiteCRMDataService;
        private readonly ISuiteCRMSyncService syncService;
        private readonly ISuiteCRMSyncUserService suiteCRMSyncUserService;
        private readonly IOrchardServices services;
        private readonly ISiteService siteService;
        private readonly IExtendedProjectService projectService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;

        public SyncController(
            ICRMContentOwnershipService contentOwnershipService,
            ISuiteCRMSyncUserService suiteCRMSyncUserService,
            IExtendedProjectService projectService,
            ISiteService siteService,
            ISuiteCRMDataService suiteCRMDataService,
            IOrchardServices services,
            ISuiteCRMSyncService syncService)
        {
            this.contentOwnershipService = contentOwnershipService;
            this.suiteCRMSyncUserService = suiteCRMSyncUserService;
            this.projectService = projectService;
            this.siteService = siteService;
            this.syncService = syncService;
            this.services = services;
            this.suiteCRMDataService = suiteCRMDataService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index(int? page, bool? listedBasedOnSuiteCRM)
        {
            int pageSize = this.services.WorkContext.CurrentSite.PageSize;
            page = page ?? 0;
            listedBasedOnSuiteCRM = listedBasedOnSuiteCRM.HasValue ? listedBasedOnSuiteCRM.Value : true;

            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission, T("Not authorized to see list of SuiteCRM projects")))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.SuiteCRMConnectionIsReady())
            {
                return RedirectToAction("Settings");
            }

            MainViewModel model = new MainViewModel();
            model.Projects.AddRange(this.syncService.GetProjects(page.Value, pageSize, listedBasedOnSuiteCRM.Value));

            model.SuiteCRMProjectsCount = this.suiteCRMDataService.ProjectCount();
            model.OrchardCollaborationProjectsCount = this.projectService.GetProjectsCount();
            model.PageSize = pageSize;
            if (listedBasedOnSuiteCRM.Value)
            {
                model.OrchardCollaborationPage = 0;
                model.SuiteCRMPage = page.Value;
            }
            else
            {
                model.OrchardCollaborationPage = page.Value;
                model.SuiteCRMPage = 0;
            }

            model.ListedBasedOnSuiteCRM = listedBasedOnSuiteCRM.Value;
            model.ViewUsersPage = false;

            foreach (var project in model.Projects.Where(c => c.OrchardCollaborationProject != null))
            {
                project.OrchardProjectShape = this.services.ContentManager.BuildDisplay(project.OrchardCollaborationProject, displayType: "SyncTitleSummary");
            }

            if (this.Request.IsAjaxRequest())
            {
                AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(model);
                return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
            }
            else
            {
                return View(model);
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Users(int? page)
        {
            int pageSize = this.services.WorkContext.CurrentSite.PageSize;
            page = page ?? 0;

            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission, T("Not authorized to see list of SuiteCRM projects")))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.SuiteCRMConnectionIsReady())
            {
                return RedirectToAction("Settings");
            }

            MainViewModel model = new MainViewModel();
            model.Users.AddRange(this.suiteCRMSyncUserService.GetUsers(page.Value, pageSize));
            model.SuiteCRMUsersCount = this.suiteCRMSyncUserService.GetSuiteCRMUsersCount();
            model.PageSize = pageSize;
            model.SuiteCRMPage = page.Value;
            model.ViewUsersPage = true;
   
            if (this.Request.IsAjaxRequest())
            {
                AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(model);
                return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
            }
            else
            {
                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult CopySuiteCRMUsersToOrchardPost(CopySuiteCRMUsersToOrchardViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission, T("Not authorized to see list of SuiteCRM projects")))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = this.CreateAjaxModel(null);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            var users = this.suiteCRMSyncUserService.CopySuiteCRMUsersToOrchard(model);

            AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(users);
            return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));            
        }

        public ActionResult Settings(string returnUrl)
        {
            if (!this.services.Authorizer.Authorize(Orchard.Core.Settings.Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var model = this.CreateSettingModel(returnUrl, string.Empty);

            return this.View(model);
        }

        [HttpPost, ActionName("Settings")]
        public ActionResult SettingsPOST(string groupInfoId, string returnUrl)
        {
            if (!this.services.Authorizer.Authorize(Orchard.Core.Settings.Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var site = this.siteService.GetSiteSettings();
            var model = this.services.ContentManager.UpdateEditor(site, this, groupInfoId);

            GroupInfo groupInfo = null;

            if (!string.IsNullOrWhiteSpace(groupInfoId))
            {
                if (model == null)
                {
                    this.services.TransactionManager.Cancel();
                    return HttpNotFound();
                }

                groupInfo = this.services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
                if (groupInfo == null)
                {
                    this.services.TransactionManager.Cancel();
                    return HttpNotFound();
                }
            }

            if (!ModelState.IsValid)
            {
                this.services.TransactionManager.Cancel();
                return this.View(this.CreateSettingModel(returnUrl, T("There are validation errors.").Text));
            }

            if (!this.SuiteCRMConnectionIsReady())
            {
                return this.View(this.CreateSettingModel(returnUrl, T("Failed to connect to the database server. Please recheck the parameters").Text));
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetSuiteCRMContact(string email)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return new HttpUnauthorizedResult();
            }

            Database.email_addr_bean_rel contact = null;

            if (Helper.IsDatabaseConnectionProvided(this.services, this.Logger))
            {
                contact = this.suiteCRMDataService.GetContact(email);
            }

            dynamic model = new ExpandoObject();

            if (contact == null)
            {
                model.Found = false;
            }
            else
            {
                model.Found = true;
                model.ContactId = contact.bean_id;
                model.Url = Helper.GetContactAddressInSuiteCRM(this.services, contact.bean_id);
            }

            AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(model);
            return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
        }

        [HttpPost]
        public ActionResult CopyOrchardProjectsToSuite(CopyOrchardProjectToSuiteViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission, T("Not authorized to see list of SuiteCRM projects")))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = this.CreateAjaxModel(null);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            var projects = this.syncService.CopyOrchardProjectsToSuite(model);
            var resultModel = new MainViewModel();
            resultModel.SuiteCRMProjectsCount = this.suiteCRMDataService.ProjectCount();
            resultModel.OrchardCollaborationProjectsCount = this.projectService.GetProjectsCount();

            resultModel.Projects.AddRange(projects);

            foreach (var project in resultModel.Projects.Where(c => c.OrchardCollaborationProject != null))
            {
                project.OrchardProjectShape = this.services.ContentManager.BuildDisplay(project.OrchardCollaborationProject, displayType: "SyncTitleSummary");
            }

            AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(resultModel);
            return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
        }

        [HttpPost]
        public ActionResult CopySuiteToOrchardProjects(CopyOrchardProjectToSuiteViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageSuiteCRMPermission, T("Not authorized to see list of SuiteCRM projects")))
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = this.CreateAjaxModel(null);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            var projects = this.syncService.CopySuiteCRMProjectsToOrchard(model);
            var resultModel = new MainViewModel();
            resultModel.Projects.AddRange(projects);
            resultModel.SuiteCRMProjectsCount = this.suiteCRMDataService.ProjectCount();
            resultModel.OrchardCollaborationProjectsCount = this.projectService.GetProjectsCount();

            foreach (var project in resultModel.Projects.Where(c => c.OrchardCollaborationProject != null))
            {
                project.OrchardProjectShape = this.services.ContentManager.BuildDisplay(project.OrchardCollaborationProject, displayType: "SyncTitleSummary");
            }

            AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(resultModel);
            return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
        }

        [HttpPost]
        public ActionResult CopyOrchardTicketToSuiteTasks(CopyOrchardTasksToSuiteViewModel model)
        {
            if (!this.contentOwnershipService.IsCurrentUserOperator())
            {
                return new HttpUnauthorizedResult();
            }

            if (!this.ModelState.IsValid)
            {
                AjaxMessageViewModel errorModel = this.CreateAjaxModel(null);
                return this.Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            var resultModel = this.syncService.CopyOrchardTicketsToSuite(model);

            foreach (var task in resultModel)
            {
                task.SuiteCRMUrl = Helper.GetTaskAddressInSuiteCRM(
                    this.services, 
                    task.SuiteCRMTaskId, 
                    task.IsProjectTask ? SuiteCRMTaskPart.SuiteCRMProjectTaskTypeValue : SuiteCRMTaskPart.SuiteCRMTaskTypeValue);
            }

            AjaxMessageViewModel ajaxMessageModel = this.CreateAjaxModel(resultModel);
            return this.Json(JsonConvert.SerializeObject(ajaxMessageModel));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private bool SuiteCRMConnectionIsReady()
        {
            try
            {
                using (var connection = Helper.GetConnection(this.services, this.Logger))
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private dynamic CreateSettingModel(string returnUrl, string error)
        {
            dynamic model;
            string groupInfoId = "SuiteCRM Settings";
            var site = this.siteService.GetSiteSettings();
            if (!string.IsNullOrWhiteSpace(groupInfoId))
            {
                model = this.services.ContentManager.BuildEditor(site, groupInfoId);

                if (model == null)
                    return HttpNotFound();

                var groupInfo = this.services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
                if (groupInfo == null)
                    return HttpNotFound();

                model.GroupInfo = groupInfo;
            }
            else
            {
                model = this.services.ContentManager.BuildEditor(site);
            }

            model.ConnectionError = error ?? "";
            model.ReturnUrl = returnUrl ?? "";

            return model;
        }

        private ContentResult Json(string data)
        {
            return this.Content(data, "application/json");
        }

        private AjaxMessageViewModel CreateAjaxModel(object model)
        {
            AjaxMessageViewModel ajaxMessageModel = new AjaxMessageViewModel { Data = model, IsDone = this.ModelState.IsValid };
            foreach (var item in ModelState)
            {
                ajaxMessageModel.Errors.AddRange(item.Value.Errors.Select(c => new KeyValuePair<string, string>(item.Key, c.Exception != null ? c.Exception.Message : c.ErrorMessage)).Where(c => !string.IsNullOrEmpty(c.Value)));
            }

            return ajaxMessageModel;
        }
    }
}