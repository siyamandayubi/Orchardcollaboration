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