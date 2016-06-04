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

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Drivers
{
    public class FolderListDriver : ContentPartDriver<FolderListPart>
    {
        private readonly IProjectService projectService;
        private readonly IFolderService folderService;
        private readonly IOrchardServices services;
        private readonly IHelperService helperService;
        protected readonly ICRMContentOwnershipService contentOwnershipService;
        public Localizer T { get; set; }

        public FolderListDriver(
            IHelperService helperService,
            IFolderService folderService,
            IProjectService projectService,
            ICRMContentOwnershipService contentOwnershipService,
            IOrchardServices services)
        {
            this.helperService = helperService;
            this.folderService = folderService;
            this.services = services;
            this.contentOwnershipService = contentOwnershipService;
            this.projectService = projectService;
            this.T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(FolderListPart part, string displayType, dynamic shapeHelper)
        {
            if (displayType != "Detail")
            {
                return null;
            }

            return this.RenderFolders(part, displayType, shapeHelper);
        }

        protected override DriverResult Editor(FolderListPart part, dynamic shapeHelper)
        {
            return this.RenderFolders(part, "Detail", shapeHelper);
        }

        protected DriverResult RenderFolders(FolderListPart part, string displayType, dynamic shapeHelper)
        {
            int? projectId = ProjectHelper.GetProjectId(part, this.projectService);

            FolderPart folderPart = part.As<FolderPart>();
            int? folderId = folderPart != null ? (int?)folderPart.Id : null;

            if (folderId == null)
            {
                AttachToFolderPart attachToFolderPart = part.As<AttachToFolderPart>();
                if (attachToFolderPart != null && attachToFolderPart.Record.Folder != null)
                {
                    folderId = attachToFolderPart.Record.Folder.Id;
                }
            }

            var folders = this.folderService.GetFolders(projectId.Value).Select(c => c.As<FolderPart>()).ToList();

            // folders doesn't contain all folders, it is restrcited by the folders visible by current user. So currentFolder is
            // equal to part, if it exists in the folders list, otherwise it is null
            FolderPart currentFolder = folderId.HasValue ? folders.FirstOrDefault(c => c.Record.Id == folderId) : null;

            // Building the tree
            FolderViewModel root = new FolderViewModel();
            root.IsSelected = folderId == null && part.ContentItem.ContentType != ContentTypes.WikiContentType;
            var tree = this.folderService.ConvertToTree(folders, folderId);

            FolderPart parentFolder = currentFolder != null && currentFolder.Record.Parent_Id != null ?
                folders.FirstOrDefault(c => c.Record.Id == currentFolder.Record.Parent_Id.Value) :
                null;

            // show the tree from the root, if the tree is small, or if there is no selected folder
            if (folderId == null || currentFolder == null || parentFolder == null || folders.Count < 100)
            {
                root.Title = T("Root").Text;
                root.ProjectId = projectId.Value;
                root.Folders.AddRange(tree.Folders);
            }
            else
            {
                root = this.folderService.FindInTree(tree, currentFolder.Record.Parent_Id.Value);
                root.Title = "...";
            }

            return ContentShape("Parts_FolderList",
                  () => shapeHelper.Parts_FolderList(
                      Model: root
                      ));
        }
    }
}