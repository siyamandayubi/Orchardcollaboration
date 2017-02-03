using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Drivers
{
    public abstract class MenuBaseDriver<TContent> : ContentPartDriver<TContent>
        where TContent : ContentPart, new()
    {
        protected readonly IOrchardServices services;
        protected readonly IFolderService folderService;
        protected readonly IHelperService helperService;
        protected readonly IExtendedProjectService projectService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;

        /// <summary>
        /// The base class for FolderDriver and ProjectTitleAndMenuDriver.
        /// </summary>
        public MenuBaseDriver(
            ICRMContentOwnershipService contentOwnershipService,
            IExtendedProjectService projectService,
            IOrchardServices services,
            IHelperService helperService,
            IFolderService folderService)
        {
            this.contentOwnershipService = contentOwnershipService;
            this.projectService = projectService;
            this.helperService = helperService;
            this.folderService = folderService;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected DriverResult GetWikiMenuAndTitle(ContentPart part, ProjectPart item, dynamic shapeHelper)
        {
            return this.GetWikiMenuAndTitle(part, item, shapeHelper, true);
        }

        protected DriverResult GetWikiMenuAndTitle(ContentPart part, ProjectPart item, dynamic shapeHelper, bool renderTitle)
        {
            var projectDetailViewModel = new ProjectDetailViewModel
            {
                ProjectPart = item,
                CurrentUserCanChangePermission = this.contentOwnershipService.CurrentUserCanChangePermission(part),
                CurrentUserCanEdit = this.contentOwnershipService.CurrentUserCanEditContent(part)
            };

            List<DriverResult> shapes = new List<DriverResult>();
            
            if (renderTitle)
            {
                shapes.Add(ContentShape("Parts_Wiki_Title", () => shapeHelper.Parts_Wiki_Title(Model: item)));
            }

            if (this.contentOwnershipService.CurrentUserCanEditContent(item))
            {
                projectDetailViewModel.MenuShape = this.GetProjectMenuShape(item.Id);
                shapes.Add(ContentShape("Parts_Project_Menu", () => shapeHelper.Parts_Wiki_Menu(Model: projectDetailViewModel)));
            }

            return Combined(shapes.ToArray());
        }

        protected dynamic GetProjectMenuShape(int projectId)
        {
            var menu = this.projectService.GetProjectMenuWidget(projectId);

            return menu != null ? this.services.ContentManager.BuildDisplay(menu) : null;
        }

        protected IEnumerable<DriverResult> GetFolderMenuAndTitleShapes(FolderPart part, ProjectPart projectPart, dynamic shapeHelper)
        {
            FolderWithAncestorsViewModel model = new FolderWithAncestorsViewModel(this.folderService.Convert(part));
            model.Project = projectPart.Record;
            model.ProjectId = projectPart.Id;

            List<DriverResult> shapes = new List<DriverResult>();

            if (this.contentOwnershipService.CurrentUserCanEditContent(projectPart.ContentItem))
            {
                shapes.Add(ContentShape("Parts_Folder_Menu", () => shapeHelper.Parts_Folder_Menu(Model: model)));
            }

            // folder path
            var folders = this.folderService.GetFolders(projectPart.Id).Select(c => c.As<FolderPart>());
            var ancestors = this.folderService.GetAncestors(folders, part.Id);
            model.Ancestors.AddRange(ancestors.Select(c => this.folderService.Convert(c)));
            shapes.Add(this.ContentShape("Parts_Folder_Title",
                () => shapeHelper.Parts_Folder_Title(
                    Model: model)));

            return shapes;
        }

    }
}