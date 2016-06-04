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
