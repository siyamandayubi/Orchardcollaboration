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