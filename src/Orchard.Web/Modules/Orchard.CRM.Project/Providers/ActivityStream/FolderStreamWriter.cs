using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Providers.ActivityStream;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using System.Web;
using System.Globalization;
using Orchard.CRM.Core.Providers.ActivityStream.Descriptors;
using System.Web.Routing;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Project.Models;
using Orchard.CRM.Project.Services;

namespace Orchard.CRM.Project.Providers.ActivityStream
{
    public class FolderStreamWriter : IActivityStreamWriter
    {
        private readonly IOrchardServices services;
        private readonly IFolderService folderService;
        private readonly IContentItemDescriptorManager contentItemDescriptorManager;

        public FolderStreamWriter(IOrchardServices services, IContentItemDescriptorManager contentItemDescriptorManager, IFolderService folderService)
        {
            this.folderService = folderService;
            this.contentItemDescriptorManager = contentItemDescriptorManager;
            this.services = services;
            this.T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanApply(ActiviyStreamWriterContext context)
        {
            return context.ContentItem.As<FolderPart>() != null;
        }

        public IEnumerable<ActivityStreamChangeItem> GetChangesDescriptions(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            List<string> changes = new List<string>();

            FolderPartRecord old = context.Snapshot != null ? (context.Snapshot as FolderPartRecord) : null;
            FolderPartRecord newValue = (context.ContentItem.As<FolderPart>()).Record;

            if (old == null)
            {
                 return null;
            }

            // change Title
            if (old.Title != newValue.Title)
            {
                string newTitleString = !string.IsNullOrEmpty(newValue.Title) ? newValue.Title : this.T("Empty").Text;
                newTitleString = string.Format(
                    CultureInfo.CurrentUICulture,
                    T("changed the Title to: '{0}'").Text,
                    newTitleString);

                changes.Add(newTitleString);
            }

            // change Parent
            if (old.Parent_Id != null && newValue.Parent_Id == null)
            {
                changes.Add(T("changes the folder to a root folder").Text);
            }
            else if (old.Parent_Id == null && newValue.Parent_Id != null)
            {
                var parentFolder = this.folderService.GetFolder(newValue.Parent_Id.Value);
                changes.Add(T("move the folder to {0}", parentFolder.Record.Title).Text);
            }
            else if (old.Parent_Id != null && newValue.Parent_Id != null && old.Parent_Id != newValue.Parent_Id)
            {
                var parentFolder = this.folderService.GetFolder(newValue.Parent_Id.Value);
                changes.Add(T("move the folder to {0}", parentFolder.Record.Title).Text);
            }

            return changes.Select(c => new ActivityStreamChangeItem(c));
        }

        public ActivityStreamContentDescription GetContentDescription(ActiviyStreamWriterContext context)
        {
            if (!this.CanApply(context))
            {
                return null;
            }

            var folderPart = context.ContentItem.As<FolderPart>();

            string folderDescription = contentItemDescriptorManager.GetDescription(context.ContentItem);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("action", "Display");
            routeValueDictionary.Add("controller", "Folder");
            routeValueDictionary.Add("area", "Orchard.CRM.Project");
            routeValueDictionary.Add("id", context.ContentItem.Id);

            ActivityStreamContentDescription returnValue = new ActivityStreamContentDescription(StreamWriters.FolderStreamWriter) { Weight = 1, RouteValues = routeValueDictionary };

            var originalFolder = this.folderService.GetFolder(folderPart.Record.Id);

            // new folder
            if (originalFolder == null)
            {
                returnValue.Description = T("Creates new folder").Text;
            }
            else if (context.Snapshot == null)
            {
                returnValue.Description = T("Creates the '{0}'", folderDescription).Text;
            }
            else
            {
                returnValue.Description = string.Format("{0} {1}", this.T("on Folder").Text, folderDescription);
            }

            return returnValue;
        }

        public string Name
        {
            get { return "Folder"; }
        }

        public dynamic TakeSnapshot(ContentItem contentItem)
        {
            var part = contentItem.As<FolderPart>();

            if (part == null)
            {
                return null;
            }

            FolderPartRecord oldData = new FolderPartRecord();
            oldData.Title = part.Record.Title;
            oldData.Parent_Id = part.Record.Parent_Id;

            return oldData;
        }
    }
}