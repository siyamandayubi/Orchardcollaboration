using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
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
    public class FolderDriver : MenuBaseDriver<FolderPart>
    {
        public FolderDriver(
            IExtendedProjectService projectService,
            IHelperService helperService,
            IFolderService folderService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices services):base(contentOwnershipService,projectService,services,helperService,folderService)
        {
        }

        protected override DriverResult Display(FolderPart part, string displayType, dynamic shapeHelper)
        {
            FolderWithAncestorsViewModel model = new FolderWithAncestorsViewModel(this.folderService.Convert(part));
            model.Project = part.Record.Project;

            switch (displayType)
            {
                case "Summary":
                    return ContentShape("Parts_Folder_Summary",
                              () => shapeHelper.Parts_Folder_Summary(
                                  Model: model));

                case "ChangePermissionsTitle":
                case "TitleAndMenu":
                    return ContentShape("Parts_Folder_ChangePermissionsTitle",
                              () => shapeHelper.Parts_Folder_ChangePermissionsTitle(
                                  Model: model));


                case "Detail":

                    if (part.Record.Project == null)
                    {
                        throw new System.Data.DataException("Folder is not associated with any project");
                    }

                    List<DriverResult> shapes = new List<DriverResult>();
                    var project = this.projectService.GetProject(part.Record.Project.Id);

                    shapes.AddRange(this.GetFolderMenuAndTitleShapes(part, project, shapeHelper));

                    // items
                    shapes.Add(this.RenderAttachItemsToFolder(part.Record.Id, part.Record.Project.Id, shapeHelper));

                    return this.Combined(shapes.ToArray());
                default:
                    return ContentShape("Parts_Folder_Summary",
                               () => shapeHelper.Parts_Folder_Summary(
                                   Model: model));
            }
        }

        protected override DriverResult Editor(FolderPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!this.contentOwnershipService.CurrentUserCanEditContent(part.ContentItem))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to create/edit the item"));
            }

            PostedEditFolderViewModel model = new PostedEditFolderViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            EditAttachToViewModel attachToProjectModel = new EditAttachToViewModel();
            updater.TryUpdateModel(attachToProjectModel, "AttachToProjectPart", null, null);
            
            // not valid
            if (attachToProjectModel.SelectedId == null)
            {
                updater.AddModelError("AttachToProjectPart.ProjectId", T("No project is selected"));
                return this.Editor(part, shapeHelper);
            }

            var project = this.projectService.GetProject(attachToProjectModel.SelectedId.Value);

            if (project == null)
            {
                throw new OrchardCoreException(T("There is no project with the given Id"));
            }

            // check user has access to the project
            if (!this.contentOwnershipService.CurrentUserCanEditContent(project))
            {
                throw new Security.OrchardSecurityException(T("You don't have permission to create/edit the item"));
            }

            // A folder can not be parent of himself or a sub-folder can not be parent of his parents
            if (model.ParentId != null)
            {
                var folders = this.folderService.GetFolders(attachToProjectModel.SelectedId.Value).Select(c => c.As<FolderPart>());
                var ancestors = this.folderService.GetAncestors(folders, model.ParentId.Value).Select(c => c.Id).ToList();
                ancestors.Add(part.Id);

                if (ancestors.Any(c => c == model.ParentId.Value))
                {
                    updater.AddModelError("FolderPart.ParentId", T("A Folder can not be parent of himself. Please select another folder."));
                    return this.Editor(part, shapeHelper);
                }
            }

            part.Record.Title = model.Title;
            part.Record.Parent_Id = model.ParentId;


            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(FolderPart part, dynamic shapeHelper)
        {
            int? projectId = null;

            // if it is Create Mode
            if (part.Record.Id == default(int))
            {
                projectId = this.projectService.GetProjectIdFromQueryString();
            }
            else
            {
                projectId = part.Record.Project != null ? (int?)part.Record.Project.Id : null;
            }

            if (projectId == null)
            {
                return null;
            }

            var folders = this.folderService.GetFolders(projectId.Value).Select(c => c.As<FolderPart>()).ToList();
            FolderViewModel root = this.folderService.ConvertToTree(folders, part.Record.Parent_Id);

            EditFolderViewModel model = new EditFolderViewModel
            {
                Tree = root,
                FolderId = part.Id != default(int) ? (int?)part.Id : null,
                Title = part.Record.Title,
                ProjectId = projectId
            };

            var project = this.projectService.GetProject(projectId.Value);

            if (project == null)
            {
                throw new AccessViolationException("The project doesn't exist or you don't have permission to access it");
            }

            List<DriverResult> shapes = new List<DriverResult>();
            shapes.AddRange(this.GetFolderMenuAndTitleShapes(part, project, shapeHelper));
            shapes.Add(ContentShape("Parts_Folder_Edit",
                   () => shapeHelper.EditorTemplate(
                       TemplateName: "Parts/Folder",
                       Model: model,
                       Prefix: Prefix)));

            return this.Combined(shapes.ToArray());
        }

        private DriverResult RenderAttachItemsToFolder(int folderId, int projectId, dynamic shapeHelper)
        {
            var pager = this.helperService.ReterivePagerFromQueryString();
            var items = this.folderService.GetAttachedItemsToFolder(folderId, projectId, pager);
            int count = this.folderService.GetAttachedItemsToFolderCount(folderId, projectId);

            var pagerShape = shapeHelper.Pager(pager).PagerId("page");
            pagerShape.TotalItemCount(count);

            AttachToFolderListViewModel model = new AttachToFolderListViewModel { Pager = pagerShape };
            foreach (var item in items)
            {
                model.Items.Add(this.services.ContentManager.BuildDisplay(item, "Summary"));
            }

            return ContentShape("Parts_AttachedToFolderList",
                 () => shapeHelper.Parts_AttachedToFolderList(
                    Model: model
                  ));
        }
    }
}