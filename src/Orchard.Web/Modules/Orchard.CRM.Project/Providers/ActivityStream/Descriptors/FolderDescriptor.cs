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

namespace Orchard.CRM.Project.Providers.ActivityStream.Descriptors
{
    using Orchard.ContentManagement;
    using Orchard.CRM.Core.Models;
    using Orchard.Localization;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
    using Orchard.CRM.Project.Services;
    using Orchard.CRM.Project.Models;
    
    public class FolderDescriptor : IContentItemDescriptor
    {
        private readonly IFolderService folderService;

        public FolderDescriptor(IFolderService projectService)
        {
            this.folderService = projectService;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetDescription(IContent content)
        {
            var folder = content.As<FolderPart>();
            if (folder == null)
            {
                return string.Empty;
            }

            folder = this.folderService.GetFolder(folder.Record.Id);

            // means it is a new folder
            if (folder == null)
            {
                return T("New folder").Text;
            }

            return T("Folder - {0}", folder.Record.Title).Text;
        }

        public bool CanApply(IContent content)
        {
            return content.As<FolderPart>() != null;
        }
    }
}