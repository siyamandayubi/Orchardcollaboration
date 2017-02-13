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