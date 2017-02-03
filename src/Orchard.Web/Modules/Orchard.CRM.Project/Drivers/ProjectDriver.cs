using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Providers.Filters;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;

namespace Orchard.CRM.Project.Drivers
{
    public class ProjectDriver : ContentPartDriver<ProjectPart>
    {
        private readonly IExtendedProjectService projectService;
        private readonly IActivityStreamService activityStreamService;
        private readonly IOrchardServices services;
        private readonly IMenuService menuService;
        private readonly IProjectionManagerWithDynamicSort projectionWithDynamicSort;
        private readonly ICRMContentOwnershipService contentOwnershipService;

        public ProjectDriver(
            IMenuService menuService,
            IExtendedProjectService projectService,
            IProjectionManagerWithDynamicSort projectionWithDynamicSort,
            IActivityStreamService activityStreamService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices services)
        {
            this.menuService = menuService;
            this.contentOwnershipService = contentOwnershipService;
            this.projectionWithDynamicSort = projectionWithDynamicSort;
            this.services = services;
            this.activityStreamService = activityStreamService;
            this.projectService = projectService;
        }

        protected override DriverResult Display(ProjectPart part, string displayType, dynamic shapeHelper)
        {
            List<DriverResult> shapes = new List<DriverResult>();
            ProjectDetailViewModel projectDetailViewModel = new ProjectDetailViewModel
            {
                ProjectPart = part,
                CurrentUserCanChangePermission = this.contentOwnershipService.CurrentUserCanChangePermission(part.ContentItem),
                CurrentUserCanEdit = this.contentOwnershipService.CurrentUserCanEditContent(part.ContentItem)
            };

            ContentItem menu = null;

            switch (displayType)
            {
                case "TitleOnly":
                    shapes.Add(ContentShape("Parts_Project_TitleOnly", () => shapeHelper.Parts_Project_TitleOnly(Model: part)));
                    return Combined(shapes.ToArray());

                case "Summary":
                    shapes.Add(ContentShape("Parts_Project_Title", () => shapeHelper.Parts_Project_Title(Model: part)));
                    shapes.Add(this.LatestActiveUsers(part, shapeHelper, 8));
                    shapes.Add(this.LastChange(part, shapeHelper));
                    return Combined(shapes.ToArray());

                case "ChangePermissionsTitle":
                    return ContentShape("Parts_Project_ChangePermissionsTitle",
                              () => shapeHelper.Parts_Project_ChangePermissionsTitle(
                                  Model: part));

                case "Detail":
                    if (part.Record.Detail != null)
                    {
                        var projectDetail = this.services.ContentManager.Get(part.Record.Detail.Id);
                        projectDetailViewModel.Detail = this.services.ContentManager.BuildDisplay(projectDetail);
                    }

                    menu = this.projectService.GetProjectMenuWidget(part.Id);

                    if (menu != null)
                    {
                        projectDetailViewModel.MenuShape = this.services.ContentManager.BuildDisplay(menu);
                    }

                    shapes.Add(ContentShape("Parts_Project_Title", () => shapeHelper.Parts_Project_Title(Model: part)));
                    shapes.Add(ContentShape("Parts_Project_Menu", () => shapeHelper.Parts_Project_Menu(Model: projectDetailViewModel)));
                    shapes.Add(ContentShape("Parts_Project", () => shapeHelper.Parts_Project(Model: projectDetailViewModel)));

                    return Combined(shapes.ToArray());

                case "TitleAndMenu":

                    menu = this.projectService.GetProjectMenuWidget(part.Id);

                    if (menu != null)
                    {
                        projectDetailViewModel.MenuShape = this.services.ContentManager.BuildDisplay(menu);
                    }

                    shapes.Add(ContentShape("Parts_Project_Title", () => shapeHelper.Parts_Project_Title(Model: part)));
                    shapes.Add(ContentShape("Parts_Project_Menu", () => shapeHelper.Parts_Project_Menu(Model: projectDetailViewModel)));

                    return Combined(shapes.ToArray());
                default:
                    return ContentShape("Parts_Project_Summary",
                               () => shapeHelper.Parts_Project_Summary(
                                   Model: part));
            }
        }

