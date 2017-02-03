using Orchard.ContentManagement;

namespace Orchard.CRM.Dashboard.Models
{
    public class CoverWidgetPart : ContentPart
    {
        /// <summary>
        /// Id of the content that the sidebar hosting that
        /// </summary>
        public int TargetContentItemId
        {
            get { return this.Retrieve(x => x.TargetContentItemId); }
            set { this.Store(x => x.TargetContentItemId, value); }
        }

        /// <summary>
        /// Type of the content that the sidebar hosting that
        /// </summary>
        public string ContentType
        {
            get { return this.Retrieve(x => x.ContentType); }
            set { this.Store(x => x.ContentType, value); }
        }

        /// <summary>
        /// If false, then it loads the Target ContentItem in async mode
        /// </summary>
        public bool LoadSync
        {
            get { return this.Retrieve(x => x.LoadSync); }
            set { this.Store(x => x.LoadSync, value); }
        }

        // If true, then the edit link will not be displayed for the 
        public bool HideEditLinkInFrontendLoadSync
        {
            get { return this.Retrieve(x => x.HideEditLinkInFrontendLoadSync); }
            set { this.Store(x => x.HideEditLinkInFrontendLoadSync, value); }
        }
    }
}