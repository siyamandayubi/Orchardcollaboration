using Orchard.ContentManagement;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.CRM.Project.Services
{
    public class CRMContentOwnershipServiceExtension : ICRMContentOwnershipServiceExtension 
    {
        private readonly IWorkContextAccessor workContextAccessor;

        public CRMContentOwnershipServiceExtension(IWorkContextAccessor workContextAccessor)
        {
            this.workContextAccessor = workContextAccessor;
        }

        public bool CanApply(IContent content, ICRMContentOwnershipService contentOwnershipService)
        {
            var folderPart = content.As<FolderPart>();
            var attachToFolder = content.As<AttachToFolderPart>();

            return folderPart != null || attachToFolder != null;
        }

        public bool HasAccessTo(IContent content, ICRMContentOwnershipService contentOwnershipService)
        {
            if (!this.CanApply(content,contentOwnershipService))
            {
                return true;
            }

            var projectService = this.workContextAccessor.GetContext().Resolve<IProjectService>();
            
            var folderPart = content.As<FolderPart>();
            var attachToFolder = content.As<AttachToFolderPart>();

            if (folderPart != null && folderPart.Record.Project != null)
            {
                var project = projectService.GetProject(folderPart.Record.Project.Id);

                if (project == null)
                {
                    return false;
                }

                return contentOwnershipService.CurrentUserCanEditContent(project);
            }

            if (attachToFolder != null && attachToFolder.Record.Folder != null && attachToFolder.Record.Folder.Project != null)
            {
                var project = projectService.GetProject(attachToFolder.Record.Folder.Project.Id);

                if (project == null)
                {
                    return false;
                }

                return contentOwnershipService.CurrentUserCanEditContent(project);
            }

            return true;
        }
    }
}
