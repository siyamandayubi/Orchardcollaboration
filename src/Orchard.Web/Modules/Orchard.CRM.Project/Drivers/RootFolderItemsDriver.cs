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

using Orchard.ContentManagement.Drivers;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;
using Orchard.CRM.Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.CRM.Project.Drivers
{
    public class RootFolderItemsDriver : ContentPartDriver<RootFolderItemsPart>
    {
        private readonly IHelperService helperService;
        private readonly IFolderService folderService;
        private readonly IOrchardServices services;
        private readonly IProjectService projectService;

        public RootFolderItemsDriver(
            IHelperService helperService,
            IProjectService projectService,
            IFolderService folderService,
            IOrchardServices services)
        {
            this.projectService = projectService;
            this.services = services;
            this.folderService = folderService;
            this.helperService = helperService;
        }

        protected override DriverResult Display(RootFolderItemsPart part, string displayType, dynamic shapeHelper)
        {
           if (displayType == "Detail")
            {
                int? projectId = ProjectHelper.GetProjectId(part, this.projectService);

                if (projectId == null)
                {
                    return null;
                }

                return this.RenderAttachItemsToFolder(projectId.Value, shapeHelper);
            }
            else
            {
                return null;
            }
        }

        private DriverResult RenderAttachItemsToFolder(int projectId, dynamic shapeHelper)
        {
            var pager = this.helperService.ReterivePagerFromQueryString();
            var items = this.folderService.GetAttachedItemsInRootFolder(projectId, pager);
            int count = this.folderService.GetAttachedItemsInRootFolderCount(projectId);

            var pagerShape = shapeHelper.Pager(pager).PagerId("page");
            pagerShape.TotalItemCount(count);
            
            AttachToFolderListViewModel model = new AttachToFolderListViewModel();
            model.Pager = pagerShape;
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