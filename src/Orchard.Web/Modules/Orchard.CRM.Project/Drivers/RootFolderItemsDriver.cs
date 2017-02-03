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