        protected DriverResult LatestActiveUsers(ProjectPart part, dynamic shapeHelper, int count)
        {
            IUser owner = part.As<CommonPart>().Owner;
            var contentManager = this.services.ContentManager;
            var query = contentManager.HqlQuery().ForVersion(VersionOptions.Published);
            dynamic state = new JObject();
            state.Project_Id = part.Record.Id.ToString(CultureInfo.InvariantCulture);
            query = this.projectionWithDynamicSort.ApplyFilter(query, AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state);
            var latestActiveUserIds = this.activityStreamService.LatestUsersInActivityStreamFilteredByGivenQuery(query, count).ToList();
            if (latestActiveUserIds.Count < count && !latestActiveUserIds.Any(c => c == owner.Id))
            {
                latestActiveUserIds.Add(owner.Id);
            }

            var users = contentManager.GetMany<IUser>(latestActiveUserIds, VersionOptions.Published, QueryHints.Empty);

            List<UserViewModel> model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userModel = ProjectHelper.Convert<UserViewModel>(user);

                model.Add(userModel);
            }

            return ContentShape("Parts_LatestActiveUsers",
                       () => shapeHelper.Parts_LatestActiveUsers(
                           Model: model));
        }

        protected DriverResult LastChange(ProjectPart part, dynamic shapeHelper)
        {
            var contentManager = this.services.ContentManager;
            var attachToProjectQuery = contentManager.HqlQuery().ForVersion(VersionOptions.Published);
            dynamic state = new JObject();
            state.Project_Id = part.Record.Id.ToString(CultureInfo.InvariantCulture);
            attachToProjectQuery = this.projectionWithDynamicSort.ApplyFilter(attachToProjectQuery, AttachToProjectFilter.CategoryName, AttachToProjectFilter.IdFilterType, state);

            var projectQuery = contentManager.HqlQuery().ForVersion(VersionOptions.Published);
            state = new JObject();
            state.Project_Id = part.Record.Id.ToString(CultureInfo.InvariantCulture);
            projectQuery = this.projectionWithDynamicSort.ApplyFilter(projectQuery, ProjectFilter.CategoryName, ProjectFilter.IdFilterType, state);

            var latestActivity = this.activityStreamService.ActivityStreamRestrictedByGivenQuery(new[] { attachToProjectQuery, projectQuery }, 0, 1).FirstOrDefault();

            if (latestActivity == null)
            {
                return null;
            }

            DateTime dateTime = SetSiteTimeZone(latestActivity.CreationDateTime);
            return ContentShape("Parts_Project_LastChange",
                       () => shapeHelper.Parts_Project_LastChange(
                           Model: dateTime));
        }

        protected override DriverResult Editor(ProjectPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            // update project Menu Title
            var projectMenu = this.menuService.GetMenu(part.MenuId);

            if (projectMenu != null)
            {
                var projectMenuTitlePart = projectMenu.As<TitlePart>();

                if (projectMenuTitlePart != null)
                {
                    projectMenu.As<TitlePart>().Title = string.Format("Project-{0} --'{1}'", part.Id.ToString(CultureInfo.InvariantCulture), part.Record.Title);
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(ProjectPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Project_Edit",
                  () => shapeHelper.EditorTemplate(
                      TemplateName: "Parts/Project",
                      Model: part,
                      Prefix: Prefix));
        }

        protected override void Exporting(ProjectPart part, ExportContentContext context)
        {
            var projectElement = context.Element(part.PartDefinition.Name);

            projectElement.SetAttributeValue("Title", part.Title);
            projectElement.SetAttributeValue("Description", part.Description);
        }

        protected override void Importing(ProjectPart part, ImportContentContext context)
        {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null)
            {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Title", title => part.Title = title);

            context.ImportAttribute(part.PartDefinition.Name, "Description", description =>
            part.Description = description);
        }

        private DateTime SetSiteTimeZone(DateTime dateTime)
        {
            var currentSite = this.services.WorkContext.CurrentSite;

            if (String.IsNullOrEmpty(currentSite.SiteTimeZone))
            {
                return dateTime;
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZoneInfo.Utc.Id, currentSite.SiteTimeZone);
        }
    }
}