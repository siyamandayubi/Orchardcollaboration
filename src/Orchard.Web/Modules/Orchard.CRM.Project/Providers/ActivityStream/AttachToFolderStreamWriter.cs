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