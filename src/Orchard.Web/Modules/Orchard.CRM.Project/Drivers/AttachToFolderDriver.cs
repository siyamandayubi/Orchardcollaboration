using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Models;

namespace Orchard.CRM.Project.Drivers
{
    public class AttachToFolderDriver : MenuBaseDriver<AttachToFolderPart>
    {
        public AttachToFolderDriver(
            ICRMContentOwnershipService contentOwnershipService,
            IExtendedProjectService projectService,
            IOrchardServices services,
            IHelperService helperService,
            IFolderService folderService)
            : base(contentOwnershipService, projectService, services, helperService, folderService)
        {
        }

        protected override DriverResult Display(AttachToFolderPart part, string displayType, dynamic shapeHelper)
        {
            if (part.Record.Folder != null)
            {
                var item = this.services.ContentManager.Get<FolderPart>(part.Record.Folder.Id, VersionOptions.Published);

                if (part.ContentItem.ContentType == ContentTypes.ProjectWikiContentType || part.ContentItem.ContentType == ContentTypes.RootWikiContentType)
                {
                    List<DriverResult> shapes = new List<DriverResult>();
                    shapes.Add(ContentShape("Parts_Wiki_Title", () => shapeHelper.Parts_Wiki_Title(Model: item)));
                    return Combined(shapes.ToArray());
                }
                else
                {
                    var project = this.projectService.GetProject(item.Record.Project.Id);
                    List<DriverResult> shapes = this.GetFolderMenuAndTitleShapes(item, project, shapeHelper);

                    return this.Combined(shapes.ToArray());
                }
            }
            else
            {
                AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();
                if (attachToProjectPart != null)
                {
                    var project = this.services.ContentManager.Get<ProjectPart>(attachToProjectPart.Record.Project.Id, VersionOptions.Published);

                    return this.GetWikiMenuAndTitle(part, project, shapeHelper);
                }
                else
                {
                    return null;
                }
            }
        }

        protected override DriverResult Editor(AttachToFolderPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            AttachToFolderEditPostModel model = new AttachToFolderEditPostModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            part.Record.Folder = model.FolderId.HasValue ? new FolderPartRecord { Id = model.FolderId.Value } : null;

            return this.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(AttachToFolderPart part, dynamic shapeHelper)
        {
            int? projectId = null;

            // if it is Create Mode
            AttachToProjectPart attachToProjectPart = part.As<AttachToProjectPart>();
            if (attachToProjectPart == null || attachToProjectPart.Record.Project == null)
            {
                projectId = this.projectService.GetProjectIdFromQueryString();
            }
            else
            {
                projectId = attachToProjectPart.Record.Project != null ? (int?)attachToProjectPart.Record.Project.Id : null;
            }

            if (projectId == null)
            {
                return null;
            }

            var folders = this.folderService.GetFolders(projectId.Value).Select(c => c.As<FolderPart>()).ToList();
            FolderViewModel model = this.folderService.ConvertToTree(folders, part.Record.Folder != null ? (int?)part.Record.Folder.Id : null);

            List<DriverResult> shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_AttachToFolder_Edit",
                   () => shapeHelper.EditorTemplate(
                       TemplateName: "Parts/AttachToFolder",
                       Model: model,
                       Prefix: Prefix)));

            var project = this.projectService.GetProject(projectId.Value);
            
            if (part.Id != default(int) && part.Record.Folder != null)
            {
                var folder = folders.FirstOrDefault(c => c.Id == part.Record.Folder.Id);
                shapes.AddRange(this.GetFolderMenuAndTitleShapes(folder, project, shapeHelper));
            }
            else
            {
                if (project != null)
                {
                    shapes.Add(this.GetWikiMenuAndTitle(part, project, shapeHelper));
                }
                else
                {
                    FolderWithAncestorsViewModel titleModel = new FolderWithAncestorsViewModel
                    {
                        Title = T("[No Folder Is Selected]").Text,
                        Project = project.Record,
                        ProjectId = projectId
                    };

                    shapes.Add(ContentShape("Parts_Folder_Title", () => shapeHelper.Parts_Folder_Title(Model: titleModel)));
                }
            }

            return Combined(shapes.ToArray());

        }
    }
}