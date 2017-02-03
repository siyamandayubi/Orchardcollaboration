using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.CRM.Project.Drivers
{
    public class ProjectTitleAndMenuDriver : MenuBaseDriver<ProjectTitleAndMenuPart>
    {
        private readonly IExtendedProjectService extendedProjectService;

        public ProjectTitleAndMenuDriver(
            IFolderService folderService,
            IOrchardServices services,
            IExtendedProjectService projectService,
            IHelperService helperService,
            ICRMContentOwnershipService contentOwnershipService)
            : base(contentOwnershipService, projectService, services, helperService, folderService)
        {
            this.extendedProjectService = projectService;
        }

        protected override DriverResult Editor(ProjectTitleAndMenuPart part, dynamic shapeHelper)
        {
            return this.Display(part, "Detail", shapeHelper);
        }

        protected override DriverResult Display(ProjectTitleAndMenuPart part, string displayType, dynamic shapeHelper)
        {
            if (!(new string[] { "Detail", "TitleAndMenu", "Planner", "GanttChart" }).Contains(displayType))
            {
                return null;
            }

            var attachToProjectPart = part.As<AttachToProjectPart>();

            if (attachToProjectPart == null || attachToProjectPart.Record.Project != null)
            {
                var item = this.services.ContentManager.Get<ProjectPart>(attachToProjectPart.Record.Project.Id, VersionOptions.Published);

                var projectDetailViewModel = new ProjectDetailViewModel
                {
                    ProjectPart = item,
                    CurrentUserCanChangePermission = this.contentOwnershipService.CurrentUserCanChangePermission(item),
                    CurrentUserCanEdit = this.contentOwnershipService.CurrentUserCanEditContent(item)
                };

                List<DriverResult> shapes = new List<DriverResult>();
                var menu = this.extendedProjectService.GetProjectMenuWidget(attachToProjectPart.Record.Project.Id);

                if (menu != null)
                {
                    projectDetailViewModel.MenuShape = this.services.ContentManager.BuildDisplay(menu);
                }

                shapes.Add(ContentShape("Parts_Project_Menu", () => shapeHelper.Parts_Project_Menu(Model: projectDetailViewModel)));

                if (part.ContentItem.ContentType == ContentTypes.ProjectWikiContentType || part.ContentItem.ContentType == ContentTypes.RootWikiContentType)
                {
                    shapes.Add(ContentShape("Parts_Wiki_Title", () => shapeHelper.Parts_Wiki_Title(Model: item)));
                }
                else
                {
                    shapes.Add(ContentShape("Parts_Project_Title", () => shapeHelper.Parts_Project_Title(Model: item)));
                }

                return Combined(shapes.ToArray());
            }
            else
            {
                return null;
            }
        }
    }
}