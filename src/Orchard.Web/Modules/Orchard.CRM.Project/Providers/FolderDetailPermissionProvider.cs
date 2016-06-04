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
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.PermissionProviders;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Project.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.Project.Providers
{
    public class FolderDetailPermissionProvider : IMasterDetailPermissionProvider
    {
        private readonly IContentManager contentManager;
        private readonly ICRMContentOwnershipService crmContentOwnershipService;
        private readonly IContentOwnershipHelper contentOwnershipHelper;
        protected IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository;

        public FolderDetailPermissionProvider(
            IRepository<ContentItemPermissionDetailRecord> permissionDetailRecordRepository,
            IContentManager contentManager,
            ICRMContentOwnershipService crmContentOwnershipService,
            IContentOwnershipHelper contentOwnershipHelper)
        {
            this.permissionDetailRecordRepository = permissionDetailRecordRepository;
            this.contentOwnershipHelper = contentOwnershipHelper;
            this.crmContentOwnershipService = crmContentOwnershipService;
            this.contentManager = contentManager;
        }

        public bool HasDetail(IContent content)
        {
            return content.ContentItem.ContentType == ContentTypes.FolderContentType && content.As<ContentItemPermissionPart>() != null;
        }

        public void DeleteChildrenPermissions(IContent content, ContentItemPermissionDetailRecord permissionRecord)
        {
            if (!this.HasDetail(content))
            {
                return;
            }

            // get all items attached to folder
            var subItems = this.contentManager
                .Query<AttachToFolderPart>()
                .Where<AttachToFolderPartRecord>(c => c.Folder.Id == content.Id)
                .List()
                .Where(c => c.ContentItem.As<ContentItemPermissionPart>() != null)
                .Select(c => c.ContentItem).ToList();

            foreach (var item in subItems)
            {
                var contentPermissionPart = item.As<ContentItemPermissionPart>();
                var allPermissionRecords = contentPermissionPart.Record.Items ?? new List<ContentItemPermissionDetailRecord>();

                ContentItemPermissionDetailRecord targetRecord = null;

                if (permissionRecord.User != null)
                {
                    targetRecord = allPermissionRecords.FirstOrDefault(c => c.User != null && c.User.Id == permissionRecord.User.Id);
                }
                else if (permissionRecord.BusinessUnit != null)
                {
                    targetRecord = allPermissionRecords.FirstOrDefault(c => c.BusinessUnit != null && c.BusinessUnit.Id == permissionRecord.BusinessUnit.Id);
                }

                if (targetRecord != null)
                {
                    this.permissionDetailRecordRepository.Delete(targetRecord);
                }
            }

            this.permissionDetailRecordRepository.Flush();
        }

        public void GrantPermissionToDetail(EditContentPermissionViewModel parameters, IContent content)
        {
            if (!this.HasDetail(content))
            {
                return;
            }

            // get all items attached to project
            var subItems = this.contentManager
                .Query<AttachToFolderPart>()
                .Where<AttachToFolderPartRecord>(c => c.Folder.Id == content.Id)
                .List()
                .Where(c => c.ContentItem.As<ContentItemPermissionPart>() != null)
                .Select(c => c.ContentItem).ToList();

            parameters.ContentIds = subItems.Select(c => c.Id).ToArray();
            this.contentOwnershipHelper.Update(parameters, subItems, false);
        }
    }
}