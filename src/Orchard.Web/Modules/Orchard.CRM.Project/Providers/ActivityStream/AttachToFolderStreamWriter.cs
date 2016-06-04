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

namespace Orchard.CRM.Project.Providers.ActivityStream
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Records;
    using Orchard.CRM.Core.Models;
    using Orchard.CRM.Core.Providers.ActivityStream;
    using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
    using Orchard.CRM.Core.Services;
    using Orchard.CRM.Core.Settings;
    using Orchard.CRM.Project.Models;
    using Orchard.CRM.Project.Services;
    using Orchard.Localization;
    using System.Collections.Generic;
    using System.Dynamic;
    
    public class AttachToFolderStreamWriter : IActivityStreamWriter
    {
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;
        private readonly IFolderService folderService;

        public AttachToFolderStreamWriter(IContentItemDescriptorManager contentItemDescriptorManager, IFolderService folderService)
        {
            this.folderService = folderService;
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            var attachToFolder = context.ContentItem.As<AttachToFolderPart>();
            bool result = attachToFolder != null && (context.Snapshot != null);

            return result;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                yield break;
            }

            var attachToFolderPart = context.ContentItem.As<AttachToFolderPart>();

            FolderPartRecord newFolder = attachToFolderPart.Record.Folder;
            FolderPartRecord oldFolder = context.Snapshot != null ? context.Snapshot.Folder : null;
            if (oldFolder == null && newFolder == null)
            {
                yield break;
            }
            else if (oldFolder == null && newFolder != null)
            {
                newFolder = this.folderService.GetFolder(newFolder.Id).As<FolderPart>().Record;
                string change = T("{0} is added to the folder '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), newFolder.Title).Text;
                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldFolder != null && newFolder == null)
            {
                oldFolder = this.folderService.GetFolder(oldFolder.Id).As<FolderPart>().Record;
                string change = T("{0} is detached from the folder '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), oldFolder.Title).Text;
                yield return new ActivityStreamChangeItem(change);
            }
            else if (oldFolder != null && newFolder != null && oldFolder.Id != newFolder.Id)
            {
                var oldFolderContentItem = this.folderService.GetFolder(oldFolder.Id);
                var newFolderContentItem = this.folderService.GetFolder(newFolder.Id);

                string change = T("{0} is detached from the folder '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), oldFolderContentItem.Record.Title).Text;
                yield return new ActivityStreamChangeItem(change, true, oldFolderContentItem.ContentItem.Id, oldFolderContentItem.ContentItem.VersionRecord.Id);

                change = T("{0} is added to the folder '{1}'", contentItemDescriptorManager.GetDescription(context.ContentItem), newFolderContentItem.Record.Title).Text;
                yield return new ActivityStreamChangeItem(change, true, newFolderContentItem.ContentItem.Id, newFolderContentItem.ContentItem.VersionRecord.Id);
            }
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            return null;
        }

        public string Name
        {
            get { return "AttachToFolder"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<AttachToFolderPart>();

            if (part == null)
            {
                return null;
            }

            dynamic oldData = new ExpandoObject();
            var oldDataDictionary = oldData as IDictionary<string, object>;

            if (part.Record.Folder != null)
            {
                var folder = this.folderService.GetFolder(part.Record.Folder.Id);
                oldData.Folder = new FolderPartRecord
                {
                    Id = folder.Record.Id,
                    Title = folder.Record.Title,
                    ContentItemRecord = new ContentItemRecord { Id = folder.Record.ContentItemRecord.Id }
                };
            }
            else
            {
                oldData.Folder = null;
            }

            oldData.VersionId = contentItem.VersionRecord.Id;
            oldData.ContentItemId = part.ContentItem.Id;
            return oldData;
        }
    }
}